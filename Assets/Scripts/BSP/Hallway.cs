using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hallway : Node
{
    public Vector2Int StartPoint { get; set; }
    public Vector2Int EndPoint { get; set; }
    public bool XFirst { get; set; }
    public List<Hallway> ConnectedHallways { get; set; }
    public List<RoomNode> ConnectedRooms { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    /// <param name="depthIndex"></param>
    /// <param name="parent"></param>
    public Hallway(Vector2Int startPoint, Vector2Int endPoint, bool xFirst, Node parent) : base(parent)
    {
        this.StartPoint = startPoint;
        this.EndPoint = endPoint;
        this.XFirst = xFirst;

        ConnectedHallways = new List<Hallway>();
        ConnectedRooms = new List<RoomNode>();
    }

    public void ConnectHallway(Hallway other)
    {
        ConnectedHallways.Add(other);
    }

    public void ConnectRoom(RoomNode room)
    {
        ConnectedRooms.Add(room);
    }
}
