using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> ChildNodesList { get; }
    public bool Visited { get; set; }
    public Node ParentNode { get; set; }
    public int DepthIndex { get; set; }

    public Node(Node parentNode)
    {
        ChildNodesList = new List<Node>();
        this.ParentNode = parentNode;
        if (parentNode != null)
        {
            parentNode.AddChild(this);
        }
    }

    /// <summary>
    /// Add child node.
    /// </summary>
    /// <param name="node">Child node to add.</param>
    public void AddChild(Node node)
    {
        ChildNodesList.Add(node);
    }

    /// <summary>
    /// Remove child node.
    /// </summary>
    /// <param name="node">Child node to remove.</param>
    public void RemoveChild(Node node)
    {
        ChildNodesList.Remove(node);
    }
}
