using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How levels are earned (M-026). One table can serve many types; a type
/// with levels points to it. All the numbers are placeholders for play
/// testing and are meant to be tuned here, in the inspector.
/// </summary>
[CreateAssetMenu(fileName = "ExperienceTable", menuName = "RTS/Experience Table", order = 1)]
public class ExperienceTable : ScriptableObject
{
    [Tooltip("Experience each level-up costs: the first entry takes level 1 to 2, " +
             "the next 2 to 3, and so on. The length of the list sets the top level.")]
    public List<int> LevelUpCosts = new List<int> { 100, 200, 300, 400, 500, 600, 700, 800, 900 };

    [Tooltip("An enemy dying this close to a unit with levels shares its reward with it.")]
    public float ShareRadius = 15f;
}
