using UnityEngine;

namespace Assets.Scripts.Infrastructure.Abstractions
{
    public interface IBuildCommand : ICommand
    {
        UnitTypeData GetBuildingType();

        Vector3 GetPoint();
    }
}
