﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic room node based off node to store room based data
public class RoomNode : Node
{
    public Vector2Int Position { get; set; }
    public Vector2Int Size { get; set; }
    public List<Hallway> ConnectedHallways { get; set; }

    public RoomNode(Vector2Int position, Vector2Int size, Node parent) : base(parent)
    {
        this.Size = size;
        this.Position = position;
    }

    /// <summary>
    /// Connect hallway to room.
    /// </summary>
    /// <param name="hallway">Hallway to connect.</param>
    public void ConnectHallway(Hallway hallway)
    {
        ConnectedHallways.Add(hallway);
    }
}
