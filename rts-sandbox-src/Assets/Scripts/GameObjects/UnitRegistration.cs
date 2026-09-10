using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

/// <summary>
/// Puts the unit into <see cref="UnitRegistry"/> while it is alive and takes it
/// out again when it dies or is switched off — the same OnEnable/OnDisable rule
/// every other subscription follows (T-028).
///
/// Only objects tagged Unit go in: that is exactly the set the target search
/// used to fish out with FindGameObjectsWithTag.
/// </summary>
public class UnitRegistration : MonoBehaviour
{
    private void OnEnable()
    {
        if (!CompareTag(Tag.Unit.ToString()))
        {
            return;
        }

        UnitRegistry.Register(gameObject);
    }

    private void OnDisable()
    {
        UnitRegistry.Unregister(gameObject);
    }
}
