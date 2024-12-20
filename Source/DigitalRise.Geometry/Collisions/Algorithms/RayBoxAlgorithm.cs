// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRise.Geometry.Shapes;
using System;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;
using Ray = DigitalRise.Geometry.Shapes.Ray;

namespace DigitalRise.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="RayShape"/> vs. 
  /// <see cref="BoxShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class RayBoxAlgorithm : CollisionAlgorithm
  {
    private readonly Gjk _gjk; // A cached GJK instance.


    /// <summary>
    /// Initializes a new instance of the <see cref="RayBoxAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public RayBoxAlgorithm(CollisionDetection collisionDetection) 
      : base(collisionDetection)
    {
      _gjk = new Gjk(collisionDetection);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="RayShape"/> and a 
    /// <see cref="BoxShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Ray vs. box has at max 1 contact.
      Debug.Assert(contactSet.Count <= 1);

      // Object A should be the ray.
      // Object B should be the box.
      IGeometricObject rayObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject boxObject = contactSet.ObjectB.GeometricObject;

      // Swap objects if necessary.
      bool swapped = (boxObject.Shape is RayShape);
      if (swapped)
        Mathematics.MathHelper.Swap(ref rayObject, ref boxObject);

      RayShape rayShape = rayObject.Shape as RayShape;
      BoxShape boxShape = boxObject.Shape as BoxShape;

      // Check if shapes are correct.
      if (rayShape == null || boxShape == null)
        throw new ArgumentException("The contact set must contain a ray and a box.", "contactSet");

      // Call line segment vs. box for closest points queries.
      if (type == CollisionQueryType.ClosestPoints)
      {
        // Find point on ray closest to the box.

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
      Vector3 boxScale = MathHelper.Absolute(boxObject.Scale);
      Pose rayPose = rayObject.Pose;
      Pose boxPose = boxObject.Pose;

      // Apply scale to box.
      Vector3 boxExtent = boxShape.Extent * boxScale;

      // See SOLID and Bergen: "Collision Detection in Interactive 3D Environments", p. 75.
      // Note: Compute in box local space.

      // Apply scale to ray and transform to local space of box.
      Ray rayWorld = new Ray(rayShape);
      rayWorld.Scale(ref rayScale);       // Apply scale to ray.
      rayWorld.ToWorld(ref rayPose);      // Transform ray to world space.
      Ray ray = rayWorld;
      ray.ToLocal(ref boxPose);           // Transform ray from world to local space.

      uint startOutcode = GeometryHelper.GetOutcode(boxExtent, ray.Origin);
      uint endOutcode = GeometryHelper.GetOutcode(boxExtent, ray.Origin + ray.Direction * ray.Length);

      if ((startOutcode & endOutcode) != 0)
      {
        // A face of the box is a separating plane.
        return; 
      }

      // Assertion: The ray can intersect with the box but may not...
      float λEnter = 0;                         // ray parameter where ray enters box
      float λExit = 1;                          // ray parameter where ray exits box
      uint bit = 1;
      Vector3 r = ray.Direction * ray.Length;  // ray vector
      Vector3 halfExtent = 0.5f * boxExtent;   // Box half-extent vector.
      Vector3 normal = Vector3.Zero;          // normal vector
      for (int i = 0; i < 3; i++)
      {
        if ((startOutcode & bit) != 0)
        {
          // Intersection is an entering point.
          float λ = (-ray.Origin.GetComponentByIndex(i) - halfExtent.GetComponentByIndex(i)) / r.GetComponentByIndex(i);
          if (λEnter < λ)
          {   
            λEnter = λ;
            normal = new Vector3();
            normal.SetComponentByIndex(i, 1);
          }
        }
        else if ((endOutcode & bit) != 0)
        {
          // Intersection is an exciting point.
          float λ = (-ray.Origin.GetComponentByIndex(i) - halfExtent.GetComponentByIndex(i)) / r.GetComponentByIndex(i);
          if (λExit > λ)
            λExit = λ;
        }
        bit <<= 1;
        if ((startOutcode & bit) != 0)
        {
          // Intersection is an entering point.
          float λ = (-ray.Origin.GetComponentByIndex(i) + halfExtent.GetComponentByIndex(i)) / r.GetComponentByIndex(i);
          if (λEnter < λ)
          {   
            λEnter = λ;
            normal = new Vector3();
            normal.SetComponentByIndex(i, -1);
          }
        }
        else if ((endOutcode & bit) != 0)
        {
          // Intersection is an exciting point.
          float λ = (-ray.Origin.GetComponentByIndex(i) + halfExtent.GetComponentByIndex(i)) / r.GetComponentByIndex(i);
          if (λExit > λ)
            λExit = λ;
        }
        bit <<= 1;
      }

      if (λEnter <= λExit)
      {
        // The ray intersects the box.
        contactSet.HaveContact = true;
        
        if (type == CollisionQueryType.Boolean)
          return;

        float penetrationDepth = λEnter * ray.Length;
        
        if (normal == Vector3.Zero)
          normal = Vector3.UnitX;

        // Create contact info.
        Vector3 position = rayWorld.Origin + rayWorld.Direction * penetrationDepth;
        normal = boxPose.ToWorldDirection(normal);
        if (swapped)
          normal = -normal;

        // Update contact set.
        Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, true);
        ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
      }
    }
  }
}
