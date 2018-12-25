using UnityEngine;

namespace ControlSystem
{

    public class EscapeControlUnit : ControlUnit
    {

        const float ShootEPS = (float)2e-1;
        public override double[] Calculate(double[] input, bool isSave)
        {
            var output = new double[3];
            output[0] = output[1] = output[2] = 0;

            bool flag = false;

            //if (check(input[input.Length - 3], 1)) Debug.Log("Ahh");
            // 被击中，反方向倒退
            if (check(input[input.Length - 3], 1))
            {
                output[1] = -input[ 3] / Mathf.Abs((float)input[3]) * (1 - Mathf.Abs((float)input[3]));
                flag = true;
            }

            //Debug.Log(input.Length);
            // 被在射程内 冷却 瞄准
            for (var i = 0; i+5 <= input.Length && (!flag); i += 5)
            {
                //Debug.LogFormat("{0} {1} {2}", input[i + 1], input[i + 3], input[i + 4]);
                if (check(input[i + 1], 1) && check(input[i + 4], 1) && Mathf.Abs((float)input[i + 3]) < ShootEPS)
                {
                    output[1] = -input[i + 3]/Mathf.Abs((float)input[i+3])*(1- Mathf.Abs((float)input[i+3]));
                    flag = true;
                }
            }

            // 逃跑！
            if (flag) output[0] = -1;

            Control(output[0], output[1], output[2]);

            return output;
        }
    }

}