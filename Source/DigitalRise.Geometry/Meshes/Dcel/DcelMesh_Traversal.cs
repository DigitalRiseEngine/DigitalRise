// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRise.Geometry.Meshes
{
  public partial class DcelMesh
  {
    /// <summary>
    /// Sets the tags of the DCEL mesh component and linked components to the given tag value.
    /// </summary>
    /// <param name="vertex">The vertex. (Can be <see langword="null"/>.)</param>
    /// <param name="tag">The tag.</param>
    /// <remarks>
    /// This method does nothing if <paramref name="vertex"/> is already tagged with the given
    /// value.
    /// </remarks>
    private static void TagLinkedComponents(DcelVertex vertex, int tag)
    {
      if (vertex != null && vertex.Tag != tag)
      {
        vertex.Tag = tag;
        TagLinkedComponents(vertex.Edge, tag);
      }
    }


    /// <summary>
    /// Sets the tags of the DCEL mesh component and linked components to the given tag value.
    /// </summary>
    /// <param name="edge">The edge. (Can be <see langword="null"/>.)</param>
    /// <param name="tag">The tag.</param>
    /// <remarks>
    /// This method does nothing if <paramref name="edge"/> is already tagged with the given value.
    /// </remarks>
    private static void TagLinkedComponents(DcelEdge edge, int tag)
    {
      // Important: This method does not create recursive calls. This could
      // lead to stack overflows very quickly!

      if (edge == null || edge.Tag == tag)
        return;

      Stack<DcelEdge> todoStack = new Stack<DcelEdge>();
      todoStack.Push(edge);

      while (todoStack.Count > 0)
      {
        edge = todoStack.Pop();

        if (edge.Tag == tag)
          continue;

        // Tag edge.
        edge.Tag = tag;

        // Tag vertex
        if (edge.Origin != null)
          edge.Origin.Tag = 1;

        // Tag faces.
        if (edge.Face != null && edge.Face.Tag != tag)
        {
          edge.Face.Tag = tag;

          AddUntaggedEdgeToStack(edge.Face.Boundary, todoStack, tag);
          if (edge.Face.Holes != null)
            for (int i = 0; i < edge.Face.Holes.Count; i++)
              AddUntaggedEdgeToStack(edge.Face.Holes[i], todoStack, tag);
        }

        // Follow connected edges.
        AddUntaggedEdgeToStack(edge.Next, todoStack, 1);
        AddUntaggedEdgeToStack(edge.Previous, todoStack, 1);
        AddUntaggedEdgeToStack(edge.Twin, todoStack, 1);
      }
    }


    /// <summary>
    /// Adds the given edge to the stack if its <see cref="DcelEdge.Tag"/> is not equal to the given
    /// tag.
    /// </summary>
    /// <param name="edge">The edge. (Can be <see langword="null"/>.)</param>
    /// <param name="stack">The stack.</param>
    /// <param name="tag">The tag.</param>
    private static void AddUntaggedEdgeToStack(DcelEdge edge, Stack<DcelEdge> stack, int tag)
    {
      Debug.Assert(stack != null);
      if (edge != null && edge.Tag != tag)
        stack.Push(edge);
    }


    // Same as AddUntaggedEdgeToStack but with InternalTag
    private static void AddUntaggedEdgeToStackInternal(DcelEdge edge, Stack<DcelEdge> stack, int internalTag)
    {
      Debug.Assert(stack != null);
      if (edge != null && edge.InternalTag != internalTag)
        stack.Push(edge);
    }


    ///// <summary>
    ///// Sets the tags of the DCEL mesh component and linked components to the given tag value.
    ///// </summary>
    ///// <param name="face">The face. (Can be <see langword="null"/>.)</param>
    ///// <param name="tag">The tag.</param>
    ///// <remarks>
    ///// This method does nothing if <paramref name="face"/> is already tagged with the given value.
    ///// </remarks>
    //private static void TagLinkedComponents(DcelFace face, int tag)
    //{
    //  if (face != null && face.Tag != tag)
    //  {
    //    face.Tag = tag;
    //    TagLinkedComponents(face.Boundary, tag);
    //    if (face.Holes != null)
    //      foreach (var edge in face.Holes)
    //        TagLinkedComponents(edge, tag);
    //  }
    //}
  }
}
