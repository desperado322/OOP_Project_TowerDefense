using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project.Entities
{
    public class Base : Entity
    {
        private List<Vector2> _basePosition;
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public event Action OnHealthOver;

        public Base(int health, List<Vector2> basePosition, Texture2D texture)
        {
            Texture = texture;
            MaxHealth = health;
            Health = MaxHealth;
            _basePosition = basePosition;
            SetPivot();
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (Health < MaxHealth)
            {
                DrawHP(spriteBatch, pixel);
            }
            spriteBatch.Draw(Texture, _basePosition[0], null, Color.White, 0f, Pivot, 1f, SpriteEffects.None, 0f);
        }

        public void GetDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                OnHealthOver?.Invoke();
            }
        }

        public void DrawHP(SpriteBatch spriteBatch, Texture2D pixel)
        {
            spriteBatch.Draw(pixel, new Rectangle((int)(_basePosition[1].X), (int)(_basePosition[1].Y), MaxHealth * 10, 25), Color.Black);
            spriteBatch.Draw(pixel, new Rectangle((int)(_basePosition[1].X), (int)(_basePosition[1].Y), Health * 10, 25), Color.LightGreen);
        }
    }
}
