using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Options")]
    public float movementSpeed = 8.0f;
    public float gravitySpeed = -9.81f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private GameObject cam;
    private Vector3 movement;

    // Start is called before the first frame update
    void Start()
    {
        controller = this.gameObject.AddComponent<CharacterController>();
        cam = new GameObject("Camera", typeof(Camera));
        cam.transform.position = new Vector3(cam.transform.position.x, 15.0f, cam.transform.position.z);
        cam.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(movement * Time.deltaTime * movementSpeed);

        cam.transform.position = new Vector3(this.transform.position.x, cam.transform.position.y, this.transform.position.z);
    }
}
