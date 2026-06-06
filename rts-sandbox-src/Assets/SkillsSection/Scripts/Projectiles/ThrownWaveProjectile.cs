using System;
using UnityEngine;

public class ThrownWaveProjectile : ThrownProjectile
{
    public void StartThrow(Func<GameObject, GameObject, bool> canHitCheck, Action<GameObject, GameObject> hitCallback, GameObject projectileOwner, Vector3 direction, float range, float speed, float widthScale)
    {
        base.StartThrow(canHitCheck, hitCallback, projectileOwner, direction, range, speed);

        var scale = transform.localScale;
        scale.x = widthScale;
        transform.localScale = scale;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        var collidedGameObject = other.gameObject;
        if (!TriggeredUnits.Contains(collidedGameObject))
        {
            TriggeredUnits.Add(collidedGameObject);

            if (CanHitCheck(collidedGameObject, ProjectileOwner))
            {
                HitCallback(collidedGameObject, ProjectileOwner);
            }
        }
    }
}
