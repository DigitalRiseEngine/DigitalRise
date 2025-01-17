// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="TriangleShape"/> vs.
  /// <see cref="TriangleShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class TriangleTriangleAlgorithm : CollisionAlgorithm
  {
    // Notes:
    // Edge indices: Edge 0 goes from vertex 0 to 1, etc.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // The separation axis projection info of a single triangle.
    private struct Configuration
    {
      public int Index0; // The index of the vertex with the smallest projected distance.
      public int Index1;
      public int Index2; // The index of the vertex with the largest projected distance.
      public float Min; // The min projection distance (projection of Triangle[Index0]).
      public float Mid; // The min projection distance (projection of Triangle[Index1]).
      public float Max; // The max projection distance (projection of triangle[Index2])
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    //public static bool UseMpr = false;
    //private MinkowskiPortalRefinement _mpr;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleTriangleAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public TriangleTriangleAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain two <see cref="TriangleShape"/>s.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <paramref name="type"/> is set to <see cref="CollisionQueryType.ClosestPoints"/>. This
    /// collision algorithm cannot handle closest-point queries. Use <see cref="Gjk"/> instead.
    /// </exception>
    [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Invoke GJK for closest points.
      if (type == CollisionQueryType.ClosestPoints)
        throw new GeometryException("This collision algorithm cannot handle closest-point queries. Use GJK instead.");

      //if (UseMpr)
      //{
      //  if (_mpr == null)
      //    _mpr = new MinkowskiPortalRefinement(CollisionDetection);

      //  _mpr.ComputeCollision(contactSet, type);
      //  return;
      //}

      CollisionObject collisionObjectA = contactSet.ObjectA;
      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      TriangleShape triangleShapeA = geometricObjectA.Shape as TriangleShape;
      TriangleShape triangleShapeB = geometricObjectB.Shape as TriangleShape;

      // Check if collision objects shapes are correct.
      if (triangleShapeA == null || triangleShapeB == null)
        throw new ArgumentException("The contact set must contain triangle shapes.", "contactSet");

      Vector3 scaleA = MathHelper.Absolute(geometricObjectA.Scale);
      Vector3 scaleB = MathHelper.Absolute(geometricObjectB.Scale);
      Pose poseA = geometricObjectA.Pose;
      Pose poseB = geometricObjectB.Pose;

      // Get triangles in world space.
      Triangle triangleA;
      triangleA.Vertex0 = poseA.ToWorldPosition(triangleShapeA.Vertex0 * scaleA);
      triangleA.Vertex1 = poseA.ToWorldPosition(triangleShapeA.Vertex1 * scaleA);
      triangleA.Vertex2 = poseA.ToWorldPosition(triangleShapeA.Vertex2 * scaleA);
      Triangle triangleB;
      triangleB.Vertex0 = poseB.ToWorldPosition(triangleShapeB.Vertex0 * scaleB);
      triangleB.Vertex1 = poseB.ToWorldPosition(triangleShapeB.Vertex1 * scaleB);
      triangleB.Vertex2 = poseB.ToWorldPosition(triangleShapeB.Vertex2 * scaleB);

      if (type == CollisionQueryType.Boolean)
      {
        contactSet.HaveContact = GeometryHelper.HaveContact(ref triangleA, ref triangleB);
        return;
      }

      Debug.Assert(type == CollisionQueryType.Contacts, "TriangleTriangleAlgorithm cannot handle closest point queries.");

      // Assume no contact.
      contactSet.HaveContact = false;

      Vector3 position, normal;
      float penetrationDepth;
      if (!GetContact(ref triangleA, ref triangleB, false, false, out position, out normal, out penetrationDepth))
        return;

      contactSet.HaveContact = true;

      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
    }


    // Used in unit test.
    internal static bool GetContact(
      ref Triangle triangleA, ref Triangle triangleB,
      out Vector3 position, out Vector3 normal, out float penetrationDepth)
    {
      return GetContact(ref triangleA, ref triangleB, false, false, out position, out normal, out penetrationDepth);
    }


    // Returns true for a contact, false for a separation.
    // position, normal and penetration depth are only computed for contacts.
    // If the triangles are one sided, bad contacts are filtered. In this case we might
    // get a contact (return value true) but no contact details (penetrationDepth = NaN).
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    internal static bool GetContact(
      ref Triangle triangleA, ref Triangle triangleB, bool isOneSidedA, bool isOneSidedB,
      out Vector3 position, out Vector3 normal, out float penetrationDepth)
    {
      position = new Vector3();
      normal = new Vector3();
      penetrationDepth = float.MaxValue;
      bool parallelEdgeDetected = false;

      // Separating axis test: We have to test 2 triangle normal directions and 3x3 edge-edge
      // cross products.
      Configuration bestConfigA = new Configuration();
      Configuration bestConfigB = new Configuration();
      int bestTest = -1;
      bool invertNormal = false;
      bool invalidFaceNormal = false;

      // Get a scaled epsilon value for numerically tolerant comparisons.
      float epsilon =
        Numeric.EpsilonF
        * (1 + Math.Abs(triangleA.Vertex0.X) + Math.Abs(triangleA.Vertex0.Y) + Math.Abs(triangleA.Vertex0.Z)
             + Math.Abs(triangleA.Vertex1.X) + Math.Abs(triangleA.Vertex1.Y) + Math.Abs(triangleA.Vertex1.Z)
             + Math.Abs(triangleA.Vertex2.X) + Math.Abs(triangleA.Vertex2.Y) + Math.Abs(triangleA.Vertex2.Z)
             + Math.Abs(triangleB.Vertex0.X) + Math.Abs(triangleB.Vertex0.Y) + Math.Abs(triangleB.Vertex0.Z)
             + Math.Abs(triangleB.Vertex1.X) + Math.Abs(triangleB.Vertex1.Y) + Math.Abs(triangleB.Vertex1.Z)
             + Math.Abs(triangleB.Vertex2.X) + Math.Abs(triangleB.Vertex2.Y) + Math.Abs(triangleB.Vertex2.Z));

      // Test 0: Triangle A normal
      var normalA = Vector3.Cross(triangleA.Vertex1 - triangleA.Vertex0, triangleA.Vertex2 - triangleA.Vertex0);
      if (normalA.TryNormalize())
      {
        Configuration configA;
        var dA = Vector3.Dot(normalA, triangleA.Vertex0);
        configA.Index0 = 0;
        configA.Index1 = 1;
        configA.Index2 = 2;
        configA.Min = dA;
        configA.Mid = dA;
        configA.Max = dA;

        Configuration configB;
        GetConfiguration(ref normalA, ref triangleB, out configB);

        // Separation test.
        if (configA.Min > configB.Max || configA.Max < configB.Min)
          return false;

        float penetrationDepth1 = configA.Max - configB.Min;
        float penetrationDepth2 = configB.Max - configA.Min;
        if (penetrationDepth2 < penetrationDepth1)
        {
          penetrationDepth1 = penetrationDepth2;
          invertNormal = true;
          if (isOneSidedA)
            invalidFaceNormal = true;
        }

        bestTest = 0;
        penetrationDepth = penetrationDepth1;
        bestConfigA = configA;
        bestConfigB = configB;
        normal = normalA;
      }
      //else
      //{
      // Triangle is degenerate (point or line segment). We can treat this as separation.
      //}

      // Test 1: Triangle B normal
      var normalB = Vector3.Cross(triangleB.Vertex1 - triangleB.Vertex0, triangleB.Vertex2 - triangleB.Vertex0);
      if (normalB.TryNormalize())
      {
        Configuration configB;
        var dB = Vector3.Dot(normalB, triangleB.Vertex0);
        configB.Index0 = 0;
        configB.Index1 = 1;
        configB.Index2 = 2;
        configB.Min = dB;
        configB.Mid = dB;
        configB.Max = dB;

        Configuration configA;
        GetConfiguration(ref normalB, ref triangleA, out configA);

        if (configA.Min > configB.Max || configA.Max < configB.Min)
          return false;

        bool localInvertNormal = true;
        bool localInvalidFaceNormal = false;
        float penetrationDepth1 = configB.Max - configA.Min;
        float penetrationDepth2 = configA.Max - configB.Min;
        if (penetrationDepth2 < penetrationDepth1)
        {
          penetrationDepth1 = penetrationDepth2;
          localInvertNormal = false;
          if (isOneSidedB)
            localInvalidFaceNormal = true;
        }

        if (penetrationDepth1 < penetrationDepth)
        {
          bestTest = 1;
          penetrationDepth = penetrationDepth1;
          bestConfigA = configA;
          bestConfigB = configB;
          normal = normalB;
          invertNormal = localInvertNormal;
          invalidFaceNormal = localInvalidFaceNormal;
        }
      }
      else
      {
        // Triangle is degenerate (point or line segment).

        // No contact possible if both triangles are degenerate.
        // (We assume line vs. line does never touch.)
        if (bestTest < 0)
          return false;
      }

      // Test 2: Edge 0 vs. Edge 0
      var edgeA0 = triangleA.Vertex1 - triangleA.Vertex0;
      var edgeB0 = triangleB.Vertex1 - triangleB.Vertex0;
      var crossE0E0 = Vector3.Cross(edgeA0, edgeB0);
      if (crossE0E0.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          2, ref crossE0E0, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }
      //else
      //{
      // Edges are parallel.
      // If one edge is in the triangle plane of the other triangle, we treat this as a
      // separation. In any other case, a different separating axis will have a smaller
      // minimum translational distance.
      //}

      // Test 3: Edge 0 vs. Edge 1
      var edgeB1 = triangleB.Vertex2 - triangleB.Vertex1;
      var crossE0E1 = Vector3.Cross(edgeA0, edgeB1);
      if (crossE0E1.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          3, ref crossE0E1, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }

      // Test 4: Edge 0 vs. Edge 2
      var edgeB2 = triangleB.Vertex0 - triangleB.Vertex2;
      var crossE0E2 = Vector3.Cross(edgeA0, edgeB2);
      if (crossE0E2.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          4, ref crossE0E2, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }

      // Test 5: Edge 1 vs. Edge 0
      var edgeA1 = triangleA.Vertex2 - triangleA.Vertex1;
      var crossE1E0 = Vector3.Cross(edgeA1, edgeB0);
      if (crossE1E0.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          5, ref crossE1E0, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }

      // Test 6: Edge 1 vs. Edge 1
      var crossE1E1 = Vector3.Cross(edgeA1, edgeB1);
      if (crossE1E1.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          6, ref crossE1E1, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }


      // Test 7: Edge 1 vs. Edge 2
      var crossE1E2 = Vector3.Cross(edgeA1, edgeB2);
      if (crossE1E2.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          7, ref crossE1E2, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }

      // Test 8: Edge 2 vs. Edge 0
      var edgeA2 = triangleA.Vertex0 - triangleA.Vertex2;
      var crossE2E0 = Vector3.Cross(edgeA2, edgeB0);
      if (crossE2E0.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          8, ref crossE2E0, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }

      // Test 9: Edge 2 vs. Edge 1
      var crossE2E1 = Vector3.Cross(edgeA2, edgeB1);
      if (crossE2E1.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          9, ref crossE2E1, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }

      // Test 10: Edge 2 vs. Edge 2
      var crossE2E2 = Vector3.Cross(edgeA2, edgeB2);
      if (crossE2E2.TryNormalize())
      {
        bool isSeparated = TestEdgeEdgeAxis(
          10, ref crossE2E2, ref triangleA, ref triangleB, epsilon,
          ref invertNormal, ref penetrationDepth, ref bestTest, ref parallelEdgeDetected, ref normal, ref bestConfigA, ref bestConfigB);

        if (isSeparated)
          return false;
      }

      // If bestTest is still -1, then all separating axes are degenerate. That means, both
      // triangles are collapsed to line segments or points. If they are line segments,
      // they are parallel. --> Treat as no contact.
      // This case should actually never happen because we exit in the else branch of test 1 (normal B).
      if (bestTest < 0)
        return false;

      if (invalidFaceNormal)
      {
        penetrationDepth = float.NaN;
        return true;
      }

      if (bestTest == 0)
      {
        // Face A - * contact.

        Vector3 pointOnB;
        if (invertNormal)
        {
          normal = -normal;
          pointOnB = triangleB[bestConfigB.Index2];
          parallelEdgeDetected |= Numeric.AreEqual(bestConfigB.Max, bestConfigB.Mid, epsilon);
        }
        else
        {
          pointOnB = triangleB[bestConfigB.Index0];
          parallelEdgeDetected |= Numeric.AreEqual(bestConfigB.Min, bestConfigB.Mid, epsilon);
        }

        // A support point (vertex) of A should be the contact point on B. However, if
        // the point is on an edge of B which is parallel to the plane of A, then the
        // vertex might not be inside the triangle and we have to compute an edge-edge
        // contact.
        bool pointIsInside = true;
        float u = 0, v = 0, w = 0;
        if (parallelEdgeDetected)
        {
          GeometryHelper.GetBarycentricFromPoint(triangleA, pointOnB, out u, out v, out w);
          pointIsInside = (u >= 0 && v >= 0 && w >= 0);
        }

        if (pointIsInside)
        {
          position = pointOnB + normal * (penetrationDepth / 2);
        }
        else if (Numeric.AreEqual(bestConfigB.Min, bestConfigA.Min, epsilon) && Numeric.AreEqual(bestConfigB.Max, bestConfigA.Min, epsilon))
        {
          // A and B are in a plane. We must clip the faces against each other to find
          // a contact position. Note that the triangle could still be separated.
          // Since the edge cross products are zero, we have not tested any edge separations.
          if (!ClipPlanarTriangles(ref triangleA, ref triangleB, epsilon, out position))
            return false;
        }
        else
        {
          // The vertex is outside the triangle. We must compute an edge-edge contact instead.
          LineSegment segmentA;
          LineSegment segmentB = new LineSegment(pointOnB, triangleB[bestConfigB.Index1]);

          bool contactIsValid = false;
          if (u < 0)
          {
            segmentA = new LineSegment(triangleA.Vertex1, triangleA.Vertex2);
            contactIsValid = GetEdgeEdgeContact(ref segmentA, ref segmentB, epsilon, ref normal, out penetrationDepth, out position);
          }

          if (v < 0 && !contactIsValid)
          {
            segmentA = new LineSegment(triangleA.Vertex0, triangleA.Vertex2);
            contactIsValid = GetEdgeEdgeContact(ref segmentA, ref segmentB, epsilon, ref normal, out penetrationDepth, out position);
          }

          if (w < 0 && !contactIsValid)
          {
            segmentA = new LineSegment(triangleA.Vertex0, triangleA.Vertex1);
            contactIsValid = GetEdgeEdgeContact(ref segmentA, ref segmentB, epsilon, ref normal, out penetrationDepth, out position);
          }

          Debug.Assert(contactIsValid, "Edge-edge contact is outside triangle.");
        }
      }
      else if (bestTest == 1)
      {
        // * - B contact. 

        Vector3 pointOnA;
        if (invertNormal)
        {
          normal = -normal;
          pointOnA = triangleA[bestConfigA.Index0];
          parallelEdgeDetected |= Numeric.AreEqual(bestConfigA.Min, bestConfigA.Mid, epsilon);
        }
        else
        {
          pointOnA = triangleA[bestConfigA.Index2];
          parallelEdgeDetected |= Numeric.AreEqual(bestConfigA.Max, bestConfigA.Mid, epsilon);
        }

        bool pointIsInside = true;
        float u = 0, v = 0, w = 0;
        if (parallelEdgeDetected)
        {
          GeometryHelper.GetBarycentricFromPoint(triangleB, pointOnA, out u, out v, out w);
          pointIsInside = (u >= 0 && v >= 0 && w >= 0);
        }

        if (pointIsInside)
        {
          position = pointOnA - normal * (penetrationDepth / 2);
        }
        else if (Numeric.AreEqual(bestConfigA.Min, bestConfigB.Min, epsilon) && Numeric.AreEqual(bestConfigA.Max, bestConfigB.Min, epsilon))
        {
          if (!ClipPlanarTriangles(ref triangleA, ref triangleB, epsilon, out position))
            return false;
        }
        else
        {
          // The vertex is outside the triangle. We must compute an edge-edge contact instead.
          LineSegment segmentA = new LineSegment(pointOnA, triangleA[bestConfigA.Index1]);
          LineSegment segmentB;
          bool contactIsValid = false;
          if (u < 0)
          {
            segmentB = new LineSegment(triangleB.Vertex1, triangleB.Vertex2);
            contactIsValid = GetEdgeEdgeContact(ref segmentA, ref segmentB, epsilon, ref normal, out penetrationDepth, out position);
          }

          if (v < 0 && !contactIsValid)
          {
            segmentB = new LineSegment(triangleB.Vertex0, triangleB.Vertex2);
            contactIsValid = GetEdgeEdgeContact(ref segmentA, ref segmentB, epsilon, ref normal, out penetrationDepth, out position);
          }

          if (w < 0 && !contactIsValid)
          {
            segmentB = new LineSegment(triangleB.Vertex0, triangleB.Vertex1);
            contactIsValid = GetEdgeEdgeContact(ref segmentA, ref segmentB, epsilon, ref normal, out penetrationDepth, out position);
          }

          Debug.Assert(contactIsValid, "Edge-edge contact is outside triangle.");
        }
      }
      else
      {
        // Edge-edge contact.
        LineSegment segmentA, segmentB;
        GetEdgeLineSegment((bestTest - 2) / 3, ref triangleA, out segmentA);
        GetEdgeLineSegment((bestTest - 2) % 3, ref triangleB, out segmentB);

        normal = invertNormal ? -normal : normal;

        float s, t;
        GeometryHelper.GetLineParameters(segmentA, segmentB, out s, out t);
        var pointOnB = segmentB.Start + t * (segmentB.End - segmentB.Start);

        position = pointOnB + normal * (penetrationDepth / 2);  // Position is between the edges.

#if DEBUG
        var pointOnA = segmentA.Start + s * (segmentA.End - segmentA.Start);
        Debug.Assert(s >= 0 && s <= 1, "Closest points are not on edge.");
        Debug.Assert(t >= 0 && t <= 1, "Closest points are not on edge.");
        Debug.Assert(Numeric.AreEqual((pointOnA - pointOnB).Length(), penetrationDepth, epsilon), "Inconsistent edge-edge penetration depth.");
        Debug.Assert(MathHelper.AreNumericallyEqual(pointOnB + normal * penetrationDepth, pointOnA, epsilon), "Inconsistent edge-edge contact positions.");
#endif

        // Remove bad normals for one-sided triangles.
        // Note: If the TriangleMeshAlgorithm supports contact welding for mesh vs. mesh,
        // then remove these checks. Contact welding has to correct these bad normals.
        if (isOneSidedA)
        {
          if (Vector3.Dot(normalA, normal) < -epsilon)
          {
            penetrationDepth = float.NaN;
            return true;
          }
        }
        if (isOneSidedB)
        {
          if (Vector3.Dot(normalB, normal) > epsilon)
          {
            penetrationDepth = float.NaN;
            return true;
          }
        }
      }

      Debug.Assert(normal.IsNumericallyNormalized());
      return true;
    }


    // Tests an edge-edge separating axis. (Returns true if separated.)
    private static bool TestEdgeEdgeAxis(
      // In:
      int testIndex, ref Vector3 axis, ref Triangle triangleA, ref Triangle triangleB, float epsilon,
      // In/Out:
      ref bool invertNormal, ref float bestPenetrationDepth, ref int bestTest, ref bool parallelEdgeDetected,
      ref Vector3 bestAxis, ref Configuration bestConfigA, ref Configuration bestConfigB)
    {
      Configuration configA, configB;
      GetConfiguration(ref axis, ref triangleA, out configA);
      GetConfiguration(ref axis, ref triangleB, out configB);

      if (configA.Min > configB.Max || configA.Max < configB.Min)
        return true;

      bool localInvertNormal = false;
      float penetrationDepth = configA.Max - configB.Min;
      var penetrationDepth2 = configB.Max - configA.Min;
      if (penetrationDepth2 < penetrationDepth)
      {
        penetrationDepth = penetrationDepth2;
        localInvertNormal = true;
      }

      if (penetrationDepth < bestPenetrationDepth)
      {
        // Replace the face-vertex contact only if the penetration is significantly larger.
        // In doubt, we want to keep the face-vertex config because its contact computation 
        // handles the face-edge, face-vertex and edge-edge cases.
        if (bestTest > 1 || penetrationDepth + epsilon < bestPenetrationDepth)
        {
          bestTest = testIndex;
          bestPenetrationDepth = penetrationDepth;
          bestConfigA = configA;
          bestConfigB = configB;
          bestAxis = axis;
          invertNormal = localInvertNormal;
        }
        else
        {
          parallelEdgeDetected = true;
        }
      }

      return false;
    }


    // Project the triangle vertices onto the separating axis.
    private static void GetConfiguration(ref Vector3 direction, ref Triangle triangle, out Configuration config)
    {
      var dB0 = Vector3.Dot(direction, triangle.Vertex0);
      var dB1 = Vector3.Dot(direction, triangle.Vertex1);
      var dB2 = Vector3.Dot(direction, triangle.Vertex2);
      if (dB0 <= dB1) // 0 <= 1
      {
        if (dB1 <= dB2) // 0 <= 1 <= 2
        {
          config.Index0 = 0;
          config.Index1 = 1;
          config.Index2 = 2;
          config.Min = dB0;
          config.Mid = dB1;
          config.Max = dB2;
        }
        else if (dB0 <= dB2) // 0 <= 2 < 1
        {
          config.Index0 = 0;
          config.Index1 = 2;
          config.Index2 = 1;
          config.Min = dB0;
          config.Mid = dB2;
          config.Max = dB1;
        }
        else // 2 < 0 <= 1
        {
          config.Index0 = 2;
          config.Index1 = 0;
          config.Index2 = 1;
          config.Min = dB2;
          config.Mid = dB0;
          config.Max = dB1;
        }
      }
      else // 1 < 0 
      {
        if (dB2 <= dB1) // 2 <= 1 < 0
        {
          config.Index0 = 2;
          config.Index1 = 1;
          config.Index2 = 0;
          config.Min = dB2;
          config.Mid = dB1;
          config.Max = dB0;
        }
        else if (dB2 <= dB0) // 1 < 2 <= 0
        {
          config.Index0 = 1;
          config.Index1 = 2;
          config.Index2 = 0;
          config.Min = dB1;
          config.Mid = dB2;
          config.Max = dB0;
        }
        else // 1 < 0 < 2
        {
          config.Index0 = 1;
          config.Index1 = 0;
          config.Index2 = 2;

          config.Min = dB1;
          config.Mid = dB0;
          config.Max = dB2;
        }
      }
    }


    // Get an edge of a triangle.
    private static void GetEdgeLineSegment(int edgeIndex, ref Triangle triangle, out LineSegment segment)
    {
      Debug.Assert(edgeIndex >= 0);
      Debug.Assert(edgeIndex < 3);

      if (edgeIndex == 0)
      {
        segment.Start = triangle.Vertex0;
        segment.End = triangle.Vertex1;
      }
      else if (edgeIndex == 1)
      {
        segment.Start = triangle.Vertex1;
        segment.End = triangle.Vertex2;
      }
      else
      {
        segment.Start = triangle.Vertex2;
        segment.End = triangle.Vertex0;
      }
    }


    // Get a contact for two coplanar triangles.
    // Returns false if triangles do not overlap.
    private static bool ClipPlanarTriangles(ref Triangle triangleA, ref Triangle triangleB, float epsilon, out Vector3 contact)
    {
      // Test vertices of A against face of B.
      if (GeometryHelper.IsOver(ref triangleA, ref triangleB.Vertex0))
      {
        contact = triangleB.Vertex0;
        return true;
      }
      if (GeometryHelper.IsOver(ref triangleA, ref triangleB.Vertex1))
      {
        contact = triangleB.Vertex1;
        return true;
      }
      if (GeometryHelper.IsOver(ref triangleA, ref triangleB.Vertex2))
      {
        contact = triangleB.Vertex2;
        return true;
      }

      // Test vertices of B against face of A.
      if (GeometryHelper.IsOver(ref triangleB, ref triangleA.Vertex0))
      {
        contact = triangleA.Vertex0;
        return true;
      }
      if (GeometryHelper.IsOver(ref triangleB, ref triangleA.Vertex1))
      {
        contact = triangleA.Vertex1;
        return true;
      }
      if (GeometryHelper.IsOver(ref triangleB, ref triangleA.Vertex2))
      {
        contact = triangleA.Vertex2;
        return true;
      }

      // Test edges.
      contact = new Vector3();
      LineSegment eA0, eA1, eA2, eB0, eB1, eB2;
      GetEdgeLineSegment(0, ref triangleA, out eA0);
      GetEdgeLineSegment(0, ref triangleB, out eB0);
      if (TestPlanarEdgeEdge(ref eA0, ref eB0, ref contact, epsilon))
        return true;

      GetEdgeLineSegment(1, ref triangleB, out eB1);
      if (TestPlanarEdgeEdge(ref eA0, ref eB1, ref contact, epsilon))
        return true;

      GetEdgeLineSegment(2, ref triangleB, out eB2);
      if (TestPlanarEdgeEdge(ref eA0, ref eB2, ref contact, epsilon))
        return true;

      GetEdgeLineSegment(1, ref triangleA, out eA1);
      if (TestPlanarEdgeEdge(ref eA1, ref eB0, ref contact, epsilon))
        return true;

      if (TestPlanarEdgeEdge(ref eA1, ref eB1, ref contact, epsilon))
        return true;

      if (TestPlanarEdgeEdge(ref eA1, ref eB2, ref contact, epsilon))
        return true;

      GetEdgeLineSegment(2, ref triangleA, out eA2);
      if (TestPlanarEdgeEdge(ref eA2, ref eB0, ref contact, epsilon))
        return true;

      if (TestPlanarEdgeEdge(ref eA2, ref eB1, ref contact, epsilon))
        return true;

      if (TestPlanarEdgeEdge(ref eA2, ref eB2, ref contact, epsilon))
        return true;

      // Triangles are in the same plane but don't overlap.
      // I think we could also land here if the triangles overlap but all vertices
      // are slightly outside the other triangles and the edge-edge contacts are 
      // also slightly out of the segments because of numerical errors 
      // (e.g. barycentric u = -0.0000001 and all  line parameters s < -0.000001).
      // This case is rare (if it can occur at all) and since the penetration depth 
      // is 0 (coplanar triangles), we can simply interpret this case as separation.
      return false;
    }


    // Checks if the two edges in the same plane are touching.
    // If they touch, return true and the contact.
    private static bool TestPlanarEdgeEdge(ref LineSegment segmentA, ref LineSegment segmentB, ref Vector3 contact, float epsilon)
    {
      float s, t;
      GeometryHelper.GetLineParameters(segmentA, segmentB, out s, out t);
      if (s < 0 || s > 1)
        return false;
      if (t < 0 || t > 1)
        return false;

      contact = segmentA.Start + s * (segmentA.End - segmentA.Start);

      // We still have to check if the lines are parallel.
      var contactB = segmentB.Start + t * (segmentB.End - segmentB.Start);
      return MathHelper.AreNumericallyEqual(contact, contactB, epsilon);
    }


    // Compute contact from edge-edge separating axis for the special F-V case.
    // Return false if the closest point from the edges is outside the edge.
    private static bool GetEdgeEdgeContact(
      // In:
      ref LineSegment segmentA, ref LineSegment segmentB, float epsilon,
      // In/Out:
      ref Vector3 normal,
      // Out:
      out float penetrationDepth, out Vector3 position)
    {
      // Update the given input normal.
      var cross = Vector3.Cross(segmentB.End - segmentB.Start, segmentA.End - segmentA.Start);
      if (cross.TryNormalize())
      {
        // Make sure the new normal points into the same direction.
        if (Vector3.Dot(cross, normal) < 0)
          cross = -cross;
        normal = cross;
      }

      // Get closest point of edges.
      float s, t;
      GeometryHelper.GetLineParameters(segmentA, segmentB, out s, out t);
      var pointOnA = segmentA.Start + s * (segmentA.End - segmentA.Start);
      var pointOnB = segmentB.Start + t * (segmentB.End - segmentB.Start);
      penetrationDepth = (pointOnA - pointOnB).Length();

      position = pointOnB + normal * (penetrationDepth / 2);  // Position is between the edges.

      // The contact is only valid if the closest point is on the edges.
      if (s < -Numeric.EpsilonF || s > 1 + Numeric.EpsilonF)
        return false;
      if (t < -Numeric.EpsilonF || t > 1 + Numeric.EpsilonF)
        return false;

#if DEBUG
      Debug.Assert(s >= 0 && s <= 1, "Closest points are not on edge.");
      Debug.Assert(t >= 0 && t <= 1, "Closest points are not on edge.");
      Debug.Assert(Numeric.AreEqual((pointOnA - pointOnB).Length(), penetrationDepth, epsilon), "Inconsistent edge-edge penetration depth.");
      Debug.Assert(MathHelper.AreNumericallyEqual(pointOnB + normal * penetrationDepth, pointOnA, epsilon), "Inconsistent edge-edge contact positions.");
#endif

      return true;
    }
    #endregion
  }
}