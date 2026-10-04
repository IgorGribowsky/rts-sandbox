using Assets.Scripts.Infrastructure.Constants;
using UnityEngine;

namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    /// <summary>
    /// Turning to the aim before a blow or a cast (T-033). Only the yaw: units
    /// stand upright whatever the height of the aim.
    /// </summary>
    public static class Facing
    {
        /// <summary>
        /// Turns this frame's share towards the point. True once the unit looks
        /// at it — within <see cref="GameConstants.FacingToleranceDegrees"/>, or
        /// the point is right under it and there is nowhere to look.
        /// </summary>
        public static bool TurnTowards(Transform unit, Vector3 point, float degreesPerSecond)
        {
            var direction = point - unit.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            var wanted = Quaternion.LookRotation(direction);
            unit.rotation = Quaternion.RotateTowards(unit.rotation, wanted, degreesPerSecond * Time.deltaTime);

            return Quaternion.Angle(unit.rotation, wanted) <= GameConstants.FacingToleranceDegrees;
        }
    }
}
