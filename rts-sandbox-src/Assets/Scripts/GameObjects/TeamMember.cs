using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamMember : MonoBehaviour
{
    /// <summary>
    /// The material a model gives its team parts (doc 08): only slots with this
    /// material take the team colour. A model without it is not painted at all.
    /// </summary>
    public const string TeamColorMaterialName = "TeamColor";

    public int TeamId = 1;

    private TeamController _teamController;

    void Start()
    {
        _teamController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString())
            .GetComponent<TeamController>();
        var team = _teamController.Teams.FirstOrDefault(t => t.Id == TeamId);

        PaintTeamParts(gameObject, team.Color);
    }

    /// <summary>
    /// Paints the TeamColor slots of every renderer in the body. Returns the
    /// material copies it made, for a caller that has to destroy them itself.
    /// </summary>
    public static List<Material> PaintTeamParts(GameObject body, Color color)
    {
        var painted = new List<Material>();

        foreach (var renderer in body.GetComponentsInChildren<Renderer>(true))
        {
            var shared = renderer.sharedMaterials;
            if (!shared.Any(IsTeamColor))
            {
                continue;
            }

            // Copies made for this renderer only; the model's material stays as it is.
            var materials = renderer.materials;
            for (var i = 0; i < materials.Length; i++)
            {
                if (IsTeamColor(shared[i]))
                {
                    materials[i].color = color;
                }

                painted.Add(materials[i]);
            }
        }

        return painted;
    }

    private static bool IsTeamColor(Material material)
    {
        return material != null && material.name == TeamColorMaterialName;
    }
}
