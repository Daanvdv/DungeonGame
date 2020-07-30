using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Info script for hallway info of spawned in hallways
public class HallwayInfo : MonoBehaviour
{
    [Header("Info")]
    public Vector2Int startPosition;
    public Vector2Int endPosition;
    public Vector2Int pos;
    public Vector2Int size;
    public bool connected;
}
