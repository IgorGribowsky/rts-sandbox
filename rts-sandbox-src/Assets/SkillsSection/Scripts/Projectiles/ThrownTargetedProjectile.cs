using System;
using UnityEngine;

/// <summary>
/// A skill projectile that follows one unit until it reaches it, the way the
/// ranged attack's bullet does (ProjectileBehavior). Not the same thing as
/// ThrownProjectile, which flies along a direction for ProjectileRange and hits
/// whoever it runs into.
///
/// The target dies mid-flight: the projectile finishes its way to the last place
/// it saw the target and disappears without hitting anybody. Same as the bullet.
/// </summary>
public class ThrownTargetedProjectile : MonoBehaviour
{
    private float _speed;

    private GameObject _owner;

    private GameObject _target;

    private Action<GameObject, GameObject> _hitCallback;

    private Vector3 _targetPosition;

    private bool _isThrown;

    public void StartThrow(Action<GameObject, GameObject> hitCallback, GameObject owner, GameObject target, float speed)
    {
        _hitCallback = hitCallback;
        _owner = owner;
        _target = target;
        _speed = speed;
        _targetPosition = target != null ? target.transform.position : transform.position;
        _isThrown = true;
    }

    private void Update()
    {
        if (!_isThrown || _speed <= 0)
        {
            return;
        }

        // While the target is alive the aim keeps updating, so the projectile
        // follows it instead of flying where it used to be.
        if (_target != null)
        {
            _targetPosition = _target.transform.position;
        }

        var step = _targetPosition - transform.position;
        var moveVector = step.normalized * _speed * Time.deltaTime;

        transform.position += moveVector;

        if (moveVector != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveVector);
        }

        if (step.magnitude < moveVector.magnitude)
        {
            if (_target != null)
            {
                _hitCallback?.Invoke(_target, _owner);
            }

            Destroy(gameObject);
        }
    }
}
