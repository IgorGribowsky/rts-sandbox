using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.Helpers
{
    public static class GameObjectExtensions
    {
        public static float GetDistanceTo(this GameObject object1, GameObject object2)
        {
            if (object1 != null && object2 != null)
            {
                // Получаем границы объектов
                Bounds bounds1 = object1.GetComponent<Renderer>().bounds;
                Bounds bounds2 = object2.GetComponent<Renderer>().bounds;

                // Вычисляем расстояние между центрами объектов
                float centerDistance = Vector3.Distance(bounds1.center, bounds2.center);

                // Вычисляем расстояние с учетом размеров объектов
                float distance = centerDistance - (bounds1.extents.magnitude + bounds2.extents.magnitude);

                return distance;
            }
            else
            {
                return float.MaxValue;
            }
        }

        public static GameObject GetNearestUnitInRadius(this GameObject gameObject, float radius, Func<GameObject, bool> filter = null)
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
            GameObject closestUnit = null;
            float closestDistanceSqr = radius * radius; // Используем квадрат расстояния для оптимизации

            foreach (GameObject unit in units)
            {
                // Если фильтр задан и объект не проходит проверку, пропускаем его
                if (filter != null && !filter(unit)) continue;

                float distanceSqr = (unit.transform.position - gameObject.transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestUnit = unit;
                    closestDistanceSqr = distanceSqr;
                }
            }

            return closestUnit;
        }
    }
}
