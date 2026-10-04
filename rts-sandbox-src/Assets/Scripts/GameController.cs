using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool FriendlyFire = false;

    [Tooltip("How much of its price an unfinished building gives back when cancelled with Esc, in percent (M-010).")]
    [Range(0, 100)]
    public int BuildingCancelRefundPercent = 70;

    [Header("Fog of war (M-027)")]
    [Tooltip("The map is hidden under the fog. Can be switched in play mode. How it looks is on the FogOfWar component.")]
    public bool FogOfWarEnabled = true;

    [Header("Effects (M-019)")]
    [Tooltip("Stars over a stunned unit: a flat picture on RTS/GroundMark, turning by its _Spin.")]
    public Material StunStarsMaterial;

    [Header("Selection circle (M-003)")]
    [Tooltip("Under the player's own units.")]
    public Color OwnSelectionColor = new Color(0.428f, 1f, 0.099f);

    [Tooltip("Under enemies of the player.")]
    public Color EnemySelectionColor = new Color(1f, 0.15f, 0.1f);

    [Tooltip("Under allies that are not the player's own, and under peaceful teams.")]
    public Color FriendlySelectionColor = new Color(1f, 0.85f, 0.1f);
}
