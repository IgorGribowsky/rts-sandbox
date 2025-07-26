namespace Assets.Scripts.Infrastructure.Constants
{
    public class GameConstants
    {
        public const float FollowingDistance = 1.2f;
        public const float DamageReceivedAgressionTime = 3f;
        public const float DamageReceivedAgressionDistance = 30f;
        public const float DamageReceivedCallToAttackDistance = 12f;
        public const float PersecutionDistance = 50f;

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

    }
}
