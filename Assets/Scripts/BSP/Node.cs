using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private List<Node> childNodes;

    public List<Node> ChildNodesList { get => childNodes; }

    public bool Visited { get; set; }

    public Node ParentNode { get; set; }

    public int DepthIndex { get; set; }

    public Node(Node parentNode)
    {
        childNodes = new List<Node>();
        this.ParentNode = parentNode;
        if (parentNode != null)
        {
            parentNode.AddChild(this);
        }
    }

    public void AddChild(Node node)
    {
        childNodes.Add(node);
    }
    public void RemoveChild(Node node)
    {
        childNodes.Remove(node);
    }
}
