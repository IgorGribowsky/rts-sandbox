using UnityEngine;

/// <summary>
/// Darkens what the camera sees by the fog of war (M-027). Hung on the camera
/// by <see cref="FogOfWar"/> at start, not set up in the scene.
///
/// A full-screen pass over the finished picture: every pixel finds its point
/// on the ground from the depth texture and looks the fog up there. The depth
/// texture is already drawn for the soft shadows of the sun, so the pass costs
/// one blit. Screen UI is drawn after it and is not darkened.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FogOfWarEffect : MonoBehaviour
{
    private const int ScreenPass = 0;

    private static readonly int FogSmoothId = Shader.PropertyToID("_FogSmooth");
    private static readonly int NoiseTexId = Shader.PropertyToID("_NoiseTex");
    private static readonly int FogAreaId = Shader.PropertyToID("_FogArea");
    private static readonly int UnexploredColorId = Shader.PropertyToID("_UnexploredColor");
    private static readonly int ExploredId = Shader.PropertyToID("_Explored");
    private static readonly int ExploredTintId = Shader.PropertyToID("_ExploredTint");
    private static readonly int CloudColorId = Shader.PropertyToID("_CloudColor");
    private static readonly int CloudsId = Shader.PropertyToID("_Clouds");
    private static readonly int WobbleId = Shader.PropertyToID("_Wobble");
    private static readonly int CameraPosId = Shader.PropertyToID("_FogCameraPos");
    private static readonly int RayBottomLeftId = Shader.PropertyToID("_RayBL");
    private static readonly int RayBottomRightId = Shader.PropertyToID("_RayBR");
    private static readonly int RayTopLeftId = Shader.PropertyToID("_RayTL");
    private static readonly int RayTopRightId = Shader.PropertyToID("_RayTR");

    private FogOfWar _fog;
    private Material _material;
    private Camera _camera;
    private readonly Vector3[] _corners = new Vector3[4];

    public void Init(FogOfWar fog, Material material)
    {
        _fog = fog;
        _material = material;
    }

    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth;
    }

    private void OnDestroy()
    {
        if (_material != null)
        {
            Destroy(_material);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_fog == null || _material == null || _fog.SmoothTexture == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Rays from the camera to the four corners of the far plane, in world
        // space: a pixel's point is the camera plus its ray times its depth.
        _camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), _camera.farClipPlane,
            Camera.MonoOrStereoscopicEye.Mono, _corners);
        var view = _camera.transform;

        _material.SetVector(RayBottomLeftId, view.TransformVector(_corners[0]));
        _material.SetVector(RayTopLeftId, view.TransformVector(_corners[1]));
        _material.SetVector(RayTopRightId, view.TransformVector(_corners[2]));
        _material.SetVector(RayBottomRightId, view.TransformVector(_corners[3]));
        _material.SetVector(CameraPosId, view.position);

        var area = _fog.Area;
        _material.SetTexture(FogSmoothId, _fog.SmoothTexture);
        _material.SetTexture(NoiseTexId, _fog.NoiseTexture);
        _material.SetVector(FogAreaId, new Vector4(area.xMin, area.yMin, 1f / area.width, 1f / area.height));
        _material.SetColor(UnexploredColorId, _fog.UnexploredColor);
        _material.SetVector(ExploredId, new Vector4(_fog.ExploredDarkening, _fog.ExploredDesaturation, 0f, 0f));
        _material.SetColor(ExploredTintId, _fog.ExploredTint);
        _material.SetColor(CloudColorId, _fog.CloudColor);

        // Strength, 1 / size, and how far the clouds have drifted, in metres.
        var drift = _fog.CloudSpeed * Time.time;
        _material.SetVector(CloudsId, new Vector4(
            _fog.CloudStrength, 1f / Mathf.Max(1f, _fog.CloudScale), drift, 0f));

        // How far the edge wavers, metres, and how fast its waves crawl.
        _material.SetVector(WobbleId, new Vector4(_fog.EdgeWobble, Time.time * 0.03f, 0f, 0f));

        Graphics.Blit(source, destination, _material, ScreenPass);
    }
}
