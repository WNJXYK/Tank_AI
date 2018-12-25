using UnityEngine;

namespace ControlSystem
{

    public class UserControlUnit : ControlUnit {

        public override double[] Calculate(double[] input, bool isSave)
        {
            var output = new double[3];
            output[0] = output[1] = 0;
            if (Input.GetKey(KeyCode.UpArrow)) output[0] = 1;
            if (Input.GetKey(KeyCode.DownArrow)) output[0] = -1;
            if (Input.GetKey(KeyCode.LeftArrow)) output[1] = -1;
            if (Input.GetKey(KeyCode.RightArrow)) output[1] = 1;
            if (Input.GetKey(KeyCode.Space)) output[2] = 1; else output[2] = 0;

            Control(output[0], output[1], output[2]);

            return output;
        }
    }

}