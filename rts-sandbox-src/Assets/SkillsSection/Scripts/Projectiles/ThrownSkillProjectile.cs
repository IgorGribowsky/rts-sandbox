using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrownSkillProjectile : MonoBehaviour
{
    private float _speed;
    private float _range;
    private Vector3 _direction;
    private Predicate<GameObject> _canHitCheck;
    private Action<GameObject> _hitCallback;
    private List<GameObject> _triggeredUnits;

    private Vector3 _startPoint;

    private void Awake()
    {
        _triggeredUnits = new List<GameObject>();
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
        if (distance.magnitude > _range)
        {
            Destroy(gameObject);
        }

        var moveVector = _direction.normalized * _speed * Time.deltaTime;
        gameObject.transform.position += moveVector;
    }

    public void StartThrow(Predicate<GameObject> canHitCheck, Action<GameObject> hitCallback, Vector3 direction, float range, float speed)
    {
        _speed = speed;
        _range = range;
        _direction = direction;
        _canHitCheck = canHitCheck;
        _hitCallback = hitCallback;
    }

    private void OnTriggerEnter(Collider other)
    {
        var collidedGameObject = other.gameObject;
        if (!_triggeredUnits.Contains(collidedGameObject))
        {
            _triggeredUnits.Add(collidedGameObject);

            if (_canHitCheck(collidedGameObject))
            {
                _hitCallback(collidedGameObject);
                Destroy(gameObject);
            }
        }
    }
}
