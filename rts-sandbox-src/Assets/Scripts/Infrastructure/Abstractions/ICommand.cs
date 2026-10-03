using Assets.Scripts.Infrastructure.Enums;

namespace Assets.Scripts.Infrastructure.Abstractions
{
    public interface ICommand
    {
        /// <summary>Which order this is, for whoever shows it (M-023).</summary>
        public UnitCommandKind Kind { get; }

        public bool Check();

        public void Start();
    }
}
