using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Vector3 GetClosestPointToInteract(this GameObject unit, Vector3 point, float size)
        {
            Bounds unitBounds = unit.GetComponent<Renderer>().bounds;

            // Центр здания задан точкой point
            Vector3 buildingCenter = new Vector3(point.x, 0, point.z);

            // Вычисляем границы будущего здания (квадратного)
            Bounds buildingBounds = new Bounds(buildingCenter, new Vector3(size / 2, 0, size / 2));

            // Проекция центра юнита на плоскость XZ
            Vector3 unitCenter = new Vector3(unitBounds.center.x, 0, unitBounds.center.z);

            // Направление от юнита к предполагаемому зданию
            Vector3 direction = (buildingCenter - unitCenter).normalized;

            // Определяем ближайшую точку на границе предполагаемого здания
            Vector3 buildingEdgePoint = buildingBounds.ClosestPoint(unitCenter);

            // Сместить ближайшую точку на расстояние радиуса юнита
            float unitRadius = Mathf.Max(unitBounds.extents.x, unitBounds.extents.z);
            Vector3 adjustedPoint = buildingEdgePoint - direction * unitRadius;

            // Возвращаем точку, куда юнит должен подойти
            return adjustedPoint;
        }

        public static GameObject GetNearestUnitInRadius(this GameObject gameObject, float radius, Func<GameObject, bool> filter = null)
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
            GameObject closestUnit = null;
            float closestDistance = radius;

            foreach (GameObject unit in units)
            {
                // Если фильтр задан и объект не проходит проверку, пропускаем его
                if (filter != null && !filter(unit)) continue;

                float distance = gameObject.GetDistanceTo(unit);
                if (distance < closestDistance)
                {
                    closestUnit = unit;
                    closestDistance = distance;
                }
            }

            return closestUnit;
        }


        public static IEnumerable<GameObject> GetAllUnitsInRadius(this GameObject gameObject, float radius, Func<GameObject, bool> filter = null)
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

            return units.Where(u => gameObject.GetDistanceTo(u) <= radius && filter(u));
        }
    }
}
