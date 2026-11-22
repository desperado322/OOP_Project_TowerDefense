using OOP_Project.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OOP_Project.Strategies
{
    public class ClosestAttackStrategy : IStrategy
    {
        public string Name { get; private set; } = "Ближайшая цель";
        public T FindTarget<T>(Tower tower, List<Enemy> enemies)
        {
            Enemy target = null;
            float lastTargetPosition = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float squareDistance = Vector2.DistanceSquared(tower.Position, enemy.Position);
                if (squareDistance <= Math.Pow(tower.Stats.Range, 2) * tower.Scale && squareDistance < lastTargetPosition)
                {
                    lastTargetPosition = squareDistance;
                    target = enemy;
                }
            }
            return (T)(object)target;
        }
    }
}
