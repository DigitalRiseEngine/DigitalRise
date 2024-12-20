// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRise.Geometry.Shapes;
using System;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Ray = DigitalRise.Geometry.Shapes.Ray;

namespace DigitalRise.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="RayShape"/> vs. 
  /// <see cref="TriangleShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class RayTriangleAlgorithm : CollisionAlgorithm
  {
    private readonly Gjk _gjk;


    /// <summary>
    /// Initializes a new instance of the <see cref="RayTriangleAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public RayTriangleAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      _gjk = new Gjk(collisionDetection);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="RayShape"/> and a 
    /// <see cref="Triangle"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Object A should be the ray.
      // Object B should be the triangle.
      IGeometricObject rayObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject triangleObject = contactSet.ObjectB.GeometricObject;

      // Swap if necessary.
      bool swapped = (triangleObject.Shape is RayShape);
      if (swapped)
        Mathematics.MathHelper.Swap(ref rayObject, ref triangleObject);

      RayShape rayShape = rayObject.Shape as RayShape;
      TriangleShape triangleShape = triangleObject.Shape as TriangleShape;

      // Check if shapes are correct.
      if (rayShape == null || triangleShape == null)
        throw new ArgumentException("The contact set must contain a ray and a triangle.", "contactSet");

      // See SOLID and Bergen: "Collision Detection in Interactive 3D Environments", pp. 84.
      // Note: All computations are done in triangle local space.

      // Get transformations.
      Vector3 rayScale = rayObject.Scale;
      Vector3 triangleScale = triangleObject.Scale;
      Pose rayPose = rayObject.Pose;
      Pose trianglePose = triangleObject.Pose;

      // Scale triangle.
      Vector3 v0 = triangleShape.Vertex0 * triangleScale;
      Vector3 v1 = triangleShape.Vertex1 * triangleScale;
      Vector3 v2 = triangleShape.Vertex2 * triangleScale;

      // Scale ray and transform ray to local space of triangle.
      Ray rayWorld = new Ray(rayShape);
      rayWorld.Scale(ref rayScale);  // Scale ray.
      rayWorld.ToWorld(ref rayPose); // Transform to world space.
      Ray ray = rayWorld;
      ray.ToLocal(ref trianglePose); // Transform to local space of triangle.
           
      Vector3 d1 = (v1 - v0);
      Vector3 d2 = (v2 - v0);
      Vector3 n = Vector3.Cross(d1, d2);
      
      // Tolerance value, see SOLID, Bergen: "Collision Detection in Interactive 3D Environments".
      float ε = n.Length() * Numeric.EpsilonFSquared;

      Vector3 r = ray.Direction * ray.Length;

      float δ = -Vector3.Dot(r, n);

      if (ε == 0.0f || Numeric.IsZero(δ, ε))
      {
        // The triangle is degenerate or the ray is parallel to triangle. 
        if (type == CollisionQueryType.Contacts || type == CollisionQueryType.Boolean)
          contactSet.HaveContact = false;
        else if (type == CollisionQueryType.ClosestPoints)
          GetClosestPoints(contactSet, rayWorld.Origin);

        return;
      }

      Vector3 triangleToRayOrigin = ray.Origin - v0;
      float λ = Vector3.Dot(triangleToRayOrigin, n) / δ;

      // Assume no contact.
      contactSet.HaveContact = false;

      if (λ < 0 || λ > 1)
      {
        // The ray does not hit.
        if (type == CollisionQueryType.ClosestPoints)
          GetClosestPoints(contactSet, rayWorld.Origin);
      }
      else
      {
        Vector3 u = Vector3.Cross(triangleToRayOrigin, r);
        float μ1 = Vector3.Dot(d2, u) / δ;
        float μ2 = Vector3.Dot(-d1, u) / δ;
        if (μ1 + μ2 <= 1 + ε && μ1 >= -ε && μ2 >= -ε)
        {
          // Hit!
          contactSet.HaveContact = true;

          if (type == CollisionQueryType.Boolean)
            return;

          float penetrationDepth = λ * ray.Length;

          // Create contact info.
          Vector3 position = rayWorld.Origin + rayWorld.Direction * penetrationDepth;
          n = trianglePose.ToWorldDirection(n);

          Debug.Assert(!n.IsNumericallyZero(), "Degenerate cases of ray vs. triangle should be treated above.");
          n.Normalize();

          if (δ > 0)
            n = -n;

          if (swapped)
            n = -n;

          Contact contact = ContactHelper.CreateContact(contactSet, position, n, penetrationDepth, true);
          ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
        }
        else
        {
          // No Hit!
          if (type == CollisionQueryType.ClosestPoints)
            GetClosestPoints(contactSet, rayWorld.Origin);
        }
      }
    }


    // Call GJK vs. triangle for closest points queries.
    private void GetClosestPoints(ContactSet contactSet, Vector3 rayOriginWorld)
    {
      _gjk.ComputeCollision(contactSet, CollisionQueryType.ClosestPoints);

      // GJK did not treat new contacts as ray contacts, so we have to post-process
      // the contacts where IsRayHit is not set. 
      // Contact occurs only for degenerate cases.
      if (contactSet.HaveContact)
      {
        int numberOfContacts = contactSet.Count;
        for (int i = 0; i < numberOfContacts; i++)
        {
          Contact contact = contactSet[i];
          if (!contact.IsRayHit && contact.PenetrationDepth >= 0)
          {
            // GJK reported a hit. Correct penetration depth.
            contact.PenetrationDepth = (contact.Position - rayOriginWorld).Length();
            contact.IsRayHit = true;
          }
        }
      }
    }
  }
}
