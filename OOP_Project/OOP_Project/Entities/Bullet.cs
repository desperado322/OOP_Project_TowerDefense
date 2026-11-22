using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OOP_Project.Scenes;
using System;

namespace OOP_Project.Entities
{
    public class Bullet : Entity
    {
        private Tower _tower;
        private Enemy _enemy;
        private Rectangle _gameBounds = new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        private float _speed;
        public float Damage { get; private set; }

        public Bullet(Texture2D texture, Tower tower, float speed)
        {
            Scale = 1f;
            Texture = texture;
            Pivot = new Vector2(Texture.Height / 2, Texture.Width / 2);
            _tower = tower;
            Position = _tower.Position;
            _speed = speed;
        }
        public void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                BulletMove(gameTime);
                CheckCollision();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, Pivot, 1f, SpriteEffects.None, 0f);
            }
        }

        private void BulletMove(GameTime gameTime)
        {
            Vector2 direction = (_enemy.Position + _enemy.Pivot) - (_tower.Position + _tower.Pivot);
            direction.Normalize();
            Position += direction * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        private void CheckCollision()
        {
            UpdateBounds();
            if (Bounds.Intersects(_enemy.Bounds) || !_gameBounds.Contains(Bounds))
            {
                DestroyBullet(this);
                _enemy.TakeDamage(Damage);
                if (_enemy.GetType() == typeof(SpikeEnemy))
                {
                    _tower.GetDamage(((SpikeEnemy)_enemy).TakeSpikeDamage());
                }
            }
        }
        private void DestroyBullet(Bullet bullet)
        {
            IsActive = false;
        }

        public void ResetBullet(Enemy enemy, float damage)
        {
            Position = _tower.Position;
            _enemy = enemy;
            Damage = damage;
            IsActive = true;
        }
    }
}
