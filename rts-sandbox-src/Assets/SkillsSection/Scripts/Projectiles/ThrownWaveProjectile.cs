using System;
using UnityEngine;

public class ThrownWaveProjectile : ThrownProjectile
{
    public void StartThrow(Predicate<GameObject> canHitCheck, Action<GameObject> hitCallback, Vector3 direction, float range, float speed, float widthScale)
    {
        base.StartThrow(canHitCheck, hitCallback, direction, range, speed);

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

            if (CanHitCheck(collidedGameObject))
            {
                HitCallback(collidedGameObject);
            }
        }
    }
}
