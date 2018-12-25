using UnityEngine;
using UnityEngine.UI;

namespace TankGame
{

	public class Unit : MonoBehaviour {

		// 生命值
		public float fullHealth = 100f;
		private float m_curHealth;

        // 生命值UI
		public Slider healthSlider;
		public Image healthFillImage;
		public Color fullHealthColor = Color.green;
		public Color zeroHealthColor = Color.red;  
		public float health{
			set { m_curHealth = fullHealth * value; }
			get{ return m_curHealth / fullHealth; }
		}

        // 受到伤害
        private int m_hurt = 0;
        public bool isHurt
        {
            get {
                if (m_hurt > 0)
                {
                    m_hurt--;
                    return true;
                }
                else return false;
            }

        }


        // 初始化物体
        public virtual void Setup() {
            healthSlider.maxValue= 1;
            healthSlider.minValue = 0;
			health = 1f;
            m_hurt = 0;
            updateHealthUI();
            gameObject.SetActive(true);
		}

        /**
         * 更新生命条UI
         */
        private void updateHealthUI()
        {
            healthSlider.value = health;
            healthFillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, health);
        }

		// 应用外来伤害
		public bool ApplyDamage(float point) {
			m_curHealth -= point;
            m_hurt = 18;
            updateHealthUI();
            if (!(m_curHealth <= 0)) return false;
			Dead();
			return true;
		}

		// 物体死亡
		public virtual void Dead() {
			gameObject.SetActive(false);
		}
		
	}
	
}