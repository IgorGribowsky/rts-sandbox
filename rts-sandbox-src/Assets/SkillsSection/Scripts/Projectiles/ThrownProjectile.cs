using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrownProjectile : MonoBehaviour
{
    protected float Speed;
    protected float Range;
    protected GameObject ProjectileOwner;
    protected Vector3 Direction;
    protected Func<GameObject, GameObject, bool> CanHitCheck;
    protected Action<GameObject, GameObject> HitCallback;
    protected List<GameObject> TriggeredUnits;

    protected Vector3 _startPoint;

    private void Awake()
    {
        TriggeredUnits = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _startPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var distance = transform.position - _startPoint;
        if (distance.magnitude > Range)
        {
            Destroy(gameObject);
        }

        var moveVector = Direction.normalized * Speed * Time.deltaTime;
        gameObject.transform.position += moveVector;
    }

    public void StartThrow(Func<GameObject, GameObject, bool> canHitCheck, Action<GameObject, GameObject> hitCallback, GameObject projectileOwner, Vector3 direction, float range, float speed)
    {
        Speed = speed;
        Range = range;
        ProjectileOwner = projectileOwner;
        Direction = direction;
        CanHitCheck = canHitCheck;
        HitCallback = hitCallback;

        transform.rotation = Quaternion.LookRotation(Direction);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        var collidedGameObject = other.gameObject;
        if (!TriggeredUnits.Contains(collidedGameObject))
        {
            TriggeredUnits.Add(collidedGameObject);

            if (CanHitCheck(collidedGameObject, ProjectileOwner))
            {
                HitCallback(collidedGameObject, ProjectileOwner);
                Destroy(gameObject);
            }
        }
    }
}
