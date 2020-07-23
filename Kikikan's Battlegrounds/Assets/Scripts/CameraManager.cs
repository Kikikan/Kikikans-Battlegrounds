using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public float Sensitivity = 0.1f;
    public float ZoomSensitivity = 1f;
    public float FOVMultiplier = 0.005f;
    

    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = new Vector3(transform.position.x + (Sensitivity + GetComponent<Camera>().orthographicSize * FOVMultiplier), transform.position.y, -10);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position = new Vector3(transform.position.x - (Sensitivity + GetComponent<Camera>().orthographicSize * FOVMultiplier), transform.position.y, -10);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (Sensitivity + GetComponent<Camera>().orthographicSize * FOVMultiplier), -10);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (Sensitivity + GetComponent<Camera>().orthographicSize * FOVMultiplier), -10);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            GetComponent<Camera>().orthographicSize += ZoomSensitivity;
            
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            GetComponent<Camera>().orthographicSize -= ZoomSensitivity;
            if (GetComponent<Camera>().orthographicSize < 2)
                GetComponent<Camera>().orthographicSize = 2;
        }
    }
}
