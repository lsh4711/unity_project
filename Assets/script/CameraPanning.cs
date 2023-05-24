using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanning : MonoBehaviour
{
    public float panSpeed = 5f;
    private Vector3 lastMousePosition;

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 pan = new Vector3(-delta.x, 0f, -delta.y) * panSpeed * Time.deltaTime;
            transform.Translate(pan, Space.World);
            lastMousePosition = Input.mousePosition;
        }
    }
}

