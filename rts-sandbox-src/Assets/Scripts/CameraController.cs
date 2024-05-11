using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera ControlledCamera;

    void Start()
    {
        if (ControlledCamera == null)
        {
            ControlledCamera = Camera.main;
        }
    }

    public void Set(Vector3 vector)
    {
        ControlledCamera.transform.position = vector;
    }

    public void Move(Vector3 vector)
    {
        ControlledCamera.transform.position += vector;
    }
}
