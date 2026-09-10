using Assets.Scripts.GameObjects;
using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.SkillsSection.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a unit, a building or a resource out of a <see cref="UnitTypeData"/>
/// asset: takes the body prefab, hangs the scripts the type asks for, and pours
/// the numbers in. The single place where a unit comes into the world — the
/// scene bootstrapper, unit production and the builder all call it.
///
/// The body is kept switched off while the scripts are being added, so every
/// Awake runs once, at the end, on a unit that is already whole.
/// </summary>
public static class UnitFactory
{
    /// <param name="configure">
    /// Runs on the finished but still switched-off unit — the place for what
    /// belongs to this one instance and not to the type: how much gold is left
    /// in this mine, how big this particular obstacle is.
    /// </param>
    public static GameObject Create(
        UnitTypeData type,
        Vector3 position,
        Quaternion rotation,
        int teamId,
        Action<GameObject> configure = null)
    {
        if (type == null)
        {
            Debug.LogError("UnitFactory: no type given, nothing to build.");
            return null;
        }

        if (type.BodyPrefab == null)
        {
            Debug.LogError($"UnitFactory: type '{type.name}' has no BodyPrefab.");
            return null;
        }

        var unit = UnityEngine.Object.Instantiate(type.BodyPrefab, position, rotation);
        var wasActive = unit.activeSelf;
        if (wasActive)
        {
            unit.SetActive(false);
        }

        unit.name = string.IsNullOrEmpty(type.DisplayName) ? type.name : type.DisplayName;

        Assemble(unit, type, teamId);

        configure?.Invoke(unit);

        unit.SetActive(true);

        return unit;
    }

    /// <summary>
    /// Half the size of the body, the way Renderer.bounds.extents used to give
    /// it before the type became data. Read from the mesh, because the body
    /// prefab is stored switched off and its renderer has no bounds until it
    /// is in the scene.
    /// </summary>
    public static Vector3 GetBodyExtents(UnitTypeData type)
    {
        if (type == null || type.BodyPrefab == null)
        {
            return Vector3.zero;
        }

        var meshFilter = type.BodyPrefab.GetComponentInChildren<MeshFilter>(true);
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return Vector3.zero;
        }

        return Vector3.Scale(meshFilter.sharedMesh.bounds.extents, type.BodyPrefab.transform.lossyScale);
    }

    /// <summary>
    /// Everything except switching the unit on. Split out so an editor tool can
    /// look at what a type produces without running the game.
    /// </summary>
    public static void Assemble(GameObject unit, UnitTypeData type, int teamId)
    {
        if (type.HasValues)
        {
            unit.AddComponent<UnitEventManager>();

            var values = unit.AddComponent<UnitValues>();
            values.Type = type;
            values.Stats = type.Stats.Clone();
        }

        if (type.HasTeam)
        {
            unit.AddComponent<TeamMember>().TeamId = teamId;
        }

        if (type.IsSelectable)
        {
            unit.AddComponent<Selectable>().SelectionCirclePrefab = type.SelectionCirclePrefab;
        }

        var needsBars = type.HasHealth || type.HasMana || type.Building.CanProduceUnits;
        if (needsBars)
        {
            unit.AddComponent<BarsContaining>();
        }

        if (type.HasHealth)
        {
            unit.AddComponent<UnitHealthPoints>();

            var bar = unit.AddComponent<HealthPointsBar>();
            bar.BarTemplate = type.HealthBarTemplate;
            bar.Priority = type.HealthBarPriority;
        }

        if (type.CallsAlliesWhenAttacked)
        {
            unit.AddComponent<CallingToAttackWhenAttacked>();
        }

        if (type.IsMobile)
        {
            unit.AddComponent<NavMeshMovement>();
        }

        if (type.TakesCommands)
        {
            unit.AddComponent<UnitCommandManager>();
            unit.AddComponent<UnitBehaviourManager>().SetBehaviours(type.Behaviours);
        }

        if (type.IsBuildingObject || type.IsResourceObject)
        {
            unit.AddComponent<BuildingValues>().Stats = type.Building.Clone();
        }

        if (type.Builder.IsBuilder)
        {
            unit.AddComponent<BuilderValues>().Stats = type.Builder.Clone();
        }

        if (type.HasHarvesting)
        {
            unit.AddComponent<HarvestingValues>().Stats = type.Harvesting.Clone();
        }

        if (type.IsResourceObject)
        {
            var resourceValues = unit.AddComponent<ResourceValues>();
            resourceValues.IsMine = type.IsMine;
            resourceValues.IsHeldMine = type.IsHeldMine;
            resourceValues.IsHarvestedResource = type.IsHarvestedResource;
            resourceValues.ResourceName = type.ResourceName;
            resourceValues.ResourcesAmount = type.ResourcesAmount;

            if (type.IsHarvestedResource)
            {
                unit.AddComponent<HarvestedResource>();
            }
        }

        // Building goes on after BuildingValues and ResourceValues: its Awake
        // reads both.
        if (type.IsBuildingObject)
        {
            unit.AddComponent<Building>();
        }

        if (type.IsHeldMine)
        {
            var heldMine = unit.AddComponent<HeldMine>();
            heldMine.ParentMine = type.ParentMineType;
            heldMine.MiningRate = type.MiningRate;
            heldMine.MiningValue = type.MiningValue;
            heldMine.MinersMaxCount = type.MinersMaxCount;
        }

        if (type.Building.CanProduceUnits)
        {
            unit.AddComponent<UnitProducing>();

            var bar = unit.AddComponent<ProducingBar>();
            bar.BarTemplate = type.ProducingBarTemplate;
            bar.Priority = type.ProducingBarPriority;
        }

        if (type.StoresHarvestedResources)
        {
            unit.AddComponent<HarvestedResourcesStorage>().StoredResources =
                new List<Assets.Scripts.Infrastructure.Enums.ResourceName>(type.StoredResources);
        }

        if (type.RequiresSupply)
        {
            unit.AddComponent<UnitSupplyRequirement>();
        }

        if (type.ProducesSupply)
        {
            unit.AddComponent<UnitSupplyProducer>();
        }

        if (type.HasSkills)
        {
            // A copy: the unit must not be able to edit the list inside the asset.
            unit.AddComponent<UnitSkills>().Skills = new List<UnitSkills.SkillSlot>(type.Skills);
        }

        if (type.HasMana)
        {
            unit.AddComponent<ManaValues>().Stats = type.Mana.Clone();
            unit.AddComponent<UnitManaPoints>();

            var bar = unit.AddComponent<ManaPointsBar>();
            bar.BarTemplate = type.ManaBarTemplate;
            bar.Priority = type.ManaBarPriority;
        }
    }
}
