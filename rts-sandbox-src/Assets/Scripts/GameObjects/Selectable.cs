using Assets.Scripts;
using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public GameObject SelectionCirclePrefab;

    public bool IsSelected = false;

    private GameObject selectedCircle;
    private Renderer[] circleRenderers;
    private MaterialPropertyBlock circleBlock;

    void Start()
    {
        EnsureCircle();
        selectedCircle.SetActive(IsSelected);
    }

    public void SetSelectionState(bool isSelected)
    {
        IsSelected = isSelected;
        EnsureCircle();

        if (IsSelected)
        {
            // Asked on every selection, not once: a mine changes hands (M-013).
            PaintCircle(RelationColor());
        }

        selectedCircle.SetActive(IsSelected);
    }

    private void EnsureCircle()
    {
        if (selectedCircle != null)
        {
            return;
        }

        selectedCircle = GameObject.Instantiate(SelectionCirclePrefab);
        selectedCircle.transform.parent = gameObject.transform;
        selectedCircle.transform.localPosition = Vector3.zero;

        var position = selectedCircle.transform.position;
        position.y = 0;
        selectedCircle.transform.position = position;

        circleRenderers = selectedCircle.GetComponentsInChildren<Renderer>(true);
        circleBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Green under the player's own, red under an enemy, yellow under an ally
    /// that is not the player's own or a peaceful team (M-003, M-009).
    /// </summary>
    private Color RelationColor()
    {
        var gameController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString())
            .GetComponent<GameController>();
        var playerTeamId = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerTeamMember>().TeamId;

        var teamMember = GetComponent<TeamMember>();
        if (teamMember == null)
        {
            return gameController.FriendlySelectionColor;
        }

        if (teamMember.TeamId == playerTeamId)
        {
            return gameController.OwnSelectionColor;
        }

        return GameServices.TeamController.GetEnemyTeams(playerTeamId).Contains(teamMember.TeamId)
            ? gameController.EnemySelectionColor
            : gameController.FriendlySelectionColor;
    }

    /// <summary>A property block: the circles share one material and it stays untouched.</summary>
    private void PaintCircle(Color color)
    {
        circleBlock.SetColor(ColorId, color);
        foreach (var circleRenderer in circleRenderers)
        {
            circleRenderer.SetPropertyBlock(circleBlock);
        }
    }
}
