using TankGame;
using BPNetwork;
using UnityEngine;

namespace ControlSystem
{

    public class NetworkControlUnit : ControlUnit
    {

        private BPNet network;


        public void NetworkSetup(int inputs, int outputs)
        {
            int hiddens = (inputs + outputs) * 3 / 2;
            network = new BPNet();
            network.CreateNet(3, inputs, hiddens, outputs);
            network.SetActivationFunctions(new SigmoidFunction(), new SigmoidFunction());
            //Debug.Assert(network.CreateNet(3, inputs, hiddens, outputs) == 0);
            //Debug.Log(network.Layer.ToString());
            //Debug.Assert(network.SetActivationFunctions(new TanhFunction(), new TanhFunction()) == 0);
        }

        public override double[] Calculate(double[] input, bool isSave)
        {
            if (network==null || network.Layer == 0) return null;
            var output = network.Predict(input);
            output[0] = output[0] * 2.0 - 1.0;
            output[1] = output[1] * 2.0 - 1.0;
            output[2] = output[2] * 2.0 - 1.0;

            Control(output[0], output[1], output[2]);

            return output;
        }

        public override double Train(double[] input, double[] output)
        {
            if (network == null || network.Layer == 0) return 0;
            return network.Train(input, output);
        }
    }

}