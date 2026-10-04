using RtsSandbox.Rules;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What the player is shown of the other teams' units under the fog
/// (M-027, T-071.3). Decided after every pass of sight, for every unit.
///
/// Only what the player sees and can touch changes; the units themselves
/// play on exactly as before. That is why a hidden unit is not switched off,
/// not even its renderer: Renderer.enabled, or SetActive, would change its
/// bounds, which the target search and the distances are measured by, or stop
/// its scripts outright. Hidden is forceRenderingOff — drawn by nobody, its
/// shadow included, everything else intact.
///
/// * a hidden unit is not drawn, has no bars, is not on the minimap, cannot be
///   clicked (the input and the selection ask <see cref="FogOfWar.IsSeen"/>);
/// * a building or a mine — anything that cannot walk — seen once stays drawn
///   as remembered, without bars, and cannot be selected; destroyed while
///   nobody looked, it leaves a ghost — a copy of its meshes — until the
///   player looks there again;
/// * a player's unit attacking by order a target that went into the fog walks
///   to where the target was seen last; auto attack simply loses the target
///   (AutoAttackingBehaviourBase asks <see cref="FogOfWar.CanTarget"/>).
/// </summary>
public sealed class FogUnitSight
{
    /// <summary>A destroyed building the player still remembers.</summary>
    public sealed class Ghost
    {
        public GameObject Body;
        public Vector3 Position;
        public float Size;
        public int TeamId;
        public readonly List<Material> Materials = new List<Material>();
    }

    private readonly FogOfWar _fog;
    private readonly HashSet<int> _friendlyTeams;
    private readonly List<Renderer> _renderers = new List<Renderer>();
    private readonly List<Ghost> _ghosts = new List<Ghost>();
    private int _playerTeamId = int.MinValue;

    public FogUnitSight(FogOfWar fog, HashSet<int> friendlyTeams)
    {
        _fog = fog;
        _friendlyTeams = friendlyTeams;
    }

    /// <summary>Destroyed buildings the player has not yet seen gone.</summary>
    public IReadOnlyList<Ghost> Ghosts => _ghosts;

    public int PlayerTeamId
    {
        get => _playerTeamId;
        set => _playerTeamId = value;
    }

    /// <summary>
    /// After a pass of sight, or with the fog switched off (then everybody is
    /// visible again). True when anybody's sight changed.
    /// </summary>
    public bool Update(bool fogIsOn)
    {
        var changed = false;
        var all = UnitRegistry.All;

        for (var i = 0; i < all.Count; i++)
        {
            var record = all[i];
            if (record.GameObject == null)
            {
                continue;
            }

            changed |= Decide(record, fogIsOn);
        }

        UpdateGhosts(fogIsOn);

        if (fogIsOn)
        {
            FollowLostAttackTargets();
        }

        return changed;
    }

    /// <summary>A unit has just come in: hidden at once if it must be, not a pass later.</summary>
    public void OnRegistered(UnitRecord record, bool fogIsOn)
    {
        Decide(record, fogIsOn);
    }

    /// <summary>
    /// A unit is dying. A building the player remembers leaves its ghost: the
    /// player did not see it go.
    /// </summary>
    public void OnDied(GameObject dead, bool fogIsOn)
    {
        var record = UnitRegistry.Of(dead);
        if (!fogIsOn || record == null || record.Sight != FogSight.Remembered)
        {
            return;
        }

        var ghost = new Ghost
        {
            Body = new GameObject("Fog Ghost " + dead.name),
            Position = dead.transform.position,
            Size = record.Size,
            TeamId = record.TeamId,
        };

        dead.GetComponentsInChildren(false, _renderers);
        foreach (var renderer in _renderers)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (!renderer.enabled || !(renderer is MeshRenderer) || meshFilter == null
                || meshFilter.sharedMesh == null || renderer.GetComponentInParent<Canvas>() != null)
            {
                continue;
            }

            var part = new GameObject(renderer.name);
            part.transform.SetParent(ghost.Body.transform, false);
            part.transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
            part.transform.localScale = renderer.transform.lossyScale;
            part.AddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;

            // Copies: the unit's own materials may go with it.
            var materials = renderer.sharedMaterials;
            for (var m = 0; m < materials.Length; m++)
            {
                if (materials[m] != null)
                {
                    materials[m] = new Material(materials[m]);
                    ghost.Materials.Add(materials[m]);
                }
            }

            var copy = part.AddComponent<MeshRenderer>();
            copy.sharedMaterials = materials;
            copy.shadowCastingMode = renderer.shadowCastingMode;
            copy.receiveShadows = renderer.receiveShadows;
        }

        _ghosts.Add(ghost);
    }

    /// <summary>Every ghost goes, with the fog or at the end of the game.</summary>
    public void Clear()
    {
        foreach (var ghost in _ghosts)
        {
            DestroyGhost(ghost);
        }

        _ghosts.Clear();
    }

    private bool Decide(UnitRecord record, bool fogIsOn)
    {
        FogSight sight;

        if (!fogIsOn)
        {
            sight = FogSight.Visible;
        }
        else
        {
            var isFriendly = _friendlyTeams.Contains(record.TeamId);
            var isInSight = !isFriendly && IsInSight(record.Transform.position, record.Size);
            if (isInSight)
            {
                record.WasSeen = true;
                record.LastSeenPosition = record.Transform.position;
            }

            sight = FogRules.SightOf(isFriendly, record.IsStatic, isInSight, record.WasSeen);
        }

        if (sight == record.Sight)
        {
            // Something drawn may have been hung on a hidden unit since the
            // last pass (stun stars): hide it too.
            if (sight == FogSight.Hidden)
            {
                SetDrawn(record, false);
            }

            return false;
        }

        record.Sight = sight;
        SetDrawn(record, sight != FogSight.Hidden);

        if (record.Bars != null)
        {
            record.Bars.SetHiddenByFog(sight != FogSight.Visible);
        }

        return true;
    }

    /// <summary>Seen when its middle or any side of its footprint is in a visible cell.</summary>
    private bool IsInSight(Vector3 position, float size)
    {
        return _fog.IsVisibleAt(position)
            || _fog.IsVisibleAt(position + new Vector3(size, 0f, 0f))
            || _fog.IsVisibleAt(position - new Vector3(size, 0f, 0f))
            || _fog.IsVisibleAt(position + new Vector3(0f, 0f, size))
            || _fog.IsVisibleAt(position - new Vector3(0f, 0f, size));
    }

    private void SetDrawn(UnitRecord record, bool drawn)
    {
        record.GameObject.GetComponentsInChildren(true, _renderers);
        foreach (var renderer in _renderers)
        {
            if (renderer.forceRenderingOff == drawn)
            {
                renderer.forceRenderingOff = !drawn;
            }
        }
    }

    private void UpdateGhosts(bool fogIsOn)
    {
        for (var i = _ghosts.Count - 1; i >= 0; i--)
        {
            var ghost = _ghosts[i];
            if (fogIsOn && !IsInSight(ghost.Position, ghost.Size))
            {
                continue;
            }

            DestroyGhost(ghost);
            _ghosts.RemoveAt(i);
        }
    }

    private static void DestroyGhost(Ghost ghost)
    {
        if (ghost.Body != null)
        {
            Object.Destroy(ghost.Body);
        }

        foreach (var material in ghost.Materials)
        {
            Object.Destroy(material);
        }
    }

    /// <summary>
    /// The player ordered an attack and the target went into the fog: the unit
    /// walks to where it was seen last (Q-20). Only the player's own units —
    /// allies are not ordered by the player.
    /// </summary>
    private void FollowLostAttackTargets()
    {
        var own = UnitRegistry.OfTeam(_playerTeamId);
        for (var i = 0; i < own.Count; i++)
        {
            var commands = own[i].Commands;
            var target = commands != null ? commands.CurrentAttackTarget : null;
            if (target == null)
            {
                continue;
            }

            var targetRecord = UnitRegistry.Of(target);
            if (targetRecord != null && targetRecord.Sight != FogSight.Visible)
            {
                commands.ReplaceAttackWithMove(targetRecord.LastSeenPosition);
            }
        }
    }
}
