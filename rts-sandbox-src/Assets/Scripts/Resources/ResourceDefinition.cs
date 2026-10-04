using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

/// <summary>
/// One resource of the game (M-012, T-052): gold, wood, food — and whatever is
/// added later as a new asset, without touching the code. Everything that names
/// a resource — a price, a store, what a worker can cut, what a mine gives —
/// points at one of these. Which resources the game has and in what order the
/// HUD shows them is the list on GameResources.
/// </summary>
[CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource")]
public class ResourceDefinition : ScriptableObject
{
    [Tooltip("The name shown to the player.")]
    public string DisplayName;

    [Tooltip("How the resource behaves: mined from a mine, cut and carried to a store, " +
             "or supply — taken by units while they live and given back when they die.")]
    public ResourceType Type;

    [Tooltip("Shown next to the amount in the HUD and on prices (M-022).")]
    public Texture2D Icon;

    public bool IsSupply => Type == ResourceType.SupplyResource;
}
