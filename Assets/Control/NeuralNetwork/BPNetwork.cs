using System;
using System.IO;

using ElemType = System.Double;
using Status = System.Int32;

namespace BPNetwork
{
    class BPNet
    {
        // 神经网络基本参数
        protected int layerNum; // 网络层数
        protected int[] neuronNum; // 每层网络节点数量
        protected IActivationFunction[] activateFunction; // 每层激活函数
        private double learningRate; // 学习速率
        private double learningMomentum; // 学习动量

        // 神经网络内部权重
        protected ElemType[,,] weight; // 层与层之间权重 
        protected ElemType[,,] deltaWeight; // 权重误差
        protected ElemType[,] neuron; // 神经网络节点值
        protected ElemType[,] delta; // 节点误差
        protected ElemType[] testNumber;
        protected ElemType[,] input;
        protected ElemType[,] output;
        protected int sampleNum;

        public int Layer
        {
            get { return layerNum; }
        }
        
        // 学习速率设置
        public double LearningRate
        {
            get{ return learningRate;}
            set
            {
                if (learningRate <= 1.0 || learningRate > 0)
                {
                    learningRate = value;
                }
                else Console.WriteLine("非法学习速率");
            }
        }

        // 学习动量设置
        public double LearningMomentum
        {
            get{return learningMomentum; }
            set
            {
                if (learningRate <= 1.0 || learningRate > 0)
                {
                    learningMomentum = value;
                }
                else Console.WriteLine("非法学习动量");
            }
        }


        public BPNet()
        {
            layerNum = 0;
            learningRate = 0.7;
            learningMomentum = 0.9;
            sampleNum = 0;
            neuronNum = null;
            weight = null;
            deltaWeight = null;
            delta = null;
            testNumber = null;
            neuron = null;
            activateFunction = null;
            input = null;
            output = null;
        }

        /**
         * 新建神经网络参数
         * @param num[0] : 神经网络层数
         *        num[1~num[0]] : 每层神经网络节点数量
         * @return Status : 是否成功建立神经网络 {0:成功, -1: 非法网络层数, -2:非法节点数量}
         */
        public Status CreateNet(params int[] nums)
        {
            // 检查神经网络层数是否正确
            if (nums[0] <= 2) return -1;
            
            // 新建神经网络节点
            layerNum = nums[0];
            neuronNum = new int[layerNum];
            int maxneuron = 0;
            for (int i = 0; i < layerNum; i++)
            {
                // 检查神经网络节点数量是否正确
                if (nums[i + 1] >= 1)
                {
                    maxneuron = Math.Max(maxneuron, nums[i + 1]);
                    neuronNum[i] = nums[i + 1] + 1;
                }
                else return -2;
            }
            maxneuron++;

            activateFunction = new IActivationFunction[layerNum];
            // 初始化激活函数(默认为Tanh)
            for (int i=1; i<layerNum; i++) activateFunction[i] = new SigmoidFunction();

            // 初始化神经网络权重与修正权重
            weight = new double[layerNum, maxneuron, maxneuron];
            deltaWeight = new double[layerNum, maxneuron, maxneuron];
        
            // 随机神经网络初始状态
            for (int i = 0; i < layerNum - 1; i++)
            {
                for (int j = 0; j < neuronNum[i + 1]; j++)
                {
                    for (int k = 0; k < neuronNum[i]; k++)
                    {
                        do
                        {
                            //weight[i, j, k] = ra.NextDouble() * 2 - 1.0;
                            weight[i, j, k] = IMath.GaussianRandom(0.01f, 0.1f);
                        } while (Math.Abs(weight[i, j, k]) >1e-1 || Math.Abs(weight[i, j, k]) < 1e-2);
                        
                        deltaWeight[i, j, k] = 0.0;
                    }
                }
            }

            // 新建节点与修正
            maxneuron += 5;//多几个神经元防止边界问题
            neuron = new ElemType[layerNum, maxneuron];
            delta = new ElemType[layerNum, maxneuron];

            return 0;
        }

        /**
         * 设置激活函数
         * @param funcs[] : 每一层的激活函数
         * @return : 激活函数设置状态{ 0:成功 , -1:非法激活函数}
         */
        public Status SetActivationFunctions(params IActivationFunction[] funcs)
        {
            for (int i = 1; i < layerNum; i++)
            {
                if (funcs[i - 1] == null) return -3;
                activateFunction[i] = funcs[i - 1];
            }
            return 0;
        }

        /**
         * 训练
         */
        public double Train(ElemType[] inputs, ElemType[] outputs)
        {
            //输入层
            for (var i=0; i< neuronNum[0] - 1; i++) neuron[0, i] = inputs[i];
            neuron[0, neuronNum[0] - 1] = 1;

            // 正向激活
            for (var i=1; i<layerNum; i++)
            {
                for (var j=0; j<neuronNum[i]-1; j++)
                {
                    neuron[i, j] = 0;
                    for (var k=0; k < neuronNum[i - 1]; k++) neuron[i, j] += neuron[i - 1, k] * weight[i - 1, j, k];
                    neuron[i, j] /= neuronNum[i - 1];
                    neuron[i, j] = activateFunction[i].Compute(neuron[i, j]);
                }
                neuron[i, neuronNum[i] - 1] = 1;
            }

            // 反向传播
            for (int i = layerNum-1, j = 0; j < neuronNum[i] - 1; j++) delta[i, j] = neuron[i, j] * (1.0 - neuron[i, j]) * (neuron[i, j] - outputs[j]);
            var avoidThreshold = 1;
            for (var i = layerNum - 2; i > 0; i--)
            {
                for (int j = 0, k = 0; j < neuronNum[i]; j++)
                {
                    delta[i, j] = 0;
                    for (k = 0; k < neuronNum[i + 1] - avoidThreshold; k++) delta[i, j] += (weight[i, k, j] * delta[i + 1, k]);
                    delta[i, j] *= (neuron[i, k] * (1.0 - neuron[i, k]));
                }
                avoidThreshold = 0;
            }

            // 更新权重
            for (int i = 0; i < layerNum - 1; i++)
            {
                if (i == layerNum - 2)  avoidThreshold = 1;
                for (int j = 0; j < neuronNum[i + 1] - avoidThreshold; j++)
                {
                    for (int k = 0; k < neuronNum[i]; k++)
                    {
                        weight[i, j, k] += learningMomentum * deltaWeight[i, j, k];
                        weight[i, j, k] -= learningRate * delta[i + 1, j] * neuron[i, k];
                        deltaWeight[i, j, k] = learningMomentum * deltaWeight[i, j, k] - learningRate * delta[i + 1, j] * neuron[i, k];
                    }
                }
            }

            // 计算误差
            // err = \frac{1}{2} \cdot \sum_{i=1}^{M}(output[i]-target[i])^2
            ElemType error = 0;
            for (var i = 0; i < neuronNum[layerNum - 1] - 1; i++) error += Math.Pow((neuron[layerNum - 1, i] - outputs[i]), 2.0);
            error /= 2.0;

            return error;
        }

        public double[] Predict(ElemType[] inputs)
        {
            //输入层
            for (var i = 0; i < neuronNum[0] - 1; i++) neuron[0, i] = inputs[i];
            neuron[0, neuronNum[0] - 1] = 1;

            // 正向激活
            for (var i = 1; i < layerNum; i++)
            {
                for (var j = 0; j < neuronNum[i] - 1; j++)
                {
                    neuron[i, j] = 0;
                    for (var k = 0; k < neuronNum[i - 1]; k++) neuron[i, j] += neuron[i - 1, k] * weight[i - 1, j, k];
                    neuron[i, j] /= neuronNum[i - 1];
                    neuron[i, j] = activateFunction[i].Compute(neuron[i, j]);
                }
                neuron[i, neuronNum[i] - 1] = 1;
            }

            //输出
            var output = new ElemType[neuronNum[layerNum - 1]];
            for (int j = 0; j < neuronNum[layerNum - 1]; j++) output[j] = neuron[layerNum - 1, j];

            return output;
        }

    }
}
