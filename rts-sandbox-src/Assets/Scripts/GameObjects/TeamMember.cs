using System.Linq;
using UnityEngine;

public class TeamMember : MonoBehaviour
{
    public int TeamId = 1;

    void Start()
    {
        var team = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<TeamController>()
            .Teams.FirstOrDefault(t => t.Id == TeamId);

        Renderer renderer = GetComponent<Renderer>();

        Material uniqueMaterial = renderer.material;

        uniqueMaterial.color = team.Color;
    }

    void Update()
    {
        
    }
}
