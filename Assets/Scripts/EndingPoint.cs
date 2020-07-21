using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingPoint : MonoBehaviour
{
    [Header("Options")]
    public GameObject indiactor;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            indiactor.SetActive(true);
        }
    }
}
