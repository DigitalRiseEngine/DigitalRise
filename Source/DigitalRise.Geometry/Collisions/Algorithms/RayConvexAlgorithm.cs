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
  /// <see cref="ConvexShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class RayConvexAlgorithm : CollisionAlgorithm
  {
    private const int MaxNumberOfIterations = 32;
    private readonly Gjk _gjk;


    /// <summary>
    /// Initializes a new instance of the <see cref="RayConvexAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public RayConvexAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      _gjk = new Gjk(CollisionDetection);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="RayShape"/> and a 
    /// <see cref="ConvexShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Ray vs. convex has at max 1 contact.
      Debug.Assert(contactSet.Count <= 1);

      // Object A should be the ray.
      // Object B should be the convex.
      IGeometricObject rayObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject convexObject = contactSet.ObjectB.GeometricObject;

      // Swap object if necessary.
      bool swapped = (convexObject.Shape is RayShape);
      if (swapped)
        Mathematics.MathHelper.Swap(ref rayObject, ref convexObject);

      RayShape rayShape = rayObject.Shape as RayShape;
      ConvexShape convexShape = convexObject.Shape as ConvexShape;

      // Check if shapes are correct.
      if (rayShape == null || convexShape == null)
        throw new ArgumentException("The contact set must contain a ray and a convex shape.", "contactSet");

      // Call line segment vs. convex for closest points queries.
      if (type == CollisionQueryType.ClosestPoints)
      {
        // Find point on ray closest to the convex shape.

        // Call GJK.
        _gjk.ComputeCollision(contactSet, type);
        if (contactSet.HaveContact == false)
          return;

        // Otherwise compute 1 contact ...
        // GJK result is invalid for penetration.
        foreach (var contact in contactSet)
          contact.Recycle();

        contactSet.Clear();
      }

      // Assume no contact.
      contactSet.HaveContact = false;

      // Get transformations.
      Vector3 rayScale = rayObject.Scale;
      Vector3 convexScale = convexObject.Scale;
      Pose convexPose = convexObject.Pose;
      Pose rayPose = rayObject.Pose;

      // See Raycasting paper of van den Bergen or Bullet.
      // Note: Compute in local space of convex (object B).

      // Scale ray and transform ray to local space of convex.
      Ray rayWorld = new Ray(rayShape);
      rayWorld.Scale(ref rayScale);     // Scale ray.
      rayWorld.ToWorld(ref rayPose);    // Transform ray to world space.
      Ray ray = rayWorld;
      ray.ToLocal(ref convexPose);      // Transform ray to local space of convex.

      var simplex = GjkSimplexSolver.Create();
      try
      {
        Vector3 s = ray.Origin;                  // source
        Vector3 r = ray.Direction * ray.Length;  // ray
        float λ = 0;                              // ray parameter
        Vector3 x = s;                           // hit spot (on ray)
        Vector3 n = new Vector3();              // normal
        Vector3 v = x - convexShape.GetSupportPoint(ray.Direction, convexScale);
                                                  // v = x - arbitrary point. Vector used for support mapping.
        float distanceSquared = v.LengthSquared();  // ||v||²
        int iterationCount = 0;

        while (distanceSquared > Numeric.EpsilonF && iterationCount < MaxNumberOfIterations)
        {
          iterationCount++;
          Vector3 p = convexShape.GetSupportPoint(v, convexScale); // point on convex
          Vector3 w = x - p;                                       // simplex/Minkowski difference point

          float vDotW = Vector3.Dot(v, w);       // v∙w
          if (vDotW > 0)
          {
            float vDotR = Vector3.Dot(v, r);     // v∙r
            if (vDotR >= 0)                       // TODO: vDotR >= - Epsilon^2 ?
              return;                             // No Hit. 

            λ = λ - vDotW / vDotR;
            x = s + λ * r;
            simplex.Clear(); // Configuration space obstacle (CSO) is translated whenever x is updated.
            w = x - p;
            n = v;
          }

          simplex.Add(w, x, p);
          simplex.Update();
          v = simplex.ClosestPoint;
          distanceSquared = (simplex.IsValid && !simplex.IsFull) ? v.LengthSquared() : 0;
        }

        // We have a contact if the hit is inside the ray length.
        contactSet.HaveContact = (0 <= λ && λ <= 1);

        if (type == CollisionQueryType.Boolean || (type == CollisionQueryType.Contacts && !contactSet.HaveContact))
        {
          // HaveContact queries can exit here.
          // GetContacts queries can exit here if we don't have a contact.
          return;
        }

        float penetrationDepth = λ * ray.Length;

        Debug.Assert(contactSet.HaveContact, "Separation was not detected by GJK above.");

        // Convert back to world space.
        Vector3 position = rayWorld.Origin + rayWorld.Direction * penetrationDepth;
        n = convexPose.ToWorldDirection(n);
        if (!n.TryNormalize())
          n = Vector3.UnitY;

        if (swapped)
          n = -n;

        // Update contact set.
        Contact contact = ContactHelper.CreateContact(contactSet, position, -n, penetrationDepth, true);
        ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
      }
      finally
      {
        simplex.Recycle();
      }
    }
  }
}
