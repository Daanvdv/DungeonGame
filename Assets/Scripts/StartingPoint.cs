﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPoint : MonoBehaviour
{
    [Header("Options")]
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(player);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
