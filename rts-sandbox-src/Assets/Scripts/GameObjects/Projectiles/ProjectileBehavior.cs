using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

namespace Assets.Scripts.GameObjects.Projectiles
{
    public class ProjectileBehavior : MonoBehaviour
    {
        public float Speed { get; set; } = 0;

        public float Damage { get; set; }

        public DamageType DamageType { get; set; }

        public GameObject Target { get; set; }

        public GameObject Owner { get; set; }

        private UnitEventManager _targetEventManager;

        private Vector3 _targetPosition; 

        public void SetProperties(GameObject target, GameObject owner, float speed, float damage, DamageType damageType)
        {
            Damage = damage;
            DamageType = damageType;
            Target = target;
            Owner = owner;
            Speed = speed;
            _targetEventManager = target.GetComponent<UnitEventManager>();
        }

        public void Update()
        {
            if (Speed > 0) 
            {
                if (Target != null)
                {
                    _targetPosition = Target.transform.position;
                }

                var moveVector = Vector3.Normalize(_targetPosition - transform.position) * Speed * Time.deltaTime;

                transform.position += moveVector;
                transform.LookAt(moveVector);

                if (Vector3.Magnitude(_targetPosition - transform.position) < moveVector.magnitude)
                {
                    if (Target != null)
                    {
                        _targetEventManager.OnDamageReceived(Owner, Damage, DamageType);
                    }

                    Destroy(gameObject);
                }
            }
        }
    }
}
