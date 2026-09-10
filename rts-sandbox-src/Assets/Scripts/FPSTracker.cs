using UnityEngine;

/// <summary>
/// Debug frame rate counter. Draws the current FPS on screen instead of
/// writing it to the console: a per-frame Debug.Log drowns real errors and
/// distorts the very measurement it reports, because the editor collects a
/// stack trace on every call.
/// </summary>
public class FPSTracker : MonoBehaviour
{
    private const float SmoothingFactor = 0.1f;

    [Tooltip("Draw the counter on screen. Turn off to keep the tracker silent.")]
    public bool ShowOnScreen = true;

    [Tooltip("Smoothed frame time in seconds. Visible in the inspector at runtime.")]
    public float deltaTime;

    /// <summary>Smoothed frames per second, rounded up.</summary>
    public int Fps => deltaTime > 0f ? Mathf.CeilToInt(1.0f / deltaTime) : 0;

    private GUIStyle _style;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * SmoothingFactor;
    }

    void OnGUI()
    {
        if (!ShowOnScreen)
            return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = Mathf.Max(14, Screen.height / 50),
            };
            _style.normal.textColor = Color.yellow;
        }

        var area = new Rect(Screen.width - 110f, 6f, 100f, _style.fontSize + 6f);
        GUI.Label(area, Fps + " FPS", _style);
    }
}
