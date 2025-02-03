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

                Vector3 adjustedCenter1 = bounds1.center;
                Vector3 adjustedCenter2 = bounds2.center;
                Vector3 adjustedExtents1 = bounds1.extents;
                Vector3 adjustedExtents2 = bounds2.extents;
                adjustedCenter1.y = 0;
                adjustedCenter2.y = 0;
                adjustedExtents1.y = 0;
                adjustedExtents2.y = 0;

                float extendsMagnitude1 = adjustedExtents1.magnitude / Mathf.Sqrt(2);
                float extendsMagnitude2 = adjustedExtents2.magnitude / Mathf.Sqrt(2);

                // Вычисляем расстояние между центрами объектов
                float centerDistance = Vector3.Distance(adjustedCenter1, adjustedCenter2);

                // Вычисляем расстояние с учетом размеров объектов
                float distance = centerDistance - (extendsMagnitude1 + extendsMagnitude2);

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
            Bounds buildingBounds = new Bounds(buildingCenter, new Vector3(size, 0, size));

            // Проекция центра юнита на плоскость XZ
            Vector3 unitCenter = new Vector3(unitBounds.center.x, 0, unitBounds.center.z);

            // Направление от юнита к предполагаемому зданию
            Vector3 direction = (buildingCenter - unitCenter).normalized;

            // Определяем ближайшую точку на границе предполагаемого здания
            Vector3 buildingEdgePoint = buildingBounds.ClosestPoint(unitCenter);


            Vector3[] corners = new Vector3[4];
            corners[0] = buildingBounds.min; // Нижний левый угол (x, z)
            corners[1] = new Vector3(buildingBounds.min.x, 0f, buildingBounds.max.z); // Нижний правый угол (x, z)
            corners[2] = new Vector3(buildingBounds.max.x, 0f, buildingBounds.min.z); // Верхний левый угол (x, z)
            corners[3] = buildingBounds.max; // Верхний правый угол (x, z)

            // Проверка, находится ли точка на одном из углов
            bool isAtCorner = corners.Any(corner => Mathf.Abs(corner.x - buildingEdgePoint.x) < Mathf.Epsilon && Mathf.Abs(corner.z - buildingEdgePoint.z) < Mathf.Epsilon);
            if (!isAtCorner)
            {
                // Проверка, находится ли точка на верхней или нижней грани (по координатам x и z)
                bool isOnTopOrBottom = Mathf.Abs(buildingEdgePoint.z - buildingBounds.min.z) < Mathf.Epsilon || Mathf.Abs(buildingEdgePoint.z - buildingBounds.max.z) < Mathf.Epsilon;
                if (isOnTopOrBottom)
                {
                    direction.x = 0;
                }

                // Проверка, находится ли точка на левой или правой грани (по координатам x и z)
                bool isOnLeftOrRight = Mathf.Abs(buildingEdgePoint.x - buildingBounds.min.x) < Mathf.Epsilon || Mathf.Abs(buildingEdgePoint.x - buildingBounds.max.x) < Mathf.Epsilon;
                if (isOnLeftOrRight)
                {
                    direction.z = 0;
                }
            }

            // Сместить ближайшую точку на расстояние радиуса юнита
            float unitRadius = Mathf.Max(unitBounds.extents.x, unitBounds.extents.z);
            Vector3 adjustedPoint = buildingEdgePoint - direction * unitRadius;

            //Возвращаем точку, куда юнит должен подойти
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
