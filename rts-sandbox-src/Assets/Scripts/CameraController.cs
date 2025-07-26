using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera ControlledCamera;
    public bool FixScreen = false;

    public float Sensitivity = 30f;
    public float MoveCameraBorderSize = 20f;

    public float Zoom = 0.5f;
    public float SensitivityZoom = 0.5f;
    public float ZoomChangingVelocity = 0.3f;
    public float MinY = 10f;
    public float MaxY = 30f;

    private float currentZoom; 
    private float currentY;

    private MapValues _mapValues;

    void Start()
    {
        if (ControlledCamera == null)
        {
            ControlledCamera = Camera.main;
        }

        _mapValues = GameObject.FindGameObjectWithTag(Tag.GameController.ToString())
            .GetComponent<MapValues>();

        currentZoom = Zoom;
        currentY = MinY + currentZoom * (MaxY - MinY);
    }

    void Update()
    {
        if (currentZoom != Zoom)
        {
            var dif = (Zoom - currentZoom);

            var deltaChange = ZoomChangingVelocity * Time.deltaTime;

            if (Mathf.Abs(dif) > deltaChange)
            {
                currentZoom += Mathf.Sign(dif) * deltaChange;
            }
            else
            {
                currentZoom = Zoom;
            }

            currentY = MinY + currentZoom * (MaxY - MinY);

            Vector3 newPosition = new Vector3(
                ControlledCamera.transform.position.x,
                currentY,
                ControlledCamera.transform.position.z
            );

            ControlledCamera.transform.position = newPosition;
        }
    }

    public void SetZoom(float value)
    {
        Zoom = value;
    }

    public void ChangeZoom(float value)
    {
        Zoom += value;

        if (Zoom > 1)
        {
            Zoom = 1;
        }
        else if (Zoom < 0)
        {
            Zoom = 0;
        }
    }

    public void SwitchFixScreen()
    {
        FixScreen = !FixScreen;
    }

    public void SetCamera(Vector3 vector)
    {
        var z = vector.z - 1 / Mathf.Tan(ControlledCamera.transform.eulerAngles.x * Mathf.PI / 180f) * ControlledCamera.transform.position.y;
        ControlledCamera.transform.position = new Vector3(vector.x, currentY, z);
    }

    public void MoveCamera(Vector3 offset)
    {
        Vector3 position = ControlledCamera.transform.position + offset;

        float minX = _mapValues.LeftTopMapCornerPosition.x;
        float maxX = _mapValues.RightBottomMapCornerPosition.x;
        float minZ = _mapValues.RightBottomMapCornerPosition.z;
        float maxZ = _mapValues.LeftTopMapCornerPosition.z;

        Vector3 newPosition = new Vector3(
            Mathf.Clamp(position.x, minX, maxX),
            currentY,
            Mathf.Clamp(position.z, minZ, maxZ)
        );

        ControlledCamera.transform.position = newPosition;
    }
}
