using System;
using UnityEngine;

namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    public abstract class UnitBehaviourBase : IUnitBehaviour
    {
        protected UnitBehaviourContext Context { get; private set; }

        public bool IsActive { get; set; }

        public abstract UnitActionType Trigger { get; }

        // Behaviours are plain classes now. These shortcuts keep their bodies
        // reading the same way they did while they were components.
        protected GameObject gameObject => Context.Owner;

        protected Transform transform => Context.Owner.transform;

        protected T GetComponent<T>() => Context.Owner.GetComponent<T>();

        protected static T Instantiate<T>(T original, Vector3 position, Quaternion rotation)
            where T : UnityEngine.Object
            => UnityEngine.Object.Instantiate(original, position, rotation);

        public void Initialize(UnitBehaviourContext context)
        {
            Context = context;
            OnInitialize();
        }

        public virtual bool CanHandle(EventArgs args) => true;

        public void Activate(EventArgs args)
        {
            IsActive = true;
            StartAction(args);
        }

        public void Deactivate()
        {
            IsActive = false;
            OnDeactivated();
        }

        public void Tick()
        {
            PreUpdate();

            if (IsActive)
            {
                UpdateAction();
            }

            PostUpdate();
        }

        public abstract void StartAction(EventArgs args);

        public virtual void Dispose() { }

        protected abstract void UpdateAction();

        /// <summary>Runs once, after every behaviour of the unit exists.</summary>
        protected virtual void OnInitialize() { }

        /// <summary>Runs when the manager switches away from this behaviour.</summary>
        protected virtual void OnDeactivated() { }

        protected virtual void PreUpdate()  { }

        protected virtual void PostUpdate() { }

        protected bool TriggerEndEventFlag = true;

        public void DisableTriggerEndEvent()
        {
            TriggerEndEventFlag = false;
        }

        public void EnableTriggerEndEvent()
        {
            TriggerEndEventFlag = true;
        }
    }
}
