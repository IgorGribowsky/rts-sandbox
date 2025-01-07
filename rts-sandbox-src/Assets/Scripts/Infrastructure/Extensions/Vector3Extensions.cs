using UnityEngine;

namespace Assets.Scripts.Infrastructure.Extensions
{
    public static  class Vector3Extensions
    {
        public static bool IsEqual(this Vector3 vectorA, Vector3 vectorB, float tolerance = 1e-5f)
        {
            return Mathf.Abs(vectorA.x - vectorB.x) < tolerance &&
                   Mathf.Abs(vectorA.y - vectorB.y) < tolerance &&
                   Mathf.Abs(vectorA.z - vectorB.z) < tolerance;
        }
    }
}
