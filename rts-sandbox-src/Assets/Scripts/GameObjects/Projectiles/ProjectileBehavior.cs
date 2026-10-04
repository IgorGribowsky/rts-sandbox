using Assets.Scripts.Infrastructure.Constants;
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

        /// <summary>A crit of the shooter (T-070): its numbers float up when this lands.</summary>
        public bool IsCritical { get; set; }

        private UnitEventManager _targetEventManager;

        private Vector3 _targetPosition;

        /// <summary>
        /// Seconds this one has been in the air. A projectile chasing a target
        /// it can never catch used to fly for the rest of the game.
        /// </summary>
        private float _timeInFlight;

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
                _timeInFlight += Time.deltaTime;
                if (_timeInFlight > GameConstants.ProjectileMaxLifetime)
                {
                    Destroy(gameObject);
                    return;
                }

                if (Target != null)
                {
                    _targetPosition = Target.transform.position;
                }

                var moveVector = Vector3.Normalize(_targetPosition - transform.position) * Speed * Time.deltaTime;

                transform.position += moveVector;

                // LookAt wants a point in the world, and moveVector is the step
                // for this frame — the projectile used to turn its nose towards
                // a spot near the origin instead of along its flight (T-014).
                if (moveVector.sqrMagnitude > 0f)
                {
                    transform.rotation = Quaternion.LookRotation(moveVector);
                }

                if (Vector3.Magnitude(_targetPosition - transform.position) < moveVector.magnitude)
                {
                    if (Target != null)
                    {
                        _targetEventManager.OnDamageReceived(Owner, Damage, DamageType);

                        // The shooter may already be dead by the time the arrow
                        // lands, then there is nobody to tell.
                        if (Owner != null)
                        {
                            Owner.GetComponent<UnitEventManager>()?.OnDamageDealt(Target, Damage, DamageType);

                            if (IsCritical)
                            {
                                GameServices.PlayerEventController?.OnCriticalHit(Owner, Damage);
                            }
                        }
                    }

                    Destroy(gameObject);
                }
            }
        }
    }
}
