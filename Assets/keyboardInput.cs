using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyboardInput : MonoBehaviour
{
    LanPlayer lanPlayer;

    public FixedJoystick joystick;
    public float speed = 10.0f;
    void Start()
    {
        lanPlayer = GetComponent<LanPlayer>();
        joystick = lanPlayer.joystick;
    }

    // Update is called once per frame

    void Update()
    {

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            direction.z = 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            direction.z = -1;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            direction.x = -1;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            direction.x = 1;
        }

        //joystick.Direction = direction;

        Vector3 velocity = new Vector3(joystick.Horizontal * speed, 0, joystick.Vertical * speed);
        transform.position += velocity * Time.deltaTime;
    }
}
