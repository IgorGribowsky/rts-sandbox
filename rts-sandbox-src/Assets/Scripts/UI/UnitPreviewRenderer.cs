using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Pictures of units and buildings for the cards (M-024), taken from their
    /// own 3D body, not drawn by hand: a new model shows up on its card with no
    /// extra work. Each type is photographed once, the first time a card needs
    /// it, and kept for the rest of the game.
    ///
    /// The body is copied far below the map on a layer only the preview camera
    /// sees, stripped of everything but its meshes, painted in the player's
    /// team colour the way TeamMember paints it in the game, shot with a
    /// transparent background from the angle of the game camera, and removed.
    ///
    /// Removed at once, not at the end of the frame: all the cards of a menu
    /// are shot in one frame, and a body left standing would get into the
    /// next card's picture.
    /// </summary>
    public static class UnitPreviewRenderer
    {
        private const int Size = 256;
        private const int PreviewLayer = 31;
        private static readonly Vector3 StagePosition = new Vector3(0f, -2000f, 0f);

        private static readonly Dictionary<UnitTypeData, RenderTexture> _cache = new Dictionary<UnitTypeData, RenderTexture>();

        /// <summary>Team whose colour the bodies are painted in: the player's.</summary>
        public static int TeamId { get; set; } = 1;

        public static RenderTexture Get(UnitTypeData type)
        {
            if (type == null || type.BodyPrefab == null)
            {
                return null;
            }

            if (_cache.TryGetValue(type, out var cached) && cached != null && cached.IsCreated())
            {
                return cached;
            }

            var texture = Render(type);
            _cache[type] = texture;
            return texture;
        }

        private static RenderTexture Render(UnitTypeData type)
        {
            var body = UnitBodyCopy.Create(type, StagePosition, type.BodyPrefab.transform.rotation, PreviewLayer);

            var holder = new GameObject("Unit Preview Camera");
            Material paintedMaterial = null;
            try
            {
                paintedMaterial = PaintInTeamColour(body);

                var renderers = body.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    return null;
                }

                var bounds = renderers[0].bounds;
                foreach (var r in renderers)
                {
                    bounds.Encapsulate(r.bounds);
                }

                var camera = holder.AddComponent<Camera>();
                camera.enabled = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.cullingMask = 1 << PreviewLayer;
                camera.fieldOfView = 30f;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 500f;

                // The way the player sees units: from above and a little to the side.
                var direction = Quaternion.Euler(40f, -35f, 0f) * Vector3.forward;
                var radius = Mathf.Max(0.1f, bounds.extents.magnitude);
                var distance = radius / Mathf.Sin(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.05f;
                camera.transform.position = bounds.center - direction * distance;
                camera.transform.rotation = Quaternion.LookRotation(direction);

                var texture = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32)
                {
                    name = "Preview " + type.name,
                    antiAliasing = 4,
                };

                camera.targetTexture = texture;
                camera.Render();
                camera.targetTexture = null;
                return texture;
            }
            finally
            {
                if (paintedMaterial != null)
                {
                    Object.Destroy(paintedMaterial);
                }

                body.SetActive(false);
                Object.Destroy(body);
                Object.Destroy(holder);
            }
        }

        /// <summary>
        /// The same as TeamMember: the root renderer gets the team colour. Returns
        /// the material copy made for it, which the caller throws away.
        /// </summary>
        private static Material PaintInTeamColour(GameObject body)
        {
            var renderer = body.GetComponent<Renderer>();
            var team = GameServices.TeamController?.Teams?.FirstOrDefault(t => t.Id == TeamId);
            if (renderer == null || team == null)
            {
                return null;
            }

            var copy = renderer.material;
            copy.color = team.Color;
            return copy;
        }
    }
}
