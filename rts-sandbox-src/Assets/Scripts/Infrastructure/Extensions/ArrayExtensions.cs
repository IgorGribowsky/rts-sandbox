namespace Assets.Scripts.Infrastructure.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] IncreaseArray<T>(this T[] originalArray, int increaseBy)
        {
            if (increaseBy <= 0)
                return originalArray;

            T[] newArray = new T[originalArray.Length + increaseBy];
            for (int i = 0; i < originalArray.Length; i++)
            {
                newArray[i] = originalArray[i];
            }

            return newArray;
        }
    }
}
