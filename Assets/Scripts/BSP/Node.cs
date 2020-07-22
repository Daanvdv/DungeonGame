using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private List<Node> childNodes;

    public List<Node> childNodesList { get => childNodes; }

    public bool visited { get; set; }

    public Node parentNode { get; set; }

    public int depthIndex { get; set; }

    public Node(Node parentNode)
    {
        childNodes = new List<Node>();
        this.parentNode = parentNode;
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
