using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turns the markers standing in the scene into real units, before anything
/// else has had a chance to look for them.
///
/// Runs in Awake, but late: with an execution order of 100 every controller
/// has already woken up and found its neighbours, while nobody has reached
/// Start yet. So a unit built here goes through Start together with the rest
/// of the scene, exactly as a unit dragged in by hand used to.
/// </summary>
[DefaultExecutionOrder(100)]
public class SceneUnitsBootstrapper : MonoBehaviour
{
    [Tooltip("How many markers were turned into units. Filled at start, for a look in the inspector.")]
    public int BuiltUnitsCount;

    private void Awake()
    {
        var placers = new List<UnitPlacer>(FindObjectsByType<UnitPlacer>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        BuiltUnitsCount = 0;

        foreach (var placer in placers)
        {
            if (placer.Type == null)
            {
                Debug.LogError($"UnitPlacer '{placer.name}' has no type — nothing was built there.", placer);
                continue;
            }

            var placerTransform = placer.transform;

            var unit = UnitFactory.Create(
                placer.Type,
                placerTransform.position,
                placerTransform.rotation,
                placer.TeamId,
                built =>
                {
                    // Size belongs to the marker: it is set while the unit is
                    // still switched off, so the NavMesh agent wakes up with
                    // its final size and does not jump.
                    built.transform.localScale = placerTransform.localScale;
                    placer.ApplyOverrides(built);
                });

            if (unit == null)
            {
                continue;
            }

            BuiltUnitsCount++;

            Destroy(placer.gameObject);
        }
    }
}
