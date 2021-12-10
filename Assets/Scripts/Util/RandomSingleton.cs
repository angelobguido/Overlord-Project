using System;

namespace Util
{
    public class RandomSingleton
    {
        private const int RandomSeed = 42;
        private RandomSingleton()
        {
            #if UNITY_EDITOR
                Random = new Random(RandomSeed);
            #else
                Random = new Random();
            #endif
        }

        private static RandomSingleton _instance;

        public Random Random { get; set; }

        public static RandomSingleton GetInstance()
        {
            return _instance ??= new RandomSingleton();
        }
    }
}