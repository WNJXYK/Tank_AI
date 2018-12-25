using System;
using UnityEngine;
using ControlSystem;

namespace TankGame
{
	
	public class TankDriver : MonoBehaviour
    {
        // 坦克本体
		public Tank target;
        public String tankName;
        
        // 坦克获得信息参数
		public float viewRange;
        public int considerTargets = 5;
        public int inputNum;
        public double shootRange;
        public double lastLoss = 0;

        // 坦克命中率奖励分
        public float hitBounsPoint;

        // 坦克控制模式
        private int controlMode;

        public ControlUnit control;
		
        /**
         * 创建神经网络
         */
		private void Awake()
        {
			target = GetComponent<Tank>();
            inputNum = considerTargets * 5 + 3;

        }

        /**
         * 坦克初始化
         */
         public void Setup(int idx, Color col, String name)
        {
            // 设置控制模式
            controlMode = idx;
            switch (idx)
            {
                case 0: // 0 号手动机
                    control = new UserControlUnit();
                    break; 
                case 1: // 1 号自瞄机
                    control = new ShootControlUnit();
                    break;
                case 2: // 2 号逃跑机
                    control = new EscapeControlUnit();
                    break;
                case 3: // 3 号学习机
                    control = new NetworkControlUnit();
                    ((NetworkControlUnit)control).NetworkSetup(inputNum, 3);
                    break;
                default: // 其他靶子
                    control = new RandomControlUnit();
                    break;
            }
            //设置控制目标
            target.SetTankColors(col);
            control.Setup(target);
            tankName = name;
        }

        /**
         * 训练坦克逻辑
         */
        public double Train(double[] inputs, double[] outputs)
        {
            return lastLoss=control.Train(inputs, outputs);
        }

        // 更新坦克逻辑
        public double[] Active(double[] inputs)
        {
            //var inputs = CalculateNetworkInputs();
            return control.Calculate(inputs, controlMode == 0);
        }

        /**
         * 生成神经网络的输入数据
         * 敌人的信息 : 距离、自己对它角度、它对自己角度、敌方分数
         * 我方信息： 武器状态、 前进加速度、 转向加速度、 当前分数、 当前生命值
         * return double[] : 表示输入数组
         */
        public double[] CalculateNetworkInputs()
        {
            var inputs = new double[inputNum];
            int idx = 0;
            for (int i=1; i<=considerTargets; i++)
            {
                // 找到离自己第i近的敌人
                var enemy = target.ClosestKthEnemy(viewRange, i);
                // 计算距离
                inputs[idx++]= enemy != null ? Vector3.Distance(transform.position, enemy.position) / viewRange : 1d;
                // 是否在射程内
                inputs[idx++] = enemy != null ? (Vector3.Distance(transform.position, enemy.position) <= shootRange)?1d : 0d : 0d;
                // 计算自己到敌人的角度
                inputs[idx++] = enemy != null ? Vector3.Dot(transform.right, (enemy.position - transform.position).normalized) : 1d;
                // 计算敌人到自己的角度
                inputs[idx++] = enemy != null ? Vector3.Dot(enemy.right, (transform.position - enemy.position).normalized) : 1d;
                // 敌方武器是否冷却
                inputs[idx++] = enemy!=null ? (enemy.GetComponent<Tank>().weaponReady ? 1d : 0d) : 0d;
            }
            // 自己是否刚刚受到伤害
            inputs[idx++] = target.GetComponent<Unit>().isHurt ? 1d : 0d;
            // 自己的武器是否冷却完毕
            inputs[idx++] = target.weaponReady ? 1d : 0d;
            // 当前的生命值
            inputs[idx++] = target.health;
            return inputs;
        }

        /**
         * 范围辅助判定
         */
        private void OnDrawGizmosSelected() {
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, viewRange);
		}
	}

}

