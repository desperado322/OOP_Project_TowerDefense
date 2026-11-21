using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using OOP_Project.Scenes;
using System;
using System.Collections.Generic;

namespace OOP_Project.Entities
{
    public abstract class Enemy : Entity
    {
        protected Texture2DAtlas _atlas;
        protected AnimatedSprite _animatedSprite;
        protected float _speed;
        protected float _traveledDistance = 0f;
        protected ArcLengthParametrization _arcLengthParametrization;
        protected float _health;
        public List<Vector2> Points { get; private set; }

        public Enemy(EnemyStats stats, float randomPath, Texture2DAtlas atlas = null)
        {
            SetStats(stats, randomPath);
            _atlas = atlas;
            _arcLengthParametrization = new ArcLengthParametrization(Points);
            if (_atlas != null)
            {
                Animate();
            }
        }

        public void Update(GameTime gameTime)
        {
            UpdateBounds();
            SplineMover(gameTime);
            if (_animatedSprite != null)
            {
                _animatedSprite.Update(gameTime);
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_animatedSprite != null)
            {
                spriteBatch.Draw(_animatedSprite, Position, Rotation);
            }
            else
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, Pivot, Scale, SpriteEffects.None, 0f);
            }
        }

        protected void SetStats(EnemyStats stats, float randomPath)
        {
            _speed = stats.Speed;
            _health = stats.Health;
            IsActive = true;
            Scale = 1f;
            Texture = stats.Texture;
            Pivot = new Vector2(Texture.Width / 2, Texture.Height / 2);
            Points = new List<Vector2>() { new (100 + randomPath, 0), new (100 + randomPath, 0), new (100 + randomPath, 900 + randomPath),
                new (100 + randomPath, 900 + randomPath), new (400 + randomPath, 900 + randomPath), new (400 + randomPath, 900 + randomPath),
                new (400 + randomPath, 200 + randomPath),  new (400 + randomPath, 200 + randomPath), new (900 + randomPath, 200 + randomPath),
                new (900 + randomPath, 200 + randomPath), new (900 + randomPath, 900 + randomPath), new (900 + randomPath, 900 + randomPath) };
        }
        public virtual void TakeDamage(float damage)
        {
            _health -= damage;
            if (_health <= 0f)
            {
                IsActive = false;
            }
        }

        protected void SplineMover(GameTime gameTime)
        {
            var previousPosition = Position;
            _traveledDistance += (float)gameTime.ElapsedGameTime.TotalSeconds * _speed;
            Position = _arcLengthParametrization.GetPositionAtDistance(_traveledDistance);
            var direction = Position - previousPosition;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }

        protected void Animate()
        {
            string run = "Run";
            SpriteSheet spriteSheet = new SpriteSheet("EnemyRun", _atlas);
            spriteSheet.DefineAnimation(run, builder =>
            {
                builder.IsLooping(true).AddFrame($"Sprite-1", TimeSpan.FromSeconds(0.1f))
                .AddFrame($"Sprite-2", TimeSpan.FromSeconds(0.2f))
                .AddFrame($"Sprite-3", TimeSpan.FromSeconds(0.3f))
                .AddFrame($"Sprite-4", TimeSpan.FromSeconds(0.4f))
                .AddFrame($"Sprite-1", TimeSpan.FromSeconds(0.5f))
                .AddFrame($"Sprite-2", TimeSpan.FromSeconds(0.6f))
                .AddFrame($"Sprite-3", TimeSpan.FromSeconds(0.7f))
                .AddFrame($"Sprite-4", TimeSpan.FromSeconds(0.8f));
            });
            _animatedSprite = new AnimatedSprite(spriteSheet, run);
            _animatedSprite.Origin = new Vector2(_animatedSprite.TextureRegion.Width / 2f, _animatedSprite.TextureRegion.Height / 2f);
        }

        protected override void UpdateBounds()
        {
            Bounds = new Rectangle(
                (int)(Position.X - Pivot.X / 2 * Scale),
                (int)(Position.Y - Pivot.Y / 2 * Scale),
                (int)(Texture.Width / 2 * Scale),
                (int)(Texture.Height / 2 * Scale)
            );
        }

        public void SetActive(bool state)
        {
            IsActive = state;
        }
        public bool GetActive()
        {
            return IsActive;
        }

        public float GetHealth()
        {
            return _health;
        }
    }
}
