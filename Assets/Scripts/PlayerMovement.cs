using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Options")]
    public float movementSpeed = 8.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float movementAmount = movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.W))
        {
            this.transform.position += new Vector3(0.0f, 0.0f, movementAmount);
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.position += new Vector3(-movementAmount, 0.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.position += new Vector3(0.0f, 0.0f, -movementAmount);
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.position += new Vector3(movementAmount, 0.0f, 0.0f);
        }
    }
}
