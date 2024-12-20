// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;

namespace DigitalRise.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// The Gilbert-Johnson-Keerthi (GJK) algorithm for computing closest points of convex objects.
  /// </summary>
  /// <remarks>
  /// The GJK algorithm cannot compute contacts of penetrating objects. Both shapes must implement a 
  /// support mapping. That means this algorithm can only be called for shapes derived from 
  /// <see cref="ConvexShape"/>s. It will throw an exception if it is called for other objects, for 
  /// example a <see cref="PlaneShape"/>.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class Gjk : CollisionAlgorithm
  {
    private const int MaxNumberOfIterations = 64;


    /// <summary>
    /// Initializes a new instance of the <see cref="Gjk"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public Gjk(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain two <see cref="ConvexShape"/>s.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <paramref name="type"/> is set to <see cref="CollisionQueryType.Contacts"/>. This collision 
    /// algorithm cannot handle contact queries. Use <see cref="MinkowskiPortalRefinement"/> 
    /// instead.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      if (type == CollisionQueryType.Contacts)
        throw new GeometryException("GJK cannot handle contact queries. Use MPR instead.");

      IGeometricObject objectA = contactSet.ObjectA.GeometricObject;
      ConvexShape shapeA = objectA.Shape as ConvexShape;
      Vector3 scaleA = objectA.Scale;
      Pose poseA = objectA.Pose;

      IGeometricObject objectB = contactSet.ObjectB.GeometricObject;
      ConvexShape shapeB = objectB.Shape as ConvexShape;
      Vector3 scaleB = objectB.Scale;
      Pose poseB = objectB.Pose;

      if (shapeA == null || shapeB == null)
        throw new ArgumentException("The contact set must contain two convex shapes.", "contactSet");

      // GJK builds a simplex of the CSO (A-B). This simplex is managed in a GjkSimplexSolver.
      var simplex = GjkSimplexSolver.Create();
      bool foundSeparatingAxis = false;
      try
      {
        // v is the separating axis or the CSO point nearest to the origin.
        // We start with last known separating axis or with an arbitrary CSO point.
        Vector3 v;
        if (contactSet.Count > 0)
        {
          // Use last separating axis.
          // The contact normal points from A to B. This is the direction we want to sample first.
          // If the frame-to-frame coherence is high we should get a point close to the origin.
          // Note: To sample in the normal direction, we need to initialize the CSO point v with
          // -normal.
          v = -contactSet[0].Normal;
        }
        else
        {
          // Use difference of inner points.
          Vector3 vA = poseA.ToWorldPosition(shapeA.InnerPoint * scaleA);
          Vector3 vB = poseB.ToWorldPosition(shapeB.InnerPoint * scaleB);
          v = vA - vB;
        }

        // If the inner points overlap, then we have already found a contact. 
        // We don't expect this case to happen often, so we simply choose an arbitrary separating
        // axis to continue with the normal GJK code.
        if (v.IsNumericallyZero())
          v = Vector3.UnitZ;

        // Cache inverted rotations.
        var orientationAInverse = poseA.Orientation.Transposed;
        var orientationBInverse = poseB.Orientation.Transposed;

        int iterationCount = 0;
        float distanceSquared = float.MaxValue;
        float distanceEpsilon;

        // Assume we have no contact. 
        contactSet.HaveContact = false;

        do
        {
          // TODO: Translate A and B close to the origin to avoid numerical problems.
          // This optimization is done in Bullet: The offset (a.Pose.Position + b.Pose.Position) / 2
          // is subtracted from a.Pose and b.Pose. This offset is added when the Contact info is 
          // computed (also in EPA if the poses are still translated).

          // Compute a new point w on the simplex. We seek for the point that is closest to the origin.
          // Therefore, we get the support points on the current separating axis v.
          Vector3 p = poseA.ToWorldPosition(shapeA.GetSupportPoint(orientationAInverse * -v, scaleA));
          Vector3 q = poseB.ToWorldPosition(shapeB.GetSupportPoint(orientationBInverse * v, scaleB));
          Vector3 w = p - q;

          // w projected onto the separating axis.
          float delta = Vector3.Dot(w, v);

          // If v∙w > 0 then the objects do not overlap.
          if (delta > 0)
          {
            // We have found a separating axis. 
            foundSeparatingAxis = true;

            // Early exit for boolean and contact queries.
            if (type == CollisionQueryType.Boolean || type == CollisionQueryType.Contacts)
            {
              // TODO: We could cache the separating axis n in ContactSet for future collision checks.
              return;
            }

            // We continue for closest point queries because we don't know if there are other
            // points closer than p and q.
          }

          // If the new w is already part of the simplex. We cannot improve any further.
          if (simplex.Contains(w))
            break;

          // If the new w is not closer to the origin (within numerical tolerance), we stop.
          if (distanceSquared - delta <= distanceSquared * Numeric.EpsilonF) // SOLID uses Epsilon = 10^-6
            break;

          // Add the new point to the simplex. 
          simplex.Add(w, p, q);

          // Update the simplex. (Unneeded simplex points are removed).
          simplex.Update();

          // Get new point of simplex closest to the origin.
          v = simplex.ClosestPoint;

          float previousDistanceSquared = distanceSquared;
          distanceSquared = v.LengthSquared();

          if (previousDistanceSquared < distanceSquared)
          {
            // If the result got worse, we use the previous result. This happens for 
            // degenerate cases for example when the simplex is a tetrahedron with all
            // 4 vertices in a plane.
            distanceSquared = previousDistanceSquared;
            simplex.Backup();
            break;
          }

          // If the new simplex is invalid, we stop.
          // Example: A simplex gets invalid if a fourth vertex is added to create a tetrahedron 
          // simplex but all vertices are in a plane. This can happen if a box corner nearly touches a 
          // face of another box.
          if (!simplex.IsValid)
            break;

          // Compare the distance of v to the origin with the distance of the last iteration.
          // We stop if the improvement is less than the numerical tolerance.
          if (previousDistanceSquared - distanceSquared <= previousDistanceSquared * Numeric.EpsilonF)
            break;

          // If we reach the iteration limit, we stop.
          iterationCount++;
          if (iterationCount > MaxNumberOfIterations)
          {
            Debug.Assert(false, "GJK reached the iteration limit.");
            break;
          }

          // Compute a scaled epsilon.
          distanceEpsilon = Numeric.EpsilonFSquared * Math.Max(1, simplex.MaxVertexDistanceSquared);

          // Loop until the simplex is full (i.e. it contains the origin) or we have come
          // sufficiently close to the origin.
        } while (!simplex.IsFull && distanceSquared > distanceEpsilon);

        Debug.Assert(simplex.IsEmpty == false, "The GJK simplex must contain at least 1 point.");

        // Compute contact normal and separation.
        Vector3 normal = -simplex.ClosestPoint;  // simplex.ClosestPoint = ClosestPointA-ClosestPointB
        float distance;
        distanceEpsilon = Numeric.EpsilonFSquared * Math.Max(1, simplex.MaxVertexDistanceSquared);
        if (distanceSquared <= distanceEpsilon)
        {
          // Distance is approximately 0.
          // --> Objects are in contact.
          if (simplex.IsValid && normal.TryNormalize())
          {
            // Normal can be used but we have to invert it because for contact we 
            // have to compute normal as pointOnA - pointOnB.
            normal = -normal;
          }
          else
          {
            // No useful normal. Use direction between inner points as a fallback.
            Vector3 innerA = poseA.ToWorldPosition(shapeA.InnerPoint * scaleA);
            normal = simplex.ClosestPointOnA - innerA;
            if (!normal.TryNormalize())
            {
              Vector3 innerB = poseB.ToWorldPosition(shapeB.InnerPoint * scaleB);
              normal = innerB - innerA;
              if (!normal.TryNormalize())
              {
                normal = Vector3.UnitY;
                // TODO: We could use better normal: e.g. normal of old contact or PreferredNormal?
              }
            }
          }

          distance = 0;
          contactSet.HaveContact = true;
        }
        else
        {
          // Distance is greater than 0.
          distance = (float)Math.Sqrt(distanceSquared);
          normal /= distance;

          // If the simplex is valid and full, then we have a contact.
          if (simplex.IsFull && simplex.IsValid)
          {
            // Let's use the current result as an estimated contact info for
            // shallow contacts.

            // TODO: The following IF was added because this can occur for valid 
            // triangle vs. triangle separation. Check this.
            if (!foundSeparatingAxis)
            {
              contactSet.HaveContact = true;
              // Distance is a penetration depth
              distance = -distance;

              // Since the simplex tetrahedron can have any position in the Minkowsky difference,
              // we do not know the real normal. Let's use the current normal and make
              // sure that it points away from A. - This is only a heuristic...
              Vector3 innerA = poseA.ToWorldPosition(shapeA.InnerPoint * scaleA);
              if (Vector3.Dot(simplex.ClosestPointOnA - innerA, normal) < 0)
                normal = -normal;
            }
          }
        }

        Debug.Assert(normal.IsNumericallyZero() == false);

        if (type != CollisionQueryType.Boolean)
        {
          Vector3 position = (simplex.ClosestPointOnA + simplex.ClosestPointOnB) / 2;
          Contact contact = ContactHelper.CreateContact(contactSet, position, normal, -distance, false);
          ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
        }
      }
      finally
      {
        simplex.Recycle();
      }
    }
  }
}
