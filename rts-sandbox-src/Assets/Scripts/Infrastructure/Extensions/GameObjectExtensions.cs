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
    }
}
