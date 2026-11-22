using OOP_Project.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OOP_Project.Strategies
{
    public class AllInRangeAttackStrategy : IStrategy
    {
        public string Name { get; private set; } = "Все цели в радиусе";
        public T FindTarget<T>(Tower tower, List<Enemy> enemies)
        {
            List<Enemy> targets = new List<Enemy>();

            foreach (var enemy in enemies)
            {
                float squareDistance = Vector2.DistanceSquared(tower.Position, enemy.Position);
                if (squareDistance <= Math.Pow(tower.Stats.Range, 2) * tower.Scale)
                {
                    targets.Add(enemy);
                }
            }
            return (T)(object)targets;
        }
    }
}
