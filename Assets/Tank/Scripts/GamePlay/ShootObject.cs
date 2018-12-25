    using System;
using UnityEngine;

namespace TankGame
{
	public class ShootObject : MonoBehaviour {

        // 子弹伤害物体图层
		public LayerMask hitLayerMask;

        // 子弹信息
        public float hit;
		public float hitRange;
		public float launchSpeed;
        public float hitPoint;
        public float killPoint;

        // 子弹基基本参数
        public float maxLifeTime = 2f;
		public ParticleSystem expEffect;
		private Action<float> m_scoreCallback;
		
        /**
         * 初始化子弹物体
         * @param : 射击回掉函数
         */
		public void Setup(Action<float> scoreCallback) {
			m_scoreCallback = scoreCallback;
            GetComponent<Rigidbody>().velocity = transform.forward * launchSpeed;
            // 定时销毁子弹，节省资源
            Destroy(gameObject, maxLifeTime);
        }

        /**
         * 子弹更新逻辑
         */
		private void Update ()
        {
            // 寻找子弹爆炸范围内的所有坦克
            var list = Physics.OverlapSphere(transform.position, hitRange, hitLayerMask);
			foreach (var item in list)
            {
				var unit = item.GetComponent<Unit>();
				if (!unit) continue;
				m_scoreCallback(unit.ApplyDamage(hit) ? killPoint : hitPoint);
			}
            // 未击中目标
            if (list.Length <= 0) return;
            // 释放爆照效果
            expEffect.transform.parent = null;
            expEffect.Play();
            Destroy(expEffect.gameObject, expEffect.main.duration);
            Destroy(gameObject);
        }

        /**
         * 范围辅助判定
         */
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, hitRange);
        }
    }
}

