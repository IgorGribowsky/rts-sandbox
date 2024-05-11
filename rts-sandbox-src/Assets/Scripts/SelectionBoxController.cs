using Unity.VisualScripting;
using UnityEngine;

public class SelectionBoxController : MonoBehaviour
{
    public Camera ControlledCamera;
    public RectTransform selectionBox;
    public Vector3 StartPosition{ get; set; }

    void Start()
    {
        if (ControlledCamera == null)
        {
            ControlledCamera = Camera.main;
        }
    }

    void Update()
    {
    }

    public void StartDrawSelection(Vector3 point)
    {
        StartPosition = point;
        if (!selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(true);
        }
    }

    public void DrawSelection(Vector2 screenPoint)
    {
        Vector2 screenStartPosition = ControlledCamera.WorldToScreenPoint(StartPosition);
        var dx = screenPoint.x - screenStartPosition.x;
        var dy = screenPoint.y - screenStartPosition.y;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(dx), Mathf.Abs(dy));

        selectionBox.anchoredPosition = screenStartPosition + new Vector2 (dx/2, dy/2);
    }

    public void EndDrawSelection()
    {
        if (selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(false);
        }
    }
}
