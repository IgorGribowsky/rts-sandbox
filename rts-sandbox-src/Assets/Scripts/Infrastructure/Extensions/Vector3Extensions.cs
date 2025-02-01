using UnityEngine;
using Assets.Scripts.Infrastructure.Constants;

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

        public static Vector3 GetGridPoint(this Vector3 point, int buildingSize)
        {
            var resultPoint = point / GameConstants.GridCellSize;

            if (buildingSize % 2 == 0)
            {
                resultPoint = new Vector3(Mathf.Round(resultPoint.x), 0, Mathf.Round(resultPoint.z));
            }
            else
            {
                resultPoint = new Vector3(Mathf.Floor(resultPoint.x) + 0.5f, 0, Mathf.Floor(resultPoint.z) + 0.5f);
            }

            resultPoint = resultPoint * GameConstants.GridCellSize;
            return resultPoint;
        }
    }
}
