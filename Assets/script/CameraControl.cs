using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraControl : MonoBehaviour
{
    public float panSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minYZoom = 5f;
    public float maxYZoom = 20f;
    public float clampLeft = 5f;
    public float clampRight = 5f;
    public float clampUp = 10f;
    public float clampDown = 5f;


    private Vector3 lastMousePosition;
    private Vector3 targetPosition;
    private float zoomSmoothTime = 0.05f;
    private Vector3 zoomVelocity;

    private void Update()
        {
        // Camera panning
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

            // Clamp camera position
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, -clampLeft, clampRight);
            clampedPosition.z = Mathf.Clamp(clampedPosition.z, -clampDown, clampUp);
            transform.position = clampedPosition;
        }

        // Camera zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newYZoom = transform.position.y - (scroll * zoomSpeed * Time.deltaTime);
        newYZoom = Mathf.Clamp(newYZoom, minYZoom, maxYZoom);

        targetPosition = new Vector3(transform.position.x, newYZoom, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, zoomSmoothTime);

    }
}
