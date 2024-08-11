using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class WindowsInputController : MonoBehaviour
{
    public float Sensitivity = 30f;
    public float MoveCameraBorderSize = 20f;
    public GameObject Controller;

    private CameraController _cameraController;
    private UnitController _unitController;
    private SelectionBoxController _selectionBoxController;

    private int ClickLayerMask;

    void Start()
    {
        ClickLayerMask = LayerMask.GetMask(
            Layer.MovementSurface.ToString(),
            Layer.Unit.ToString()
            );

        _unitController = Controller.GetComponent<UnitController>();
        _cameraController = Controller.GetComponent<CameraController>();
        _selectionBoxController = Controller.GetComponent<SelectionBoxController>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            var center = _unitController.GetTheMostRangedUnitPosition();
            _cameraController.Set(center);
        }

        if (Input.GetMouseButtonDown(0))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, ClickLayerMask))
            {
                _unitController.StartSelection(hit.point);
                _selectionBoxController.StartDrawSelection(hit.point);
            }
        }

        if (Input.GetMouseButton(0))
        {
            _selectionBoxController.DrawSelection(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, ClickLayerMask))
            {
                _unitController.EndSelection(hit.point, Input.GetKey(KeyCode.LeftShift));
                _selectionBoxController.EndDrawSelection();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, ClickLayerMask))
            {
                var isShiftButtonPressed = Input.GetKey(KeyCode.LeftShift);
                var gameObject = hit.transform.gameObject;
                if (gameObject.layer == (int)Layer.MovementSurface)
                {
                    _unitController.OnGroundRightClick(hit.point, isShiftButtonPressed);
                }
                else if (gameObject.layer == (int)Layer.Unit)
                {
                    _unitController.OnUnitRightClick(gameObject, isShiftButtonPressed);
                }
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
