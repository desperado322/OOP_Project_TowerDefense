using OOP_Project.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OOP_Project.Strategies
{
    public class MaxHPAttackStrategy : IStrategy
    {
        public string Name { get; private set; } = "Цель с наибольшим здоровьем";
        public T FindTarget<T>(Tower tower, List<Enemy> enemies)
        {
            Enemy target = null;
            float maxHP = float.MinValue;

            foreach (var enemy in enemies)
            {
                float squareDistance = Vector2.DistanceSquared(tower.Position, enemy.Position);
                if (squareDistance <= Math.Pow(tower.Stats.Range, 2) && enemy.GetHealth() > maxHP)
                {
                    target = enemy;
                }
            }
            return (T)(object)target;
        }
    }
}
