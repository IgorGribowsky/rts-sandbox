using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera ControlledCamera;

    private MapValues _mapValues;

    void Start()
    {
        if (ControlledCamera == null)
        {
            ControlledCamera = Camera.main;
        }

        _mapValues = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<MapValues>();
    }

    public void Set(Vector3 vector)
    {
        var z = vector.z - 1 / Mathf.Tan(ControlledCamera.transform.eulerAngles.x * Mathf.PI / 180f) * ControlledCamera.transform.position.y;
        ControlledCamera.transform.position = new Vector3(vector.x, ControlledCamera.transform.position.y, z);
    }

    public void Move(Vector3 vector)
    {
        var newPosition = ControlledCamera.transform.position + vector;

        newPosition.x = Mathf.Clamp(
            newPosition.x,
            _mapValues.LeftTopMapCornerPosition.x,
            _mapValues.RightBottomMapCornerPosition.x
        );

        newPosition.z = Mathf.Clamp(
            newPosition.z,
            _mapValues.RightBottomMapCornerPosition.z,
            _mapValues.LeftTopMapCornerPosition.z
        );

        ControlledCamera.transform.position = newPosition;
    }
}
