using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame
{
	
	public class Tank : Unit {

		// 敌人图层
		public LayerMask enemyMask;

		// 移动模块参数
		public float maxSpeed;
		public float maxTorque;

		// 武器模块参数
		public float shootCooldown;
		public bool weaponReady { get; private set; }
		public Transform shootPoint;
		public GameObject bullet;

		// 当前坦克得分
		public float score { get; private set; }
        public float shootCost;
        public float tankMasterScore;
        public Renderer tankColorRenderer1, tankColorRenderer2, tankColorRenderer3, tankColorRenderer4;
        private Color tankNewBieColor = Color.white;
        private Color tankExpertColor = Color.red;
        private Color tankMasterColor = Color.yellow;


        // 当前世界信息
        private int worldEpoch = 0;

        // 当前坦克物理模块
        private Rigidbody m_rigidbody {  get; set; }
		private Collider m_collider;

        // 当前坦克命中率
        private float m_totalshoot = 0;
        private float m_hitshoot = 0f;
       

		/**
		 * 坦克初始化
		 */
		public override void Setup()
        {
			weaponReady = true;
            score = 0f;
            Score(score);
            m_totalshoot = m_hitshoot = 0;
            base.Setup();
		}

        /**
         * 设置坦克代数
         */
         public void setEpoch(int ep)
        {
            worldEpoch = ep;
        }

		/**
		 * 更新物理模块组建
		 */
		private void Start()
        {
            Setup();
            m_rigidbody = GetComponent<Rigidbody>();
			m_collider = GetComponent<Collider>();
		}

		/**
		 * 设置坦克移动加速度
		 * @param : spe 移动速度[-1, 1]
		 * 
		 */
		public void SetMove(float spe)
        {
            m_rigidbody.velocity = transform.forward * maxSpeed * spe;
            //m_rigidbody.transform.Translate(Vector3.forward * maxSpeed * spe);
        }

		/**
		 * 设置坦克旋转加速度
		 * @param : rot 旋转角度[-1, 1]
		 */
		public void SetRotate(float rot)
        {
            m_rigidbody.angularVelocity = transform.up * maxTorque * rot;
            //m_rigidbody.transform.Rotate(Vector3.up * maxTorque * rot * Mathf.PI);
        }

		/**
		 * 设置坦克得分
		 * @param : v 坦克获得得分
		 */
		public void Score(float v)
        {
            // 射中目标
            m_hitshoot++;
            // 得分
			score += v;
            // 更改坦克外观
            if (score <= tankMasterScore)
            {
                //tankColorRenderer1.materials[0].color = Color.Lerp(tankNewBieColor, tankExpertColor, score / tankMasterScore);
                tankColorRenderer2.materials[0].color = Color.Lerp(tankNewBieColor, tankExpertColor, score / tankMasterScore);
                tankColorRenderer3.materials[0].color = Color.Lerp(tankNewBieColor, tankExpertColor, score / tankMasterScore);
                tankColorRenderer4.materials[0].color = Color.Lerp(tankNewBieColor, tankExpertColor, score / tankMasterScore);
            }
            else
            {
                //tankColorRenderer1.materials[0].color = Color.Lerp(tankMasterColor, tankMasterColor, 1);
                tankColorRenderer2.materials[0].color = Color.Lerp(tankMasterColor, tankMasterColor, 1);
                tankColorRenderer3.materials[0].color = Color.Lerp(tankMasterColor, tankMasterColor, 1);
                tankColorRenderer4.materials[0].color = Color.Lerp(tankMasterColor, tankMasterColor, 1);
            }
        }

        /**
         * 设置坦克颜色，用来区分阵营
         */
        public void SetTankColors(Color col)
        {
            tankColorRenderer1.materials[0].color = Color.Lerp(col, col, 1);
        }

		/**
		 * 坦克射击函数
		 */
		public bool Shoot()
        {
			if (!weaponReady || !gameObject.activeSelf) return false;
			weaponReady = false;
            // 射击操作
            score -= shootCost;
            m_totalshoot++;
			Instantiate(bullet, shootPoint.position, shootPoint.rotation).GetComponent<ShootObject>().Setup(Score);
            // 进入冷却
            StartCoroutine(CooldownWeapon(worldEpoch));
            return true;
		}

		/**
		 * 武器冷却
		 */
		private IEnumerator CooldownWeapon(int x)
        {
			yield return new WaitForSeconds(shootCooldown);
			if (x==worldEpoch) weaponReady = true;
		}

        /**
        * 坦克命中率
        */
        public float ShootHitRate()
        {
            if (m_totalshoot < 1) return 0f;
            return m_hitshoot / m_totalshoot;
        }

        /**
         * 坦克死亡操作
         */
        public override void Dead()
        {
			//score -= 40;
			score = Mathf.Max(0, score);
			base.Dead();
		}

        /**
         * 寻找第i近的敌人函数
         * 查找所有范围内坦克并排除自己，按顺序返回第i个
         * @param viewRange : 视野距离
         * @param idx : 第idx近的敌人
         * @return : 第i近的敌人对象
         */
        public Transform ClosestKthEnemy(float viewRange, int idx)
        {
            var cols = new List<Collider>(Physics.OverlapSphere(transform.position, viewRange, enemyMask));
            cols.Remove(m_collider);
            if (cols.Count < idx) return null;
            var ret = cols.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ElementAt(idx - 1);
            return ret != null ? ret.transform : null;
        }

        /**
         * 获得范围内坦克的最大分值
         * 查找所有范围内，排序并返回最大距离
         * @param viewRange : 视野距离
         * @return : 最大分数
         */
        public double CloseEnemyMaxScore(float viewRange)
        {
            var cols = new List<Collider>(Physics.OverlapSphere(transform.position, viewRange, enemyMask));
            //cols.Remove(m_collider);
            if (cols.Count <= 0) return 1d;
            var ret = 1d;
            var list = cols.OrderBy(x => x.GetComponent<Tank>().score);
            if (list.Last() != null) ret = Mathf.Max((float)ret, Mathf.Abs(list.Last().GetComponent<Tank>().score));
            if (list.First() != null) ret = Mathf.Max((float)ret, Mathf.Abs(list.First().GetComponent<Tank>().score));
            return ret;
        }
    }
	
}


