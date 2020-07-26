using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode : Node
{
    public Vector2Int Position { get; set; }
    public Vector2Int Size { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos">Base position the corner facing towards (0,0).</param>
    /// <param name="size">Size of the room in x and y.</param>
    /// <param name="depth">Depth of node in tree.</param>
    /// <param name="parent">Parent of the node.</param>
    public RoomNode(Vector2Int position, Vector2Int size, int depth, Node parent) : base(parent)
    {
        this.Size = size;
        this.Position = position;
        this.DepthIndex = depth;
    }
}
