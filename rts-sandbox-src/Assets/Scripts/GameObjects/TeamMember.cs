using Assets.Scripts.Infrastructure.Enums;
using System.Linq;
using UnityEngine;

public class TeamMember : MonoBehaviour
{
    public int TeamId = 1;

    private TeamController _teamController;

    void Start()
    {
        _teamController = GameObject.FindGameObjectWithTag(Tag.GameController.ToString())
            .GetComponent<TeamController>();
        var team = _teamController.Teams.FirstOrDefault(t => t.Id == TeamId);

        Renderer renderer = GetComponent<Renderer>();

        Material uniqueMaterial = renderer.material;

        uniqueMaterial.color = team.Color;
    }

    void Update()
    {
        
    }
}
