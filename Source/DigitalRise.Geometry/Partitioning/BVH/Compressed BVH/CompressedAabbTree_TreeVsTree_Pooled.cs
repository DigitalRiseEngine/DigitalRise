// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if POOL_ENUMERABLES
using System.Collections.Generic;
using DigitalRise.Collections;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics.Algebra;


namespace DigitalRise.Geometry.Partitioning
{
  partial class CompressedBoundingBoxTree
  {
    // ReSharper disable StaticFieldInGenericType
    private sealed class GetOverlapsWithPartitionWork : PooledEnumerable<Pair<int>>
    {
      private static readonly ResourcePool<GetOverlapsWithPartitionWork> Pool = new ResourcePool<GetOverlapsWithPartitionWork>(() => new GetOverlapsWithPartitionWork(), x => x.Initialize(), null);
      private CompressedBoundingBoxTree _partition;
      private ISpatialPartition<int> _otherPartition;
      private IEnumerator<Node> _leafNodes;
      private IEnumerator<int> _otherCandidates;

      public static IEnumerable<Pair<int>> Create(CompressedBoundingBoxTree partition, ISpatialPartition<int> otherPartition)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._otherPartition = otherPartition;
        enumerable._leafNodes = partition.GetLeafNodes(otherPartition.BoundingBox).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out Pair<int> current)
      {
        while (true)
        {
          if (_otherCandidates == null)
          {
            if (_leafNodes.MoveNext())
            {
              var leaf = _leafNodes.Current;
              BoundingBox leafBoundingBox = _partition.GetBoundingBox(leaf);
              _otherCandidates = _otherPartition.GetOverlaps(leafBoundingBox).GetEnumerator();
            }
            else
            {
              current = default(Pair<int>);
              return false;
            }
          }

          while (_otherCandidates.MoveNext())
          {
            var leaf = _leafNodes.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<int>(leaf.Item, otherCandidate);
            if (_partition.Filter == null || _partition.Filter.Filter(overlap))
            {
              current = overlap;
              return true;
            }
          }

          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
      }

      protected override void OnRecycle()
      {
        _partition = null;
        _otherPartition = null;
        _leafNodes.Dispose();
        _leafNodes = null;
        if (_otherCandidates != null)
        {
          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithTransformedPartitionWork : PooledEnumerable<Pair<int>>
    {
      private static readonly ResourcePool<GetOverlapsWithTransformedPartitionWork> Pool = new ResourcePool<GetOverlapsWithTransformedPartitionWork>(() => new GetOverlapsWithTransformedPartitionWork(), x => x.Initialize(), null);
      private CompressedBoundingBoxTree _partition;
      private ISpatialPartition<int> _otherPartition;
      private IEnumerator<Node> _leafNodes;
      private BoundingBox _leafBoundingBox;
      private IEnumerator<int> _otherCandidates;
      private Vector3 _scale;
      private Vector3 _otherScaleInverse;
      private Pose _toOther;

      public static IEnumerable<Pair<int>> Create(CompressedBoundingBoxTree partition,
        ISpatialPartition<int> otherPartition, IEnumerable<Node> leafNodes,
        ref Vector3 scale, ref Vector3 otherScaleInverse, ref Pose toOther)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._otherPartition = otherPartition;
        enumerable._leafNodes = leafNodes.GetEnumerator();
        enumerable._scale = scale;
        enumerable._otherScaleInverse = otherScaleInverse;
        enumerable._toOther = toOther;
        return enumerable;
      }

      protected override bool OnNext(out Pair<int> current)
      {
        while (true)
        {
          if (_otherCandidates == null)
          {
            if (_leafNodes.MoveNext())
            {
              var leaf = _leafNodes.Current;
              _leafBoundingBox = _partition.GetBoundingBox(leaf);
              _leafBoundingBox = _leafBoundingBox.GetBoundingBox(_scale, _toOther);
              _leafBoundingBox.Scale(_otherScaleInverse);
              _otherCandidates = _otherPartition.GetOverlaps(_leafBoundingBox).GetEnumerator();
            }
            else
            {
              current = default(Pair<int>);
              return false;
            }
          }

          while (_otherCandidates.MoveNext())
          {
            var leaf = _leafNodes.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<int>(leaf.Item, otherCandidate);
            if (_partition.Filter == null || _partition.Filter.Filter(overlap))
            {
              current = overlap;
              return true;
            }
          }

          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
      }

      protected override void OnRecycle()
      {
        _partition = null;
        _otherPartition = null;
        _leafNodes.Dispose();
        _leafNodes = null;
        if (_otherCandidates != null)
        {
          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
        Pool.Recycle(this);
      }
    }
    // ReSharper restore StaticFieldInGenericType
  }
}
#endif
