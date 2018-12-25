using UnityEngine;

namespace ControlSystem
{

    public class ShootControlUnit : ControlUnit
    {
        // 射击精准度
        const float ShootEPS = (float)5e-2;
        public override double[] Calculate(double[] input, bool isSave)
        {
            var output = new double[3];
            output[0] = output[1] = output[2] = 0;

            //Debug.Log(check(input[1], 1) ? "射程内" : "射程外");
            //Debug.Log(check(input[input.Length-2], 1) ? "冷却" : "过热");
            //Debug.Log((Mathf.Abs((float)input[2]) < ShootEPS) ? "已瞄准" : "未瞄准" +input[2].ToString());
            // 瞄准 & 射程内 & 已冷却 => 开枪
            if (Mathf.Abs((float)input[2]) < ShootEPS && check(input[1], 1) && check(input[input.Length - 2], 1)) output[2] = 1;

            // 旋转
            if (input[2] > 0) output[1] = 1; else output[1] = -1;
            // 如果在调整最佳射击角度，则减慢速度
            if (Mathf.Abs((float)input[2]) <= ShootEPS * 10 && check(input[1], 1)) output[1] = input[2]*10;

            // 前进
            if (!check(input[1], 1)) output[0] = 1; else output[0] = -0.6;

            Control(output[0], output[1], output[2]);

            return output;
        }
    }

}