using OpenCover.Framework.Model;
using System;
using UnityEngine;

namespace Assets.Scripts.GameObjects.UnitBahaviour
{
    public abstract class UnitBehaviourBase : MonoBehaviour
    {
        public bool IsActive { get; set; }

        public void Update()
        {
            PreUpdate();

            if (IsActive)
            {
                UpdateAction();
            }

            PostUpdate();
        }

        public abstract void StartAction(EventArgs args);

        protected abstract void UpdateAction();


        protected virtual void PreUpdate()  { }

        protected virtual void PostUpdate() { }
    }
}
