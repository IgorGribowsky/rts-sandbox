using UnityEngine;

namespace Assets.Scripts.GameObjects.UnitBehaviour
{
    /// <summary>
    /// Everything a behaviour needs from the unit it belongs to. Behaviours are
    /// plain classes now, so this replaces what MonoBehaviour used to give them.
    /// </summary>
    public class UnitBehaviourContext
    {
        public UnitBehaviourContext(GameObject owner, UnitBehaviourManager manager)
        {
            Owner = owner;
            Manager = manager;
        }

        public GameObject Owner { get; }

        public UnitBehaviourManager Manager { get; }
    }
}
