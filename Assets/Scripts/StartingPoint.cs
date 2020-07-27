using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPoint : MonoBehaviour
{
    [Header("Options")]
    public GameObject player;

    [Header("Info")]
    public RoomNode room;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(player, this.transform);
    }
}
