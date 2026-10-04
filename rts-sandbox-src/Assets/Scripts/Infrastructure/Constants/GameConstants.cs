namespace Assets.Scripts.Infrastructure.Constants
{
    public class GameConstants
    {
        public const float FollowingDistance = 1.2f;
        public const float DamageReceivedAgressionTime = 3f;
        public const float DamageReceivedAgressionDistance = 30f;
        public const float DamageReceivedCallToAttackDistance = 12f;
        public const float PersecutionDistance = 50f;

        /// <summary>
        /// Who gives way in a fight (T-064), 0 the most important: a melee unit
        /// at its place around the target goes first, archers standing to shoot
        /// make way for it and never shove the melee line, a melee unit with no
        /// place left waits behind both. An ordinary walk is 90.
        /// </summary>
        public const int MeleeFightAvoidancePriority = 40;
        public const int RangeFireAvoidancePriority = 60;
        public const int MeleeNoPlaceAvoidancePriority = 70;

        /// <summary>
        /// How long an ordinary projectile may stay in the air. Without it a
        /// shot at a target that runs faster than the arrow never lands and
        /// never disappears (T-014).
        /// </summary>
        public const float ProjectileMaxLifetime = 8f;

        /// <summary>
        /// Auto attack looks for a target once in this many frames, not every
        /// frame (T-011). Units are spread across the frames by instance id, so
        /// they do not all search at once. A lost target is picked up at once,
        /// whatever the counter says.
        /// </summary>
        public const int TargetSearchFrameInterval = 5;

        public const float GridCellSize = 1f;
        public const float GridCellNarrowing = 0.1f;

        public const float BuildingHPStartPercent = 0.1f;

        public const float ExtraRadiusForMining = 0.6f;
        public const float MiningAcceptDistance = 0.7f;

        public const float ResourceFindDistance = 35f;
        public const float StorageFindDistance = 35f;
        public const float HarvestingDistance = 0.3f;

        public const float DoubleClickTime = 0.3f; // Максимальное время между кликами

        public const float DoubleClickSelectDistance = 20f;

        public const float ResourcesReturnedWhenBuildingCanceled = 0.7f;

        public const float HpRegenRate = 1f;
        public const float ManaRegenRate = 1f;

        public const float DefaultEffectTickRate = 0.5f;
    }
}
