using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A copy of a unit type's 3D body with nothing in it but what is drawn: for
/// the card pictures (M-024) and the ghost of a building being placed (M-010).
///
/// The body lies switched off on disk (M-021): it is copied switched off and
/// cleaned before anything in it wakes up. An agent or obstacle waking up
/// away from the NavMesh complains, colliders would be hit by the game's rays,
/// scripts would join the game as a unit, and the bar canvas has nothing to show.
/// </summary>
public static class UnitBodyCopy
{
    public static GameObject Create(UnitTypeData type, Vector3 position, Quaternion rotation, int layer)
    {
        var body = Object.Instantiate(type.BodyPrefab, position, rotation);
        body.SetActive(false);
        StripToMeshes(body);
        SetLayer(body.transform, layer);
        body.SetActive(true);
        return body;
    }

    private static void StripToMeshes(GameObject body)
    {
        foreach (var agent in body.GetComponentsInChildren<NavMeshAgent>(true)) Object.DestroyImmediate(agent);
        foreach (var obstacle in body.GetComponentsInChildren<NavMeshObstacle>(true)) Object.DestroyImmediate(obstacle);
        foreach (var collider in body.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        foreach (var canvas in body.GetComponentsInChildren<Canvas>(true))
        {
            if (canvas.gameObject != body) Object.DestroyImmediate(canvas.gameObject);
        }
        foreach (var behaviour in body.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(behaviour);
    }

    private static void SetLayer(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform child in root)
        {
            SetLayer(child, layer);
        }
    }
}
