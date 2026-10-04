using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Helpers;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public enum GatherTargetKind
    {
        /// <summary>Carries something: take it to a storage.</summary>
        Deliver,

        /// <summary>A tree or another resource picked up by hand.</summary>
        Harvest,

        /// <summary>One of our own held mines with a free place.</summary>
        Mine,
    }

    public struct GatherTarget
    {
        public GatherTargetKind Kind;
        public GameObject Target;
    }

    /// <summary>
    /// Where a worker goes on the "gather" order (M-023, M-014): with a load —
    /// to the nearest storage that takes it; empty-handed — to the nearest
    /// source it can work, a tree or an own held mine with a free place, both
    /// counted alike. Searched in the same radius the worker already uses on
    /// its own (35).
    ///
    /// Asked twice: by the HUD, to show the button as usable or not, and by the
    /// order at the moment it starts, so a queued order goes to whatever is
    /// nearest then, not when it was given.
    /// </summary>
    public static class GatherTargets
    {
        public static bool CanGather(GameObject unit)
        {
            var values = unit != null ? unit.GetComponent<HarvestingValues>() : null;
            return values != null && (values.IsHarvestor || values.IsMiner);
        }

        public static bool TryFind(GameObject unit, out GatherTarget target)
        {
            target = default;

            if (!CanGather(unit))
            {
                return false;
            }

            var values = unit.GetComponent<HarvestingValues>();
            var team = unit.GetComponent<TeamMember>();
            var teamId = team != null ? team.TeamId : 0;

            var harvesting = unit.GetComponent<UnitBehaviourManager>()?.Get<HarvestingBehaviour>();
            if (harvesting != null && harvesting.CurrentResourceValues > 0 && harvesting.CurrentResource != null)
            {
                var resource = harvesting.CurrentResource;
                var storage = unit.GetNearestUnitInRadius(GameConstants.StorageFindDistance, candidate =>
                {
                    var candidateTeam = candidate.GetComponent<TeamMember>();
                    var candidateStorage = candidate.GetComponent<HarvestedResourcesStorage>();
                    return candidateTeam != null && candidateTeam.TeamId == teamId
                        && candidateStorage != null && candidateStorage.isActiveAndEnabled
                        && candidateStorage.StoredResources.Contains(resource);
                });

                // A worker with a load does not wander off to work more: nowhere
                // to take it means the order has nothing to do (M-023).
                if (storage == null)
                {
                    return false;
                }

                target = new GatherTarget { Kind = GatherTargetKind.Deliver, Target = storage };
                return true;
            }

            GameObject tree = null;
            if (values.IsHarvestor)
            {
                tree = unit.GetNearestResourceInRadius(GameConstants.ResourceFindDistance, candidate =>
                {
                    var resourceValues = candidate.GetComponent<ResourceValues>();
                    return resourceValues != null && resourceValues.ResourcesAmount > 0
                        && values.HarvestableResources.Contains(resourceValues.Resource);
                });
            }

            GameObject mine = null;
            if (values.IsMiner)
            {
                mine = unit.GetNearestUnitInRadius(GameConstants.ResourceFindDistance, candidate =>
                {
                    var candidateTeam = candidate.GetComponent<TeamMember>();
                    var heldMine = candidate.GetComponent<HeldMine>();
                    return candidateTeam != null && candidateTeam.TeamId == teamId
                        && heldMine != null && heldMine.isActiveAndEnabled
                        && heldMine.CheckIfCanAddMiner();
                });
            }

            if (tree == null && mine == null)
            {
                return false;
            }

            var mineIsCloser = mine != null
                && (tree == null || unit.GetDistanceTo(mine) <= unit.GetDistanceTo(tree));

            target = mineIsCloser
                ? new GatherTarget { Kind = GatherTargetKind.Mine, Target = mine }
                : new GatherTarget { Kind = GatherTargetKind.Harvest, Target = tree };
            return true;
        }
    }
}
