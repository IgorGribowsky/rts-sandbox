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
        var z = vector.z - 1 / Mathf.Tan(ControlledCamera.transform.eulerAngles.x * Mathf.PI / 180f) * ControlledCamera.transform.position.y;
        ControlledCamera.transform.position = new Vector3(vector.x, ControlledCamera.transform.position.y, z);
    }

    public void Move(Vector3 vector)
    {
        ControlledCamera.transform.position += vector;
    }
}
