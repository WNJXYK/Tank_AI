using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace TankGame
{
	
	public class WorldController : MonoBehaviour
    {

        // 训练每回合训练轮数
        public int totalStepsPerEpoch = 1000;
		// 坦克数量
		public int tankCount;
        
        // 刷新坦克
		public Transform generatePoint;
		public float generateRadius;
		public GameObject tankPrefab;

        // 当前世界信息
		public int currentStepsInEpoch { get; private set; }
		public int epoch { get; private set; }
        public Text screenResult;

        // 坦克实体列表
		private List<Vector3> m_initPosition = new List<Vector3>();
		private List<TankDriver> m_drivers = new List<TankDriver>();
        private List<double[]> inputList = new List<double[]>(), outputList = new List<double[]>();

        /**
         * 随机出生点
         * @param n : 坦克数量
         */
        private void GeneratePosition(int n)
        {
            m_initPosition.Clear();
            for (var i = 0; i < tankCount; i++)
            {
                var rp = Random.insideUnitSphere;
                rp.y = 0;
                rp = generatePoint.position + rp.normalized * Random.Range(1, generateRadius);
                m_initPosition.Add(rp);
            }
        }

        /**
         * 初始化所有坦克
         */
		private void GenerateInitial() {
            currentStepsInEpoch = 0;
            // 随机出生点
            GeneratePosition(tankCount);
            // 添加坦克实体
            for (var i = 0; i < tankCount; i++) {
				m_drivers.Add(Instantiate(tankPrefab, m_initPosition[i], Quaternion.identity).GetComponent<TankDriver>());
			}
            
            // 添加坦克控制
            m_drivers[0].Setup(0, Color.black, "User"); // 手动档

            m_drivers[1].Setup(1, Color.red, "Shoot"); // 自动射击档

            m_drivers[2].Setup(2, Color.yellow, "Escape"); // 逃跑档

            m_drivers[3].Setup(3, Color.grey, "L-User");
            m_drivers[3].control.SetStudyTarget(0); // 学习机 学习 手动档

            m_drivers[4].Setup(3, Color.cyan, "L-Shoot"); 
            m_drivers[4].control.SetStudyTarget(1); // 学习机学 习 自动射击档

            m_drivers[6].Setup(3, Color.Lerp(Color.yellow, Color.red, 0.5f), "L-S&E");
            m_drivers[6].control.SetStudyTarget(1); // 学习机学 习 自动射击档
            m_drivers[6].control.SetStudyTarget(2); // 学习机学 习 逃跑档

            m_drivers[5].Setup(3, Color.green , "L-L-S&E");
            m_drivers[5].control.SetStudyTarget(6); // 学习机学 习 学习机

            m_drivers[7].Setup(3, Color.cyan, "L-Shoot");
            m_drivers[7].control.SetStudyTarget(1); // 学习机学 习 自动射击档

            m_drivers[8].Setup(3, Color.Lerp(Color.yellow, Color.red, 0.5f), "L-S&E");
            m_drivers[8].control.SetStudyTarget(1); // 学习机学 习 自动射击档
            m_drivers[8].control.SetStudyTarget(2); // 学习机学 习 逃跑档

            m_drivers[9].Setup(3, Color.cyan, "L-Shoot");
            m_drivers[9].control.SetStudyTarget(1); // 学习机学 习 自动射击档

            for (var i = 10; i < tankCount; i++) m_drivers[i].Setup(-1, Color.white, "Target"); // Bot

            // 初始化坦克
            for (var i = 0; i < tankCount; i++)
            {
                m_drivers[i].GetComponent<Tank>().Setup();
                m_drivers[i].GetComponent<Tank>().setEpoch(epoch);
            }
        }

        /**
         * 坦克重新初始化
         */
		public void RestoreInitial() {
			currentStepsInEpoch = 0;
            GeneratePosition(tankCount);
            for (var i = 0; i < tankCount; i++) {
				m_drivers[i].transform.position = m_initPosition[i];
				m_drivers[i].GetComponent<Tank>().Setup();
                m_drivers[i].GetComponent<Tank>().setEpoch(epoch);
            }
		}

        /**
         * 初始化游戏环境
         */
		private void Start() {
			GenerateInitial();
		}

        /**
         * 更新游戏逻辑
         */
        const float EPS = (float)1e-8;
		private void Update() {
            int flag = 0; // 是否存在存活坦克，不存在直接下一轮

            // 清空输入输出列表
            inputList.Clear();
            outputList.Clear();

            // 执行控制程序决策
            for (var i = 0; i < tankCount; i++)
            {
                //记录输入输出
                var input = m_drivers[i].CalculateNetworkInputs();
                var output = m_drivers[i].Active(input);
                // 学习目标死亡，则不进行学习
                if (!m_drivers[i].GetComponent<Unit>().gameObject.activeInHierarchy) output[0] = output[1] = output[2] = 0f; else flag++;
                // 转化为神经网络输出值
                output[0] = (output[0] + 1f) / 2f;
                output[1] = (output[1] + 1f) / 2f;
                output[2] = (output[2] + 1f) / 2f;
                // 加入列表
                inputList.Add(input);
                outputList.Add(output);
            }

            // 神经网络学习
            for (var i = 0; i < tankCount; i++)
            {
                foreach (var x in m_drivers[i].control.studyTarget)
                {
                    // 只学习运动状态指令
                    if (Mathf.Abs((float)outputList[x][0] - 0.5f) > EPS || Mathf.Abs((float)outputList[x][1] - 0.5f) > EPS || Mathf.Abs((float)outputList[x][2] - 0.5f) > EPS)
                    {
                        //Debug.LogFormat("{0} Learn From {1}", i, x);
                        m_drivers[i].Train(inputList[x], outputList[x]);
                    }
                }
            }

            printResulttToScreen();

            // 清除坦克无故受力
            /*for (var i = 0; i < tankCount; i++)
            {
                m_drivers[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
                m_drivers[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }*/

            // 时间到头重新启动战斗
            currentStepsInEpoch++;
            if (flag<=1 || currentStepsInEpoch > totalStepsPerEpoch || Input.GetKeyDown(KeyCode.Return))
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.LinuxPlayer) printResultToFile("Result.txt");
                epoch++;
                RestoreInitial();
            }
        }

        /**
         * 输出结果到屏幕
         */
        private void printResulttToScreen()
        {
            screenResult.text = "";
            for (var i = 0; i < tankCount; i++)
            {
                screenResult.text = screenResult.text + m_drivers[i].tankName + " " + m_drivers[i].GetComponent<Tank>().score.ToString() + " Acc = " + ((int)100*m_drivers[i].GetComponent<Tank>().ShootHitRate()).ToString() + "% Loss = "+m_drivers[i].lastLoss.ToString()+"\n";
            }
        }

        /**
         * 输出结果到文件
         */
        private void printResultToFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName)) File.Create(fileName);
                StreamWriter sw = new StreamWriter(fileName, true);
                for (var i = 0; i < tankCount; i++)
                {
                    sw.WriteLine("{4} {0} {1} {2} {3}", m_drivers[i].tankName, m_drivers[i].GetComponent<Tank>().score, m_drivers[i].GetComponent<Tank>().ShootHitRate(), m_drivers[i].GetComponent<Unit>().gameObject.activeInHierarchy, epoch);
                }
                sw.Close();
            }catch(Exception err)
            {
                //Do nothing
            }
        }

	
        /**
         * 辅助设计视图
         */
		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(generatePoint.position,generateRadius);
		}
	}

}

