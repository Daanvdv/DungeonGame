using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingPoint : MonoBehaviour
{
    [Header("Options")]
    public GameObject indicator;

    [Header("Info")]
    public RoomNode room;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            indicator.SetActive(true);
        }
    }
}
