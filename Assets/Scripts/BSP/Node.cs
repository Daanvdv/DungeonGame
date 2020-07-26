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

    public void AddChild(Node node)
    {
        ChildNodesList.Add(node);
    }
    public void RemoveChild(Node node)
    {
        ChildNodesList.Remove(node);
    }
}
