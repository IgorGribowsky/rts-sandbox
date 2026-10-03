using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Placer prefabs: the things a level designer drags into the scene by hand.
///
/// One prefab per <see cref="UnitTypeData"/>: a root with <see cref="UnitPlacer"/>
/// and an editor-only copy of the body's meshes under it, so the scene shows
/// what stands where. The copy keeps nothing but transforms, meshes and
/// materials — no colliders, no agent, no canvas — and is tagged EditorOnly,
/// so it never reaches a build. At start <see cref="SceneUnitsBootstrapper"/>
/// replaces the placer, preview and all, with the real unit.
/// </summary>
public static class UnitPlacerTools
{
    private const string TypesFolder = "Assets/Data/UnitTypes";
    private const string PlacersFolder = "Assets/Prefabs/GameObjects/Units/Placers";
    private const string PreviewName = "Preview";
    private const string EditorOnlyTag = "EditorOnly";

    /// <summary>
    /// Builds a placer prefab for every unit type, or refreshes its preview
    /// when the prefab already exists. Run it again after adding a type or
    /// changing a body.
    ///
    /// An existing prefab keeps its root and its UnitPlacer untouched — only
    /// the preview is swapped. Rebuilding the root would give it new file IDs,
    /// and every placer in every scene would silently lose its team and
    /// overrides.
    /// </summary>
    [MenuItem("Tools/Units/Build Placer Prefabs")]
    public static void BuildPlacerPrefabs()
    {
        EnsureFolder(PlacersFolder);

        var created = 0;
        var refreshed = 0;

        foreach (var type in LoadTypes())
        {
            if (type.BodyPrefab == null)
            {
                Debug.LogError($"Unit type '{type.name}' has no body — no placer built for it.", type);
                continue;
            }

            var path = GetPlacerPath(type);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                var root = new GameObject(GetPlacerName(type));
                try
                {
                    root.AddComponent<UnitPlacer>().Type = type;
                    CreatePreview(type, root.transform);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    created++;
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }

                continue;
            }

            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var placer = contents.GetComponent<UnitPlacer>();
                if (placer == null)
                {
                    Debug.LogError($"'{path}' has no UnitPlacer on its root — left as it is.");
                    continue;
                }

                placer.Type = type;

                var oldPreview = contents.transform.Find(PreviewName);
                if (oldPreview != null)
                {
                    Object.DestroyImmediate(oldPreview.gameObject);
                }

                CreatePreview(type, contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                refreshed++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Placer prefabs: {created} created, {refreshed} refreshed, in {PlacersFolder}.");
    }

    /// <summary>
    /// Brings the open scenes over to placer prefabs: bare UnitPlacer markers
    /// and instances of the old per-unit prefabs both become placer prefab
    /// instances, keeping name, place, rotation, size, team and overrides.
    /// Adds a <see cref="SceneUnitsBootstrapper"/> to the scene's
    /// SceneController when the scene has none, and saves the scene.
    /// </summary>
    [MenuItem("Tools/Units/Convert Scene To Placer Prefabs")]
    public static void ConvertOpenScenes()
    {
        var typesByName = new Dictionary<string, UnitTypeData>();
        foreach (var type in LoadTypes())
        {
            typesByName[type.name] = type;
        }

        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                ConvertScene(scene, typesByName);
            }
        }
    }

    private static void ConvertScene(Scene scene, Dictionary<string, UnitTypeData> typesByName)
    {
        var markers = 0;
        var oldInstances = 0;
        var failed = 0;

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var placer in root.GetComponentsInChildren<UnitPlacer>(true))
            {
                if (PrefabUtility.IsPartOfPrefabInstance(placer))
                {
                    continue;
                }

                var replaced = Replace(placer.gameObject, placer.Type, placer.TeamId, placer);
                if (replaced) markers++; else failed++;
            }
        }

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform == null || !PrefabUtility.IsOutermostPrefabInstanceRoot(transform.gameObject))
                {
                    continue;
                }

                var source = PrefabUtility.GetCorrespondingObjectFromSource(transform.gameObject);
                var sourcePath = AssetDatabase.GetAssetPath(source);
                if (source == null || sourcePath.StartsWith(PlacersFolder) || !typesByName.TryGetValue(source.name, out var type))
                {
                    continue;
                }

                // An old per-unit prefab: its team lived on TeamMember.
                var teamMember = transform.GetComponent<TeamMember>();
                var teamId = teamMember != null ? teamMember.TeamId : 1;

                var replaced = Replace(transform.gameObject, type, teamId, null);
                if (replaced) oldInstances++; else failed++;
            }
        }

        var bootstrapperAdded = EnsureBootstrapper(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"Scene '{scene.name}': {markers} markers and {oldInstances} old unit prefabs turned into placer prefabs, " +
                  $"{failed} failed, bootstrapper {(bootstrapperAdded ? "added" : "already there")}.");
    }

    /// <summary>Puts a placer prefab instance where <paramref name="old"/> stands, then removes the old object.</summary>
    private static bool Replace(GameObject old, UnitTypeData type, int teamId, UnitPlacer overridesFrom)
    {
        if (type == null)
        {
            Debug.LogError($"'{old.name}' has no unit type — left in place.", old);
            return false;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetPlacerPath(type));
        if (prefab == null)
        {
            Debug.LogError($"No placer prefab for '{type.name}' — run Tools/Units/Build Placer Prefabs first. '{old.name}' left in place.", old);
            return false;
        }

        var oldTransform = old.transform;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, old.scene);
        var transform = instance.transform;

        transform.SetParent(oldTransform.parent, false);
        transform.SetSiblingIndex(oldTransform.GetSiblingIndex());
        transform.localPosition = oldTransform.localPosition;
        transform.localRotation = oldTransform.localRotation;
        transform.localScale = oldTransform.localScale;
        instance.name = old.name;
        instance.SetActive(old.activeSelf);

        var placer = instance.GetComponent<UnitPlacer>();
        placer.TeamId = teamId;
        if (overridesFrom != null)
        {
            placer.ResourcesAmountOverride = overridesFrom.ResourcesAmountOverride;
            placer.ObstacleSizeOverride = overridesFrom.ObstacleSizeOverride;
        }

        Undo.RegisterCreatedObjectUndo(instance, "Convert to placer prefab");
        Undo.DestroyObjectImmediate(old);
        return true;
    }

    private static bool EnsureBootstrapper(Scene scene)
    {
        GameObject sceneController = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<SceneUnitsBootstrapper>(true) != null)
            {
                return false;
            }

            // It is not always a root: in TestScene it sits under Settings.
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (sceneController == null && transform.name == "SceneController")
                {
                    sceneController = transform.gameObject;
                }
            }
        }

        if (sceneController == null)
        {
            Debug.LogError($"Scene '{scene.name}' has no SceneController — add a SceneUnitsBootstrapper by hand, or no unit will be built.");
            return false;
        }

        Undo.AddComponent<SceneUnitsBootstrapper>(sceneController);
        return true;
    }

    /// <summary>
    /// Copies the body's visible part: every object that carries a mesh, or
    /// leads to one, with its local transform. The body's root is laid out at
    /// zero under the placer, because the factory puts the unit exactly at
    /// the placer's position, rotation and size.
    /// </summary>
    private static void CreatePreview(UnitTypeData type, Transform parent)
    {
        var preview = CopyVisible(type.BodyPrefab.transform, parent);
        if (preview == null)
        {
            Debug.LogWarning($"Body of '{type.name}' has no meshes — its placer shows only the gizmo.", type);
            return;
        }

        preview.name = PreviewName;
        preview.transform.localPosition = Vector3.zero;
        preview.transform.localRotation = Quaternion.identity;
        preview.transform.localScale = Vector3.one;

        foreach (var t in preview.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.tag = EditorOnlyTag;
        }
    }

    private static GameObject CopyVisible(Transform source, Transform parent)
    {
        var mesh = GetMesh(source);
        var renderer = source.GetComponent<Renderer>();
        var hasMesh = mesh != null && renderer != null;

        GameObject copy = null;
        if (hasMesh)
        {
            copy = CreateCopy(source, parent);
            copy.AddComponent<MeshFilter>().sharedMesh = mesh;
            var copyRenderer = copy.AddComponent<MeshRenderer>();
            copyRenderer.sharedMaterials = renderer.sharedMaterials;
            copyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        foreach (Transform child in source)
        {
            // The bars' canvas has nothing to show before the unit exists.
            if (child.GetComponent<Canvas>() != null)
            {
                continue;
            }

            if (copy == null)
            {
                if (!HasMeshBelow(child))
                {
                    continue;
                }

                copy = CreateCopy(source, parent);
            }

            CopyVisible(child, copy.transform);
        }

        return copy;
    }

    private static GameObject CreateCopy(Transform source, Transform parent)
    {
        var copy = new GameObject(source.name);
        copy.transform.SetParent(parent, false);
        copy.transform.localPosition = source.localPosition;
        copy.transform.localRotation = source.localRotation;
        copy.transform.localScale = source.localScale;
        copy.SetActive(true);
        return copy;
    }

    private static bool HasMeshBelow(Transform source)
    {
        if (GetMesh(source) != null && source.GetComponent<Renderer>() != null)
        {
            return true;
        }

        foreach (Transform child in source)
        {
            if (child.GetComponent<Canvas>() == null && HasMeshBelow(child))
            {
                return true;
            }
        }

        return false;
    }

    private static Mesh GetMesh(Transform source)
    {
        var filter = source.GetComponent<MeshFilter>();
        if (filter != null)
        {
            return filter.sharedMesh;
        }

        var skinned = source.GetComponent<SkinnedMeshRenderer>();
        return skinned != null ? skinned.sharedMesh : null;
    }

    private static IEnumerable<UnitTypeData> LoadTypes()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:UnitTypeData", new[] { TypesFolder }))
        {
            var type = AssetDatabase.LoadAssetAtPath<UnitTypeData>(AssetDatabase.GUIDToAssetPath(guid));
            if (type != null)
            {
                yield return type;
            }
        }
    }

    private static string GetPlacerName(UnitTypeData type) => $"{type.name} Placer";

    private static string GetPlacerPath(UnitTypeData type) => $"{PlacersFolder}/{GetPlacerName(type)}.prefab";

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}
