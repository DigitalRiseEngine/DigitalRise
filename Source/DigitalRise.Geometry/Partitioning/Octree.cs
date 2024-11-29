// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics.Algebra;


namespace DigitalRise.Geometry.Partitioning
{
  public class Octree<T> : BaseBoundingBoxPartition<T>
  {
    //
    // Note:
    // This simple Octree implementation provides many opportunities for
    // improvements. See Christer Ericsson's Real Time Collision Detection 
    // book for ideas.
    //
    // Ideas:
    // We could let the user define the Root AABB size (= world size in 
    // collision detection broad-phase.
    //
    // For node order see, Real-Time Collision Detection p. 308.
    //

    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private sealed class Node
    {
      public BoundingBox BoundingBox;
      public Node[] Children = new Node[8];  // Note: We could also use ushort indices and store all nodes in one list.
      public List<T> Items = new List<T>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Node Root;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------


    public BoundingBox? MinimumRootBoundingBox
    {
      get { return _minimumRootBoundingBox; }
      set
      {
        if (value != _minimumRootBoundingBox)
        {
          _minimumRootBoundingBox = value;

          if (_minimumRootBoundingBox.HasValue)
          {
            if (!IsContained(BoundingBox, _minimumRootBoundingBox.Value))
              Invalidate();
          }
        }
      }
    }
    private BoundingBox? _minimumRootBoundingBox;


    public float MinimumCellSize { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public Octree(Func<T, BoundingBox> getBoundingBox)
      : base(getBoundingBox)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    public override IEnumerable<T> GetOverlaps(BoundingBox aabb)
    {
      Update(false, null);

      if (Root == null)
        yield break;

      var stack = new Stack<Node>();
      stack.Push(Root);

      while (stack.Count > 0)
      {
        var node = stack.Pop();

        if (GeometryHelper.HaveContact(node.BoundingBox, aabb))
        {
          foreach (var item in node.Items)
          {
            if (GeometryHelper.HaveContact(GetBoundingBoxForItem(item), aabb))
              yield return item;
          }


          for (int i = 0; i < 8; i++)
          {
            if (node.Children[i] != null)
              stack.Push(node.Children[i]);
          }
        }
      }
    }


    protected override void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems, Action<T, T> overlapCallback)
    {
      // TODO: Only total rebuild supported.

      if (Count == 0)
      {
        Root = null;
        BoundingBox = new BoundingBox();
        if (EnableSelfOverlaps)
          SelfOverlaps.Clear();
        return;
      }

      var aabbs = new BoundingBox[Count];
      aabbs[0] = GetBoundingBoxForItem(Items[0]);
      BoundingBox = aabbs[0];
      for (int i = 1; i < Count; i++)
      {
        aabbs[i] = GetBoundingBoxForItem(Items[i]);
        BoundingBox.Grow(aabbs[i]);
      }

      if (MinimumRootBoundingBox.HasValue)
        BoundingBox.Grow(MinimumRootBoundingBox.Value);


      Root = new Node { BoundingBox = BoundingBox, };

      for (int i = 0; i < Items.Count; i++)
      {
        var item = Items[i];
        var node = Root;
        var nodeBoundingBox = BoundingBox;
        var itemBoundingBox = aabbs[i];

        while (IsContained(nodeBoundingBox, itemBoundingBox))
        {
          int childIndex = -1;
          BoundingBox childNodeBoundingBox = new BoundingBox();

          if (nodeBoundingBox.Extent().LargestComponent() >= 2 * MinimumCellSize)
          {
            for (int j = 0; j < 8; j++)
            {
              childNodeBoundingBox = CreateChildBoundingBox(nodeBoundingBox, j);
              if (IsContained(childNodeBoundingBox, itemBoundingBox))
              {
                childIndex = j;
                break;
              }
            }
          }

          if (childIndex == -1)
          {
            node.Items.Add(item);
            break;
          }
          else
          {
            if (node.Children[childIndex] == null)
            {
              node.Children[childIndex] = new Node { BoundingBox = childNodeBoundingBox, };
            }

            node = node.Children[childIndex];
          }
        }
      }

      if (EnableSelfOverlaps)
      {
        for (int i = 0; i < Items.Count; i++)
        {
          var item = Items[i];
          var itemBoundingBox = aabbs[i];

          foreach (var touchedItem in GetOverlaps(itemBoundingBox))
          {
            if (Comparer.Equals(item, touchedItem))
              continue;

            if (Filter != null && !Filter.Filter(item, touchedItem))
              continue;

            var overlap = new Overlap<T>(item, touchedItem);
            bool isNew = SelfOverlaps.Add(overlap);

            if (overlapCallback != null && isNew)
              overlapCallback(item, touchedItem);
          }
        }
      }
    }

    private bool IsContained(BoundingBox container, BoundingBox aabb)
    {
      // TODO: What about numerical tolerances?
      return container.Minimum <= aabb.Min && aabb.Max <= container.Maximum;
    }



    private BoundingBox CreateChildBoundingBox(BoundingBox parentBoundingBox, int childIndex)
    {
      Vector3 offset;
      switch (childIndex % 4)
      {
        case 0: offset = new Vector3(0, 0, 0); break;
        case 1: offset = new Vector3(1, 0, 0); break;
        case 2: offset = new Vector3(0, 1, 0); break;
        default:
          Debug.Assert(childIndex % 4 == 3);
          offset = new Vector3(1, 1, 1);
          break;

      }
      if (childIndex > 3)
        offset.Z = 1;

      var childExtent = parentBoundingBox.Extent() * 0.5f;
      var minimum = parentBoundingBox.Min + offset * childExtent;
      var maximum = minimum + childExtent;
      return new BoundingBox(minimum, maximum);
    }
    #endregion

  }
}
*/
