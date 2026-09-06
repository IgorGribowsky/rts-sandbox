using UnityEngine;

namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    public abstract class AttackingBehaviourBase : UnitBehaviourBase
    {
        public override UnitActionType Trigger => UnitActionType.Attack;

        protected GameObject Target = null;
    }
}
