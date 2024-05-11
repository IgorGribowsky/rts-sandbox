using Assets.Scripts.Enums;
using UnityEngine;

public class WindowsController : MonoBehaviour
{
    public float Sensitivity = 30f;
    public float MoveCameraBorderSize = 20f;
    public GameObject Controller;

    private CameraController _cameraController;
    private UnitController _unitController;

    private int MovementSurfaceLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        MovementSurfaceLayerMask = LayerMask.GetMask(Layers.MovementSurface.ToString());

        _unitController = Controller.GetComponent<UnitController>();
        _cameraController = Controller.GetComponent<CameraController>();
    }

    void Update() 
    {
        if (Input.GetMouseButtonUp(1))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, MovementSurfaceLayerMask))
            {
                _unitController.MoveTo(hit.point);
            }
        }

        var moveCameraVector = Vector3.zero;
        if (Input.mousePosition.x < MoveCameraBorderSize)
        {
            moveCameraVector += new Vector3(-Sensitivity, 0, 0);
        }
        else if (Input.mousePosition.x > Screen.width - MoveCameraBorderSize)
        {
            moveCameraVector += new Vector3(Sensitivity, 0, 0);
        }
        if (Input.mousePosition.y < MoveCameraBorderSize)
        {
            moveCameraVector += new Vector3(0, 0, -Sensitivity);
        }
        else if (Input.mousePosition.y > Screen.height - MoveCameraBorderSize)
        {
            moveCameraVector += new Vector3(0, 0, Sensitivity);
        }
        Debug.Log(moveCameraVector);
        _cameraController.Move(moveCameraVector * Time.deltaTime);
    }
}
