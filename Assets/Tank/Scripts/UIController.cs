using UnityEngine;
using UnityEngine.UI;

namespace TankGame
{
	public class UIController : MonoBehaviour
    {
        
        // 显示游戏进度
		public Text label;
        public Slider slider;
        // 世界控制进程
		public WorldController controller;

        /**
         * 初始化UI
         */
        private void Start()
        {
            slider.maxValue = controller.totalStepsPerEpoch;
            slider.minValue = 0;
            
        }

        /**
         * 更新UI
         */
        private void Update () {
            label.text =  controller.epoch.ToString();
            slider.value = controller.currentStepsInEpoch;
        }

	}
}

