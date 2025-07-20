using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Infrastructure.Helpers
{
    public static class GameObjectExtensions
    {
        public static float GetDistanceTo(this GameObject object1, GameObject object2)
        {
            if (object1 != null && object2 != null)
            {
                // Получаем границы объектов
                var (extendsMagnitude1, adjustedCenter1) = object1.GetSizeAndCenter();
                var (extendsMagnitude2, adjustedCenter2) = object2.GetSizeAndCenter();

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

        public static float GetDistanceTo(this GameObject object1, Vector3 point)
        {
            if (object1 != null)
            {
                var (extendsMagnitude1, adjustedCenter1) = object1.GetSizeAndCenter();

                float centerDistance = Vector3.Distance(adjustedCenter1, point);

                float distance = centerDistance - extendsMagnitude1;

                return distance;
            }
            else
            {
                return float.MaxValue;
            }
        }

        public static (float, Vector3) GetSizeAndCenter(this GameObject gameObject)
        {
            var center = GetBoundCenter(gameObject);

            var size = GetSize(gameObject);

            return (size, center);
        }

        public static Vector3 GetBoundCenter(this GameObject gameObject)
        {
            Bounds bounds = gameObject.GetComponent<Renderer>().bounds;
            Vector3 adjustedCenter = bounds.center;
            adjustedCenter.y = 0;

            return adjustedCenter;
        }


        public static float GetSize(this GameObject gameObject)
        {
            var buildingValues = gameObject.GetComponent<BuildingValues>();
            var navMeshAgent = gameObject.GetComponent<NavMeshAgent>();

            if (buildingValues != null)
            {
                return buildingValues.ObstacleSize / 2f;
            }
            else if (navMeshAgent != null)
            {
                return NavMesh.GetSettingsByID(navMeshAgent.agentTypeID).agentRadius;
            }
            else
            {
                Bounds bounds = gameObject.GetComponent<Renderer>().bounds;
                Vector3 adjustedExtents = bounds.extents;
                adjustedExtents.y = 0;
                float extendsMagnitude = adjustedExtents.magnitude / Mathf.Sqrt(2);

                return extendsMagnitude;
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

        public static GameObject GetNearestResourceInRadius(this GameObject gameObject, float radius, Func<GameObject, bool> filter = null)
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag(Tag.HarvestedResource.ToString());
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

        public static bool CanBeAttacked(this GameObject unit, DamageType damageType)
        {
            var unitValues = unit.GetComponent<UnitValues>();

            return !unitValues.IsInvulnerable;
        }
    }
}
