// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Geometry.Shapes;
using Microsoft.Xna.Framework;


namespace DigitalRise.Geometry.Partitioning
{
  partial class CompressedBoundingBoxTree
  {
    /// <summary>
    /// Gets or sets the threshold that determines when a bottom-up tree build method is used.
    /// </summary>
    /// <value>
    /// The threshold that determines when the tree is built using a bottom-up method. The default
    /// value is 128.
    /// </value>
    /// <remarks>
    /// <para>
    /// AABB trees can be built using top-down or bottom-up methods. Top-down methods are faster but
    /// less optimal. Bottom-up methods are slower but produce more balanced trees. 
    /// </para>
    /// <para>
    /// The <see cref="CompressedBoundingBoxTree"/> uses a mixed approach: It starts with a top-down
    /// approach. When the number of nodes for an internal subtree is less than or equal to 
    /// <see cref="BottomUpBuildThreshold"/> it uses a bottom-up method for the subtree.
    /// </para>
    /// <para>
    /// Increasing <see cref="BottomUpBuildThreshold"/> produces a better tree but (re)building the
    /// tree takes more time. Decreasing <see cref="BottomUpBuildThreshold"/> decreases the build
    /// time but produces less optimal trees.
    /// </para>
    /// <para>
    /// Changing <see cref="BottomUpBuildThreshold"/> does not change the tree structure 
    /// immediately. It takes effect the next time the tree is rebuilt.
    /// </para>
    /// </remarks>
    public int BottomUpBuildThreshold
    {
      get { return _bottomUpBuildThreshold; }
      set { _bottomUpBuildThreshold = value; }
    }
    private int _bottomUpBuildThreshold = 128;


    /// <summary>
    /// Builds the AABB tree.
    /// </summary>
    /// <exception cref="GeometryException">
    /// Cannot build AABB tree. The property <see cref="GetBoundingBoxForItem"/> of the spatial partition
    /// is not set.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void Build()
    {
      Debug.Assert(_items != null && _items.Count > 0, "Build should not be called for empty CompressedBoundingBoxTree.");
      if (GetBoundingBoxForItem == null)
        throw new GeometryException("Cannot build AABB tree. The property GetBoundingBoxForItem of the spatial partition is not set.");

      if (_items.Count == 1)
      {
        // AABB tree contains exactly one item. (One leaf, no internal nodes.)
        int item = _items[0];

        // Determine AABB of spatial partition and prepare factors for quantization.
        BoundingBox aabb = GetBoundingBoxForItem(item);
        SetQuantizationValues(aabb);

        // Create node.
        Node node = new Node();
        node.Item = _items[0];
        SetBoundingBox(ref node, _aabb);

        _nodes = new[] { node };
        _numberOfItems = 1;
      }
      else
      {
        // Default case: Several items. (Data is stored in the leaves.)
        // First create a normal BoundingBoxTree<int> which is then compressed.
        _numberOfItems = _items.Count;

        List<IBoundingBoxTreeNode<int>> leaves = DigitalRise.ResourcePools<IBoundingBoxTreeNode<int>>.Lists.Obtain();
        for (int i = 0; i < _numberOfItems; i++)
        {
          int item = _items[i];
          BoundingBox aabb = GetBoundingBoxForItem(item);
          leaves.Add(new BoundingBoxTree<int>.Node { BoundingBox = aabb, Item = item });
        }

        // Build tree.
        BoundingBoxTree<int>.Node root = (BoundingBoxTree<int>.Node)BoundingBoxTreeBuilder.Build(leaves, () => new BoundingBoxTree<int>.Node(), BottomUpBuildThreshold);

        // Set AABB of spatial partition and prepare the factors for quantization.
        SetQuantizationValues(root.BoundingBox);

        // Compress AABB tree.
        var nodes = DigitalRise.ResourcePools<Node>.Lists.Obtain();
        CompressTree(nodes, root);
        _nodes = nodes.ToArray();

        // Recycle temporary lists.
        DigitalRise.ResourcePools<IBoundingBoxTreeNode<int>>.Lists.Recycle(leaves);
        DigitalRise.ResourcePools<Node>.Lists.Recycle(nodes);
      }

      // Recycle items list, now that we have a valid tree.
      DigitalRise.ResourcePools<int>.Lists.Recycle(_items);
      _items = null;
    }


    /// <summary>
    /// Compresses an AABB tree.
    /// </summary>
    /// <param name="compressedNodes">The list of compressed AABB nodes.</param>
    /// <param name="uncompressedNode">The root of the uncompressed AABB tree.</param>
    private void CompressTree(List<Node> compressedNodes, BoundingBoxTree<int>.Node uncompressedNode)
    {
      if (uncompressedNode.IsLeaf)
      {
        // Compress leaf node.
        Node node = new Node();
        node.Item = uncompressedNode.Item;
        SetBoundingBox(ref node, uncompressedNode.BoundingBox);
        compressedNodes.Add(node);
      }
      else
      {
        // Node is internal node.
        int currentIndex = compressedNodes.Count;
        Node node = new Node();
        SetBoundingBox(ref node, uncompressedNode.BoundingBox);
        compressedNodes.Add(node);

        // Compress child nodes.
        CompressTree(compressedNodes, uncompressedNode.LeftChild);
        CompressTree(compressedNodes, uncompressedNode.RightChild);

        // Set escape offset. (Escape offset = size of subtree)
        node.EscapeOffset = compressedNodes.Count - currentIndex;
        compressedNodes[currentIndex] = node;
      }
    }


    private void Refit()
    {
      // Compute new unquantized AABBs.
      BoundingBox[] buffer = new BoundingBox[_nodes.Length];
      int count = 0;
      ComputeBoundingBoxs(buffer, 0, ref count);

      // Update AABB of spatial partition and prepare the factors for quantization.
      SetQuantizationValues(buffer[0]);

      // Update compressed AABBs.
      for (int i = 0; i < _nodes.Length; i++)
      {
        Node node = _nodes[i];
        SetBoundingBox(ref node, buffer[i]);
        _nodes[i] = node;
      }
    }


    private void ComputeBoundingBoxs(BoundingBox[] buffer, int index, ref int count)
    {
      // Increment the counter for each node visited.
      count++;

      Node node = _nodes[index];
      if (node.IsLeaf)
      {
        // Store unquantized AABB of leaf node.
        buffer[index] = GetBoundingBoxForItem(node.Item);
      }
      else
      {
        // Compute AABB of child nodes.
        int leftIndex = index + 1;
        ComputeBoundingBoxs(buffer, leftIndex, ref count);

        int rightIndex = count;
        ComputeBoundingBoxs(buffer, rightIndex, ref count);
        buffer[index] = BoundingBox.CreateMerged(buffer[leftIndex], buffer[rightIndex]);
      }
    }
  }
}
