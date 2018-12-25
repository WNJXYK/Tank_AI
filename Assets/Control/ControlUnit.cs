using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame;
using System;

namespace ControlSystem
{
    public abstract class ControlUnit
    {

        /**
         * 检查两个数字是否相等
         */
        const double EPS = 1e-8;
        protected bool check(double a, double b)
        {
            return Mathf.Abs((float)a - (float)b) < EPS;
        }

        protected Tank target;
        public List<int> studyTarget = new List<int>();

        public void Setup(Tank m_tank)
        {
            target = m_tank;
            studyTarget.Clear();
        }

        /**
         * 设置学习目标
         */
        public void SetStudyTarget(int x)
        {
            if (studyTarget.Contains(x)) return;
            studyTarget.Add(x);
        }

        /**
         * 计算决策
         */
        virtual public double[] Calculate(double[] input, bool isSave)
        {
            return null;
        }

        /**
         * 控制坦克移动
         */
        public void Control(double v0, double v1, double v2)
        {
            if ((float)v2 > 0) target.Shoot();
            target.SetMove((float)v0);
            target.SetRotate((float)v1);
        }

        /**
         * 训练网络
         */
        virtual public double Train(double[] input, double[] output)
        {
            return 0;
        }

    }
}
