using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrownProjectile : MonoBehaviour
{
    protected float Speed;
    protected float Range;
    protected Vector3 Direction;
    protected Predicate<GameObject> CanHitCheck;
    protected Action<GameObject> HitCallback;
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

    public void StartThrow(Predicate<GameObject> canHitCheck, Action<GameObject> hitCallback, Vector3 direction, float range, float speed)
    {
        Speed = speed;
        Range = range;
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

            if (CanHitCheck(collidedGameObject))
            {
                HitCallback(collidedGameObject);
                Destroy(gameObject);
            }
        }
    }
}
