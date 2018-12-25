using System;

namespace BPNetwork
{
    public static class IMath
    {
        private static readonly Random RANDOM = new Random();

        public static double GaussianRandomDistributed
        {
            get
            {
                var u1 = RANDOM.NextDouble();
                var u2 = RANDOM.NextDouble();

                var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                      Math.Sin(2.0 * Math.PI * u2);
                return randStdNormal / 12.5663706144 + 0.5;
            }
        }

        public static float GaussianRandom(float min, float max)
        {
            return min + (max - min) * (float)GaussianRandomDistributed;
        }
    }
}
