using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowsInputController : MonoBehaviour
{
    public GameObject Controller;

    public KeyCode AClickKey = KeyCode.A;
    public KeyCode FixScreenKey = KeyCode.F8;
    public KeyCode HoldKey = KeyCode.H;
    public KeyCode OpenBuildingMenuKey = KeyCode.B;
    public KeyCode CancelKey = KeyCode.Escape;
    public KeyCode ReturnCameraKey = KeyCode.Space;
    public KeyCode AddToQueueKey = KeyCode.LeftShift;

    public bool AClickPressed { get => aClickPressed; }

    private CameraController _cameraController;
    private UnitsController _unitController;
    private SelectionBoxController _selectionBoxController;
    private BuildingController _buildingController;
    private PlayerEventController _playerEventController;
    private SkillController _skillController;

    private const float snapStep = 0.03f * GameConstants.GridCellSize;

    private bool selectionStarted = false;
    private bool aClickPressed = false;
    private int clickLayerMask;
    private int buildLayerMask;
    private Vector3 lastSnappedPosition;

    private float lastClickTime = -1f;

    private KeyCode[] allKeyCodes;
    private List<KeyCode> usedKeys;
    private KeyCode currentKeyPressed = KeyCode.None;


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

    void Awake()
    {
        allKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

        // The reserved set is the single list; the fields below are added on top
        // so that rebinding a key in the inspector still takes it out of skills.
        usedKeys = new List<KeyCode>(ReservedKeys.All);
        usedKeys.AddRange(new[]
        {
            AClickKey,
            FixScreenKey,
            HoldKey,
            OpenBuildingMenuKey,
            CancelKey,
            ReturnCameraKey,
            AddToQueueKey,
        });
        usedKeys.AddRange(keypadCodes);
    }

    void Start()
    {
        clickLayerMask = LayerMask.GetMask(
            Layer.MovementSurface.ToString(),
            Layer.Unit.ToString(),
            Layer.HarvestedResource.ToString()
            );

        buildLayerMask = LayerMask.GetMask(Layer.MovementSurface.ToString());

        _unitController = Controller.GetComponent<UnitsController>();
        _buildingController = Controller.GetComponent<BuildingController>();
        _cameraController = Controller.GetComponent<CameraController>();
        _selectionBoxController = Controller.GetComponent<SelectionBoxController>();
        _playerEventController = Controller.GetComponent<PlayerEventController>();
        _skillController = Controller.GetComponent<SkillController>();
    }

    void Update()
    {
        UpdateMousePosition();

        if (!_cameraController.FixScreen)
        {
            var moveCameraVector = Vector3.zero;
            if (Input.mousePosition.x < _cameraController.MoveCameraBorderSize)
            {
                moveCameraVector += new Vector3(-_cameraController.Sensitivity, 0, 0);
            }
            else if (Input.mousePosition.x > Screen.width - _cameraController.MoveCameraBorderSize)
            {
                moveCameraVector += new Vector3(_cameraController.Sensitivity, 0, 0);
            }
            if (Input.mousePosition.y < _cameraController.MoveCameraBorderSize)
            {
                moveCameraVector += new Vector3(0, 0, -_cameraController.Sensitivity);
            }
            else if (Input.mousePosition.y > Screen.height - _cameraController.MoveCameraBorderSize)
            {
                moveCameraVector += new Vector3(0, 0, _cameraController.Sensitivity);
            }

            _cameraController.MoveCamera(moveCameraVector * Time.deltaTime);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            var zoomDelta = -1 * Input.mouseScrollDelta.y * Time.deltaTime * _cameraController.SensitivityZoom;
            _cameraController.ChangeZoom(zoomDelta);
        }

        if (Input.GetKey(ReturnCameraKey))
        {
            var center = _unitController.GetTheMostRangedUnitPosition();
            _cameraController.SetCamera(center);
        }

        var isShiftButtonPressed = Input.GetKey(AddToQueueKey);

        if (aClickPressed)
        {
            if (Input.GetMouseButtonDown(1))
            {
                aClickPressed = false;
            }

            if (Input.GetKeyDown(CancelKey))
            {
                aClickPressed = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 100f, clickLayerMask))
                {
                    aClickPressed = false;

                    var gameObject = hit.transform.gameObject;
                    if (gameObject.layer == (int)Layer.MovementSurface)
                    {
                        _unitController.OnGroundAClick(hit.point, isShiftButtonPressed);
                    }
                    else if (gameObject.layer == (int)Layer.Unit)
                    {
                        _unitController.OnUnitAClick(gameObject, isShiftButtonPressed);
                    }
                }
            }

            return;
        }

        if (_buildingController.BuildingMod)
        {
            if (Input.GetMouseButtonDown(1))
            {
                _buildingController.DisableBuildingMod();
            }

            if (Input.GetKeyDown(CancelKey))
            {
                _buildingController.DisableBuildingMod();
            }

            if (Input.GetMouseButtonDown(0))
            {
                var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 100f, buildLayerMask))
                {
                    _unitController.Build(hit.point, isShiftButtonPressed);
                }
            }

            return;
        }

        if (_buildingController.BuildingMenuMod)
        {
            if (Input.GetKeyDown(CancelKey))
            {
                _buildingController.DisableBuildingMenuMod();
                return;
            }

            if (AlphabetKeyDown(out KeyCode alphabetKeyDown))
            {
                _buildingController.EnableBuildingMod(alphabetKeyDown);
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 100f, clickLayerMask))
            {
                selectionStarted = true;
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
            if (selectionStarted)
            {
                var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100f, clickLayerMask))
                {
                    _unitController.EndSelection(hit.point, isShiftButtonPressed);
                    _selectionBoxController.EndDrawSelection();

                    var targetGameObject = hit.transform.gameObject;
                    if (targetGameObject.layer == (int)Layer.Unit)
                    {
                        float timeSinceLastClick = Time.time - lastClickTime;
                        if (timeSinceLastClick <= GameConstants.DoubleClickTime)
                        {
                            _unitController.OnDoubleClick(targetGameObject, isShiftButtonPressed);
                            lastClickTime = -1f;
                        }
                        else
                        {
                            lastClickTime = Time.time;
                        }
                    }
                }
                selectionStarted = false;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, clickLayerMask))
            {
                var targetGameObject = hit.transform.gameObject;
                if (targetGameObject.layer == (int)Layer.MovementSurface)
                {
                    _unitController.OnGroundRightClick(hit.point, isShiftButtonPressed);
                }
                else if (targetGameObject.layer == (int)Layer.Unit)
                {
                    _unitController.OnUnitRightClick(targetGameObject, isShiftButtonPressed);
                }
                else if (targetGameObject.layer == (int)Layer.HarvestedResource)
                {
                    _unitController.RightClickOnResource(targetGameObject, hit.point, isShiftButtonPressed);
                }
            }
        }

        if (currentKeyPressed != KeyCode.None)
        {
            if (Input.GetKeyUp(currentKeyPressed))
            {
                _skillController.CommandSkillCast(currentKeyPressed, isShiftButtonPressed);
                currentKeyPressed = KeyCode.None;
            }

            return;
        }

        if (currentKeyPressed == KeyCode.None && GetAllowedKeyDown(out var keyCode))
        {
            var isSkillExists = _skillController.PrepareSkillCast(keyCode);
            if (isSkillExists)
            {
                currentKeyPressed = keyCode;
            }
        }

        if (Input.GetKeyDown(CancelKey))
        {
            _unitController.OnCancelClick();
        }

        if (KeypadCodeDown(out KeyCode keypadCode, out int num))
        {
            _unitController.ProduceUnit(num);
        }

        if (Input.GetKeyDown(FixScreenKey))
        {
            _cameraController.SwitchFixScreen();
        }

        if (Input.GetKeyDown(OpenBuildingMenuKey))
        {
            _buildingController.EnableBuildingMenuMod();
        }

        if (Input.GetKeyDown(AClickKey))
        {
            aClickPressed = true;
        }

        if (Input.GetKeyDown(HoldKey))
        {
            _unitController.OnHoldKeyDown(isShiftButtonPressed);
        }
    }

    private bool GetAllowedKeyDown(out KeyCode keyCode)
    {
        foreach (KeyCode key in allKeyCodes)
        {
            if (usedKeys.Contains(key))
                continue;

            if (Input.GetKeyDown(key))
            {
                keyCode = key;
                return true;
            }
        }

        keyCode = KeyCode.None;
        return false;
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

    bool AlphabetKeyDown(out KeyCode key)
    {
        if (!Input.anyKeyDown) // Проверяем, нажата ли любая клавиша
        {
            key = KeyCode.None;
            return false;
        }

        foreach (KeyCode potentialKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (potentialKey >= KeyCode.A && potentialKey <= KeyCode.Z && Input.GetKeyDown(potentialKey))
            {
                key = potentialKey;
                return true;
            }
        }

        key = KeyCode.None;
        return false;
    }

    private void UpdateMousePosition()
    {
        Ray ray = _cameraController.ControlledCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100f, LayerMask.GetMask(Layer.MovementSurface.ToString())))
        {
            Vector3 worldPos = hit.point;
            Vector3 snappedPos = new Vector3(
                Mathf.Round(worldPos.x / snapStep) * snapStep,
                Mathf.Round(worldPos.y / snapStep) * snapStep,
                Mathf.Round(worldPos.z / snapStep) * snapStep
            );

            if (snappedPos != lastSnappedPosition)
            {
                GameObject unitUnderCursor = null;

                if (Physics.Raycast(ray, out var unitHit, 100f, LayerMask.GetMask(Layer.Unit.ToString())))
                {
                    unitUnderCursor = unitHit.transform.gameObject;
                }

                var cursorPosition = worldPos;

                _playerEventController.OnCursorMoved(cursorPosition, unitUnderCursor);

                lastSnappedPosition = snappedPos;
            }
        }
    }
}
