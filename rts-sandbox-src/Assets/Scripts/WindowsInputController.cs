using Assets.Scripts.Enums;
using UnityEngine;

public class WindowsInputController : MonoBehaviour
{
    public float Sensitivity = 30f;
    public float MoveCameraBorderSize = 20f;
    public GameObject Controller;

    private CameraController _cameraController;
    private UnitController _unitController;
    private SelectionBoxController _gizmosController;

    private int MovementSurfaceLayerMask;

    void Start()
    {
        MovementSurfaceLayerMask = LayerMask.GetMask(Layers.MovementSurface.ToString());

        _unitController = Controller.GetComponent<UnitController>();
        _cameraController = Controller.GetComponent<CameraController>();
        _gizmosController = Controller.GetComponent<SelectionBoxController>();
    }

    void Update() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, MovementSurfaceLayerMask))
            {
                _unitController.StartSelection(hit.point);
                _gizmosController.StartDrawSelection(hit.point);
            }
        }

        if (Input.GetMouseButton(0))
        {
             _gizmosController.DrawSelection(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, MovementSurfaceLayerMask))
            {
                _unitController.EndSelection(hit.point, Input.GetKey(KeyCode.LeftShift));
                _gizmosController.EndDrawSelection();
            }

        }

        if (Input.GetMouseButtonDown(1))
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

        _cameraController.Move(moveCameraVector * Time.deltaTime);
    }
}
