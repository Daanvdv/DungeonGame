using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo : MonoBehaviour
{
    [Header("Info")]
    public Vector2Int pos;
    public Vector2Int size;

    public bool CompareRooms(NodeInfo info)
    {
        if (this.pos == info.pos && this.size == info.size)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
