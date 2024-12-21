using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class WindowsInputController : MonoBehaviour
{
    public float Sensitivity = 30f;
    public float MoveCameraBorderSize = 20f;
    public GameObject Controller;

    public bool FixScreen = false;
    public KeyCode FixScreenKey = KeyCode.F8;

    private CameraController _cameraController;
    private UnitsController _unitController;
    private SelectionBoxController _selectionBoxController;

    private int ClickLayerMask;

    KeyCode[] keypadCodes = new KeyCode[]
        {
          KeyCode.Alpha1,
          KeyCode.Alpha2,
          KeyCode.Alpha3,
          KeyCode.Alpha4,
          KeyCode.Alpha5,
          KeyCode.Alpha6,
          KeyCode.Alpha7,
          KeyCode.Alpha8,
          KeyCode.Alpha9,
          KeyCode.Alpha0,
        };

    void Start()
    {
        ClickLayerMask = LayerMask.GetMask(
            Layer.MovementSurface.ToString(),
            Layer.Unit.ToString()
            );

        _unitController = Controller.GetComponent<UnitsController>();
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

        if (KeypadCodeDown(out KeyCode keypadCode, out int num))
        {
            _unitController.ProduceUnit(num);
        }

        if (Input.GetKeyDown(FixScreenKey))
        {
            FixScreen = !FixScreen;
        }

        if (!FixScreen)
        {
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

    bool KeypadCodeDown(out KeyCode keypadCodeDown, out int num)
    {
        for (int i = 0; i < keypadCodes.Length; i++)
        {
            KeyCode keypadCode = keypadCodes[i];
            if (Input.GetKeyDown(keypadCode))
            {
                // Output the KeyCode that was pressed.
                keypadCodeDown = keypadCode;
                num = i;
                return true;
            }
        }

        // Output some default value to if none of the keypad codes were pressed.
        keypadCodeDown = KeyCode.None;
        num = -1;
        return false;
    }
}
