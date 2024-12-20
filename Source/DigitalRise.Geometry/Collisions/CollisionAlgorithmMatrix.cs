// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRise.Collections;
using DigitalRise.Geometry.Collisions.Algorithms;
using DigitalRise.Geometry.Shapes;


namespace DigitalRise.Geometry.Collisions
{
  /// <summary>
  /// A matrix which assigns a <see cref="CollisionAlgorithm"/> to each pair of <see cref="Shape"/> 
  /// types.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This matrix is symmetric in the sense that the algorithm for Box vs. Sphere is the same as for
  /// Box vs. Sphere. If the algorithm Box vs. Sphere is set, then the same algorithm will be used
  /// for Sphere vs. Box. Algorithms are defined per class type pair not per class instance pair.
  /// </para>
  /// <para>
  /// If a shape type is not registered, the base class of the shape type is checked. Example: If 
  /// <see cref="PerspectiveViewVolume"/> is not tested, the base classes <see cref="ViewVolume"/>
  /// and <see cref="ConvexShape"/> will be checked. If an algorithm is registered for a base class,
  /// then this algorithm is used.
  /// </para>
  /// <para>
  /// An algorithm can be set to <see cref="NoCollisionAlgorithm"/> to disable collisions between
  /// two shape types.
  /// </para>
  /// <para>
  /// The matrix is automatically initialized with the default collision algorithms.
  /// </para>
  /// </remarks>
  public class CollisionAlgorithmMatrix
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Dictionary<Pair<Type>, CollisionAlgorithm> _matrix = new Dictionary<Pair<Type>, CollisionAlgorithm>();
    
    // Optimization info for Optimize(). Currently only used when the internal constructor
    // is used. 
    private readonly object _newEntryLock;
    private readonly Dictionary<Pair<Type>, CollisionAlgorithm> _newEntries;
    internal int _version;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionAlgorithmMatrix"/> class.
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionAlgorithmMatrix"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public CollisionAlgorithmMatrix(CollisionDetection collisionDetection)
    {
      // Initialize with dummy collision algorithms.
      var noAlgo = new NoCollisionAlgorithm(collisionDetection);   // Definitely no collision wanted.
      var infiniteAlgo = new InfiniteShapeAlgorithm(collisionDetection);   // Returns always a collision.

      // Build default configuration:
      var gjk = new Gjk(collisionDetection);
      var gjkBoxAlgorithm = new CombinedCollisionAlgorithm(collisionDetection, gjk, new BoxBoxAlgorithm(collisionDetection));
      var gjkMprAlgorithm = new CombinedCollisionAlgorithm(collisionDetection, gjk, new MinkowskiPortalRefinement(collisionDetection));
      var gjkTriTriAlgorithm = new CombinedCollisionAlgorithm(collisionDetection, gjk, new TriangleTriangleAlgorithm(collisionDetection));

      BoxSphereAlgorithm boxSphereAlgorithm = new BoxSphereAlgorithm(collisionDetection);
      CompositeShapeAlgorithm compositeAlgorithm = new CompositeShapeAlgorithm(collisionDetection);
      HeightFieldAlgorithm heightFieldAlgorithm = new HeightFieldAlgorithm(collisionDetection);
      LineAlgorithm lineAlgorithm = new LineAlgorithm(collisionDetection);
      PlaneBoxAlgorithm planeBoxAlgorithm = new PlaneBoxAlgorithm(collisionDetection);
      PlaneConvexAlgorithm planeConvexAlgorithm = new PlaneConvexAlgorithm(collisionDetection);
      PlaneRayAlgorithm planeRayAlgorithm = new PlaneRayAlgorithm(collisionDetection);
      PlaneSphereAlgorithm planeSphereAlgorithm = new PlaneSphereAlgorithm(collisionDetection);
      RayBoxAlgorithm rayBoxAlgorithm = new RayBoxAlgorithm(collisionDetection);
      RayConvexAlgorithm rayConvexAlgorithm = new RayConvexAlgorithm(collisionDetection);
      RaySphereAlgorithm raySphereAlgorithm = new RaySphereAlgorithm(collisionDetection);
      RayTriangleAlgorithm rayTriangleAlgorithm = new RayTriangleAlgorithm(collisionDetection);
      SphereSphereAlgorithm sphereSphereAlgorithm = new SphereSphereAlgorithm(collisionDetection);
      TransformedShapeAlgorithm transformedShapeAlgorithm = new TransformedShapeAlgorithm(collisionDetection);
      TriangleMeshAlgorithm triangleMeshAlgorithm = new TriangleMeshAlgorithm(collisionDetection);
      RayCompositeAlgorithm rayCompositeAlgorithm = new RayCompositeAlgorithm(collisionDetection);
      RayTriangleMeshAlgorithm rayTriangleMeshAlgorithm = new RayTriangleMeshAlgorithm(collisionDetection);
      RayHeightFieldAlgorithm rayHeightFieldAlgorithm = new RayHeightFieldAlgorithm(collisionDetection);

      this[typeof(PointShape), typeof(PointShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(LineShape)] = lineAlgorithm;
      this[typeof(PointShape), typeof(RayShape)] = rayConvexAlgorithm;
      this[typeof(PointShape), typeof(LineSegmentShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(TriangleShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(RectangleShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(BoxShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(ConvexShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(ScaledConvexShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(SphereShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(PointShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(PointShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(PointShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(PointShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(PointShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(PointShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(PointShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(LineShape), typeof(LineShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(RayShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(LineSegmentShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(TriangleShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(RectangleShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(BoxShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(ConvexShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(ScaledConvexShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(CircleShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(SphereShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(CapsuleShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(ConeShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(CylinderShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(LineShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(LineShape), typeof(PlaneShape)] = noAlgo;
      this[typeof(LineShape), typeof(HeightField)] = noAlgo;
      this[typeof(LineShape), typeof(TriangleMeshShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(TransformedShape)] = lineAlgorithm;
      this[typeof(LineShape), typeof(CompositeShape)] = lineAlgorithm;

      this[typeof(RayShape), typeof(RayShape)] = noAlgo;
      this[typeof(RayShape), typeof(LineSegmentShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(TriangleShape)] = rayTriangleAlgorithm;
      this[typeof(RayShape), typeof(RectangleShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(BoxShape)] = rayBoxAlgorithm;
      this[typeof(RayShape), typeof(ConvexShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(ScaledConvexShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(CircleShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(SphereShape)] = raySphereAlgorithm;
      this[typeof(RayShape), typeof(CapsuleShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(ConeShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(CylinderShape)] = rayConvexAlgorithm;
      this[typeof(RayShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(RayShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(RayShape), typeof(PlaneShape)] = planeRayAlgorithm;
      this[typeof(RayShape), typeof(HeightField)] = rayHeightFieldAlgorithm;
      this[typeof(RayShape), typeof(TriangleMeshShape)] = rayTriangleMeshAlgorithm;
      this[typeof(RayShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(RayShape), typeof(CompositeShape)] = rayCompositeAlgorithm;

      this[typeof(LineSegmentShape), typeof(LineSegmentShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(TriangleShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(RectangleShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(BoxShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(ConvexShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(ScaledConvexShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(SphereShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(LineSegmentShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(LineSegmentShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(LineSegmentShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(LineSegmentShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(LineSegmentShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(LineSegmentShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(LineSegmentShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(TriangleShape), typeof(TriangleShape)] = gjkTriTriAlgorithm;
      this[typeof(TriangleShape), typeof(RectangleShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(BoxShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(ConvexShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(ScaledConvexShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(SphereShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(TriangleShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(TriangleShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(TriangleShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(TriangleShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(TriangleShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(TriangleShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(TriangleShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(RectangleShape), typeof(RectangleShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(BoxShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(ConvexShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(ScaledConvexShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(SphereShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(RectangleShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(RectangleShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(RectangleShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(RectangleShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(RectangleShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(RectangleShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(RectangleShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(BoxShape), typeof(BoxShape)] = gjkBoxAlgorithm;
      this[typeof(BoxShape), typeof(ConvexShape)] = gjkMprAlgorithm;
      this[typeof(BoxShape), typeof(ScaledConvexShape)] = gjkMprAlgorithm;
      this[typeof(BoxShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(BoxShape), typeof(SphereShape)] = boxSphereAlgorithm;
      this[typeof(BoxShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(BoxShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(BoxShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(BoxShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(BoxShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(BoxShape), typeof(PlaneShape)] = planeBoxAlgorithm;
      this[typeof(BoxShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(BoxShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(BoxShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(BoxShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(ConvexShape), typeof(ConvexShape)] = gjkMprAlgorithm;
      this[typeof(ConvexShape), typeof(ScaledConvexShape)] = gjkMprAlgorithm;
      this[typeof(ConvexShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(ConvexShape), typeof(SphereShape)] = gjkMprAlgorithm;
      this[typeof(ConvexShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(ConvexShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(ConvexShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(ConvexShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(ConvexShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(ConvexShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(ConvexShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(ConvexShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(ConvexShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(ConvexShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(ScaledConvexShape), typeof(ScaledConvexShape)] = gjkMprAlgorithm;
      this[typeof(ScaledConvexShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(ScaledConvexShape), typeof(SphereShape)] = gjkMprAlgorithm;
      this[typeof(ScaledConvexShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(ScaledConvexShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(ScaledConvexShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(ScaledConvexShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(ScaledConvexShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(ScaledConvexShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(ScaledConvexShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(ScaledConvexShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(ScaledConvexShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(ScaledConvexShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(CircleShape), typeof(CircleShape)] = gjkMprAlgorithm;
      this[typeof(CircleShape), typeof(SphereShape)] = gjkMprAlgorithm;
      this[typeof(CircleShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(CircleShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(CircleShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(CircleShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(CircleShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(CircleShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(CircleShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(CircleShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(CircleShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(CircleShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(SphereShape), typeof(SphereShape)] = sphereSphereAlgorithm;
      this[typeof(SphereShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(SphereShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(SphereShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(SphereShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(SphereShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(SphereShape), typeof(PlaneShape)] = planeSphereAlgorithm;
      this[typeof(SphereShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(SphereShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(SphereShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(SphereShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(CapsuleShape), typeof(CapsuleShape)] = gjkMprAlgorithm;
      this[typeof(CapsuleShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(CapsuleShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(CapsuleShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(CapsuleShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(CapsuleShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(CapsuleShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(CapsuleShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(CapsuleShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(CapsuleShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(ConeShape), typeof(ConeShape)] = gjkMprAlgorithm;
      this[typeof(ConeShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(ConeShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(ConeShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(ConeShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(ConeShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(ConeShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(ConeShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(ConeShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(CylinderShape), typeof(CylinderShape)] = gjkMprAlgorithm;
      this[typeof(CylinderShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(CylinderShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(CylinderShape), typeof(PlaneShape)] = planeConvexAlgorithm;
      this[typeof(CylinderShape), typeof(HeightField)] = heightFieldAlgorithm;
      this[typeof(CylinderShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(CylinderShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(CylinderShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(EmptyShape), typeof(EmptyShape)] = noAlgo;
      this[typeof(EmptyShape), typeof(InfiniteShape)] = noAlgo;  // No collision between Empty and Infinite.
      this[typeof(EmptyShape), typeof(PlaneShape)] = noAlgo;
      this[typeof(EmptyShape), typeof(HeightField)] = noAlgo;
      this[typeof(EmptyShape), typeof(TriangleMeshShape)] = noAlgo;
      this[typeof(EmptyShape), typeof(TransformedShape)] = noAlgo;
      this[typeof(EmptyShape), typeof(CompositeShape)] = noAlgo;

      this[typeof(InfiniteShape), typeof(InfiniteShape)] = infiniteAlgo;
      this[typeof(InfiniteShape), typeof(PlaneShape)] = infiniteAlgo;
      this[typeof(InfiniteShape), typeof(HeightField)] = infiniteAlgo;
      this[typeof(InfiniteShape), typeof(TriangleMeshShape)] = infiniteAlgo;
      this[typeof(InfiniteShape), typeof(TransformedShape)] = infiniteAlgo;
      this[typeof(InfiniteShape), typeof(CompositeShape)] = infiniteAlgo;

      this[typeof(PlaneShape), typeof(PlaneShape)] = noAlgo;
      this[typeof(PlaneShape), typeof(HeightField)] = noAlgo;
      this[typeof(PlaneShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(PlaneShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(PlaneShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(HeightField), typeof(HeightField)] = noAlgo;
      // We could also call triangleMeshAlgorithm. But since HeightField has usually larger parts it 
      // is better to call the heightFieldAlgorithm. The heightFieldAlgorithm will cull all but a 
      // few height field cells very quickly.
      this[typeof(HeightField), typeof(TriangleMeshShape)] = heightFieldAlgorithm;
      this[typeof(HeightField), typeof(TransformedShape)] = transformedShapeAlgorithm;
      // Same as for triangle meshes: Call height field algorithm first.
      this[typeof(HeightField), typeof(CompositeShape)] = heightFieldAlgorithm;

      this[typeof(TriangleMeshShape), typeof(TriangleMeshShape)] = triangleMeshAlgorithm;
      this[typeof(TriangleMeshShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(TriangleMeshShape), typeof(CompositeShape)] = compositeAlgorithm;

      this[typeof(TransformedShape), typeof(TransformedShape)] = transformedShapeAlgorithm;
      this[typeof(TransformedShape), typeof(CompositeShape)] = transformedShapeAlgorithm;

      this[typeof(CompositeShape), typeof(CompositeShape)] = compositeAlgorithm;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionAlgorithmMatrix" /> class.
    /// </summary>
    /// <param name="matrix">The <see cref="CollisionAlgorithmMatrix" /> from which the settings are copied.</param>
    internal CollisionAlgorithmMatrix(CollisionAlgorithmMatrix matrix)
    {
      _newEntryLock = new object();
      _newEntries = new Dictionary<Pair<Type>, CollisionAlgorithm>();

      foreach (var entry in matrix._matrix)
        _matrix[entry.Key] = entry.Value;

      _version = matrix._version;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Gets or sets the <see cref="CollisionAlgorithm"/> for a pair of objects.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets or sets the <see cref="CollisionAlgorithm"/> for the pair of collision objects.
    /// </summary>
    /// <param name="pair">A contact set containing a pair of collision objects.</param>
    /// <value>The collision algorithm.</value>
    /// <remarks>
    /// Collision algorithms can be defined per pair of shape classes (not per pair of shape 
    /// instances). If an algorithm is set for [A, B], the same algorithm is automatically set for 
    /// [B, A].
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pair"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No <see cref="CollisionAlgorithm"/> is registered for the given shape pair.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
    public CollisionAlgorithm this[ContactSet pair]
    {
      get
      {
        if (pair == null)
          throw new ArgumentNullException("pair");

        Debug.Assert(pair.ObjectA != null, "ContactSet needs to ensure that ObjectA is not null.");
        Debug.Assert(pair.ObjectB != null, "ContactSet needs to ensure that ObjectB is not null.");
        Debug.Assert(pair.ObjectA.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(pair.ObjectB.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(pair.ObjectA.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");
        Debug.Assert(pair.ObjectB.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");

        return this[pair.ObjectA.GeometricObject.Shape.GetType(), pair.ObjectB.GeometricObject.Shape.GetType()];
      }
      set
      {
        if (pair == null)
          throw new ArgumentNullException("pair");

        Debug.Assert(pair.ObjectA != null, "ContactSet needs to ensure that ObjectA is not null.");
        Debug.Assert(pair.ObjectB != null, "ContactSet needs to ensure that ObjectB is not null.");
        Debug.Assert(pair.ObjectA.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(pair.ObjectB.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(pair.ObjectA.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");
        Debug.Assert(pair.ObjectB.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");

        this[pair.ObjectA.GeometricObject.Shape.GetType(), pair.ObjectB.GeometricObject.Shape.GetType()] = value;
      }
    }


    /// <summary>
    /// Gets or sets the <see cref="CollisionAlgorithm"/> for the specified collision objects.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <value>The collision algorithm.</value>
    /// <remarks>
    /// Collision algorithms can be defined per pair of shape classes (not per pair of shape 
    /// instances). If an algorithm is set for [A, B], the same algorithm is automatically set for 
    /// [B, A].
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No <see cref="CollisionAlgorithm"/> is registered for the given shape pair.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
    public CollisionAlgorithm this[CollisionObject objectA, CollisionObject objectB]
    {
      get
      {
        if (objectA == null)
          throw new ArgumentNullException("objectA");
        if (objectB == null)
          throw new ArgumentNullException("objectB");

        Debug.Assert(objectA.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(objectB.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(objectA.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");
        Debug.Assert(objectB.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");

        return this[objectA.GeometricObject.Shape.GetType(), objectB.GeometricObject.Shape.GetType()];
      }
      set
      {
        if (objectA == null)
          throw new ArgumentNullException("objectA");
        if (objectB == null)
          throw new ArgumentNullException("objectB");

        Debug.Assert(objectA.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(objectB.GeometricObject != null, "CollisionObject needs to ensure that GeometricObject is not null.");
        Debug.Assert(objectA.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");
        Debug.Assert(objectB.GeometricObject.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");

        this[objectA.GeometricObject.Shape.GetType(), objectB.GeometricObject.Shape.GetType()] = value;
      }
    }


    /// <summary>
    /// Gets or sets the <see cref="CollisionAlgorithm"/> for the specified geometric objects.
    /// </summary>
    /// <param name="geometricObjectA">The first geometric object.</param>
    /// <param name="geometricObjectB">The second geometric object.</param>
    /// <value>The collision algorithm.</value>
    /// <remarks>
    /// Collision algorithms can be defined per pair of shape classes (not per pair of shape 
    /// instances). If an algorithm is set for [A, B], the same algorithm is automatically set for 
    /// [B, A].
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="geometricObjectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="geometricObjectB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No <see cref="CollisionAlgorithm"/> is registered for the given shape pair.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
    public CollisionAlgorithm this[IGeometricObject geometricObjectA, IGeometricObject geometricObjectB]
    {
      get
      {
        if (geometricObjectA == null)
          throw new ArgumentNullException("geometricObjectA");
        if (geometricObjectB == null)
          throw new ArgumentNullException("geometricObjectB");

        Debug.Assert(geometricObjectA.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");
        Debug.Assert(geometricObjectB.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");

        return this[geometricObjectA.Shape.GetType(), geometricObjectB.Shape.GetType()];
      }
      set
      {
        if (geometricObjectA == null)
          throw new ArgumentNullException("geometricObjectA");
        if (geometricObjectB == null)
          throw new ArgumentNullException("geometricObjectB");

        Debug.Assert(geometricObjectA.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");
        Debug.Assert(geometricObjectB.Shape != null, "IGeometricObject needs to ensure that Shape is not null.");

        this[geometricObjectA.Shape.GetType(), geometricObjectB.Shape.GetType()] = value;
      }
    }


    /// <summary>
    /// Gets or sets the <see cref="CollisionAlgorithm"/> for the specified shape types.
    /// </summary>
    /// <param name="shapeA">The first shape.</param>
    /// <param name="shapeB">The second shape.</param>
    /// <value>The collision algorithm.</value>
    /// <remarks>
    /// Collision algorithms can be defined per pair of shape classes (not per pair of shape 
    /// instances). If an algorithm is set for [A, B], the same algorithm is automatically set for 
    /// [B, A].
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shapeA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shapeB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No <see cref="CollisionAlgorithm"/> is registered for the given shape pair.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
    public CollisionAlgorithm this[Shape shapeA, Shape shapeB]
    {
      get
      {
        if (shapeA == null)
          throw new ArgumentNullException("shapeA");
        if (shapeB == null)
          throw new ArgumentNullException("shapeB");

        return this[shapeA.GetType(), shapeB.GetType()];
      }
      set
      {
        if (shapeA == null)
          throw new ArgumentNullException("shapeA");
        if (shapeB == null)
          throw new ArgumentNullException("shapeB");

        this[shapeA.GetType(), shapeB.GetType()] = value;
      }
    }


    /// <summary>
    /// Gets or sets the <see cref="CollisionAlgorithm"/> for the specified
    /// shape types.
    /// </summary>
    /// <param name="typeA">The first shape type.</param>
    /// <param name="typeB">The second shape type.</param>
    /// <value>The collision algorithm.</value>
    /// <remarks>
    /// Collision algorithms can be defined per pair of shape types (not per pair of shape 
    /// instances). If an algorithm is set for [A, B], the same algorithm is automatically set for 
    /// [B, A].
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="typeA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="typeB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No <see cref="CollisionAlgorithm"/> is registered for the given shape pair.
    /// </exception>
    /// <exception cref="ArgumentException">The specified type does not inherit from Shape.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
    public CollisionAlgorithm this[Type typeA, Type typeB]
    {
      get
      {
        if (typeA == null)
          throw new ArgumentNullException("typeA");
        if (typeB == null)
          throw new ArgumentNullException("typeB");

        bool addEntry = false;

        // Search for collision algorithm. If a type is not found, the base type is checked. 
        // So if Sphere vs. * is not registered Convex vs. * is used.
        Type searchTypeA = typeA;
        while (searchTypeA != null)
        {
          Type searchTypeB = typeB;
          while (searchTypeB != null)
          {
            CollisionAlgorithm collisionAlgorithm;
            if (_matrix.TryGetValue(new Pair<Type>(searchTypeA, searchTypeB), out collisionAlgorithm))
            {
              // Collision algorithm found.
              if (addEntry && _newEntries != null)
                lock(_newEntryLock)
                  _newEntries[new Pair<Type>(typeA, typeB)] = collisionAlgorithm;

              return collisionAlgorithm;
            }

            if (searchTypeB == typeof(RayShape))
              break; // Do not cast RayShape to ConvexShape.

            searchTypeB = searchTypeB.BaseType;

            addEntry = true;
          }

          if (searchTypeA == typeof(RayShape))
            break; // Do not cast RayShape to ConvexShape.

          searchTypeA = searchTypeA.BaseType;
          addEntry = true;
        }

        // If we come to here, we haven't found anything :-(
        throw new KeyNotFoundException(String.Format(CultureInfo.InvariantCulture, "No collision algorithm found for ({0}, {1}).", typeA, typeB));
      }
      set
      {
        if (typeA == null)
          throw new ArgumentNullException("typeA");
        if (typeB == null)
          throw new ArgumentNullException("typeB");
        if (value == null)
          throw new ArgumentNullException("value", "'null' is not a valid collision algorithm. Use NoCollisionAlgorithm if you want to disable collisions.");

        if (!typeof(Shape).IsAssignableFrom(typeA))
          throw new ArgumentException("The specified type must inherit from Shape.", "typeA");
        if (!typeof(Shape).IsAssignableFrom(typeB))
          throw new ArgumentException("The specified type must inherit from Shape.", "typeB");

        _matrix[new Pair<Type>(typeA, typeB)] = value;
        _version++;
      }
    }


    /// <summary>
    /// Optimizes this instance using collected usage information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the algorithm matrix is used, information about its usage is collected. 
    /// When this method is called, the internal structure is optimized. 
    /// </para>
    /// <para>
    /// This method is not thread-safe.
    /// </para>
    /// </remarks>
    internal void Optimize()
    {
      if (_newEntries == null)
        return;

      // Note: This lock is not really necessary because we also change _matrix which is
      // not locked elsewhere. Hence, this operation is never thread-safe.
      lock (_newEntryLock)
      {
        foreach (var entry in _newEntries)
          _matrix[entry.Key] = entry.Value;

        _newEntries.Clear();

        _version++;
      }
    }
    #endregion
  }
}
