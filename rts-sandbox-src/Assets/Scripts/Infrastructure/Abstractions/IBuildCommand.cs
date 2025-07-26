using UnityEngine;

namespace Assets.Scripts.Infrastructure.Abstractions
{
    public interface IBuildCommand : ICommand
    {
        GameObject GetBuildingObject();

        Vector3 GetPoint();
    }
}
