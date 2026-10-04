using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera ControlledCamera;
    public bool FixScreen = false;

    public float Sensitivity = 30f;
    public float MoveCameraBorderSize = 20f;

    // Target zoom, 0 is MinY and 1 is MaxY; the camera eases towards it (M-002).
    public float Zoom = 0.5f;
    [Tooltip("Zoom change per wheel notch, as a share of the whole MinY..MaxY range.")]
    public float ZoomStep = 0.125f;
    [Tooltip("Seconds the camera takes to reach the target height, easing out at the end.")]
    public float ZoomSmoothTime = 0.2f;
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
        if (currentZoom == Zoom)
        {
            return;
        }

        // Exponential ease-out: fast at the start, slowing down at the end, about
        // 95% of the way in ZoomSmoothTime at any FPS. Notches spun in a row only
        // move the target on, so the camera keeps one continuous motion.
        var rate = 3f / Mathf.Max(ZoomSmoothTime, 0.01f);
        currentZoom = Mathf.Lerp(currentZoom, Zoom, 1f - Mathf.Exp(-rate * Time.deltaTime));

        if (Mathf.Abs(Zoom - currentZoom) < 0.0005f)
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

    public void SetZoom(float value)
    {
        Zoom = value;
    }

    public void ChangeZoom(float value)
    {
        Zoom = Mathf.Clamp01(Zoom + value);
    }

    // Wheel notches, positive away from the player: no deltaTime, one notch is
    // the same step at any FPS.
    public void ZoomByNotches(float notches)
    {
        ChangeZoom(notches * ZoomStep);
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
