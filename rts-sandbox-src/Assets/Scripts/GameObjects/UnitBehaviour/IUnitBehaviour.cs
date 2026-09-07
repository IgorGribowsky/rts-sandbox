using System;

namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    public interface IUnitBehaviour
    {
        /// <summary>Order this behaviour answers to.</summary>
        UnitActionType Trigger { get; }

        bool IsActive { get; set; }

        void Initialize(UnitBehaviourContext context);

        /// <summary>
        /// Lets several behaviours share one trigger and pick by the arguments,
        /// the way casting to a point and casting into a target will.
        /// </summary>
        bool CanHandle(EventArgs args);

        void Activate(EventArgs args);

        void Deactivate();

        void Tick();

        void StartAction(EventArgs args);

        void Dispose();
    }
}
