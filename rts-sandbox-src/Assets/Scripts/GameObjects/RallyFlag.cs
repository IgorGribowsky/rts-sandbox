using UnityEngine;

/// <summary>
/// The flag of a rally point (T-030, M-011). A placeholder built from
/// primitives, coloured as the team: a pole and a cloth, no colliders, so
/// clicks go through it to the ground. One per building, shown only while
/// that building is selected.
/// </summary>
public class RallyFlag : MonoBehaviour
{
    private const float PoleHeight = 2.4f;
    private const float PoleThickness = 0.1f;
    private static readonly Vector3 ClothSize = new Vector3(1.1f, 0.7f, 0.05f);

    private static readonly Color PoleColor = new Color(0.35f, 0.25f, 0.15f);

    public static RallyFlag Create(Color teamColor)
    {
        var root = new GameObject("RallyFlag");
        var flag = root.AddComponent<RallyFlag>();

        var pole = MakePart(PrimitiveType.Cylinder, root.transform, PoleColor);
        pole.localScale = new Vector3(PoleThickness, PoleHeight / 2f, PoleThickness);
        pole.localPosition = new Vector3(0f, PoleHeight / 2f, 0f);

        var cloth = MakePart(PrimitiveType.Cube, root.transform, teamColor);
        cloth.localScale = ClothSize;
        cloth.localPosition = new Vector3(ClothSize.x / 2f, PoleHeight - ClothSize.y / 2f, 0f);

        root.SetActive(false);
        return flag;
    }

    public void Place(Vector3 point)
    {
        point.y = 0f;
        transform.position = point;
    }

    public void SetVisible(bool visible)
    {
        if (gameObject.activeSelf != visible)
        {
            gameObject.SetActive(visible);
        }
    }

    private static Transform MakePart(PrimitiveType type, Transform parent, Color color)
    {
        var part = GameObject.CreatePrimitive(type);
        Destroy(part.GetComponent<Collider>());
        part.transform.SetParent(parent, false);

        var renderer = part.GetComponent<Renderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var block = new MaterialPropertyBlock();
        block.SetColor("_Color", color);
        renderer.SetPropertyBlock(block);

        return part.transform;
    }
}
