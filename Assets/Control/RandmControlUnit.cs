using System;
using UnityEngine;

namespace ControlSystem
{

    public class RandomControlUnit : ControlUnit
    {
        private static System.Random rand = new System.Random(0);

        public override double[] Calculate(double[] input, bool isSave)
        {
            var output = new double[3];
            output[0] = (rand.NextDouble() * 2.0 - 1.0) * 0.4;
            output[1] = (rand.NextDouble() * 2.0 - 1.0) * 0.3;
            output[2] = 0; // 不射击


            Control(output[0], output[1], output[2]);

            return output;
        }
    }

}