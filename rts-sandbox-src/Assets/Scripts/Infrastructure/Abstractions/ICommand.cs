namespace Assets.Scripts.Infrastructure.Abstractions
{
    public interface ICommand
    {
        public bool Check();

        public void Start();
    }
}
