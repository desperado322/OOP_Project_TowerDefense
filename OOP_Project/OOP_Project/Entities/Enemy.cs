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
        public EnemyStats Stats { get; private set; }
        public List<Vector2> Points;

        public Enemy(EnemyStats stats, float randomPath, List<Vector2> splinePoints, Texture2DAtlas atlas = null)
        {
            SetStats(stats, randomPath);
            Points = new List<Vector2>();
            Points.AddRange(splinePoints);
            for (int i = 0;  i < Points.Count; i++)
            {
                var point = Points[i];
                point.X += randomPath;
                point.Y += randomPath;
                Points[i] = point;
            }
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
            Stats = stats;
            _speed = stats.Speed;
            _health = stats.Health;
            IsActive = true;
            Scale = 1f;
            Texture = stats.Texture;
            Pivot = new Vector2(Texture.Width / 2, Texture.Height / 2);
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

    public enum EnemyType
    {
        Standart,
        Fast,
        Resist,
        Spike
    }

    public class EnemyStats
    {
        public Texture2D Texture { get; private set; }
        public float Health { get; private set; }
        public float Speed { get; private set; }
        public EnemyType Type { get; private set; }
        public float Bounty { get; private set; }

        public EnemyStats(Texture2D texture, float health, float speed, EnemyType type, float bounty)
        {
            Texture = texture;
            Health = health;
            Speed = speed;
            Type = type;
            Bounty = bounty;
        }
    }

    public class StandartEnemy : Enemy
    {
        public StandartEnemy(EnemyStats stats, float randomPath, List<Vector2> splinePoints, Texture2DAtlas atlas) : base(stats, randomPath, splinePoints, atlas) { }
    }

    public class FastEnemy : Enemy
    {
        public FastEnemy(EnemyStats stats, float randomPath, List<Vector2> splinePoints) : base(stats, randomPath, splinePoints) { }
    }

    public class ResistEnemy : Enemy
    {
        public ResistEnemy(EnemyStats stats, float randomPath, List<Vector2> splinePoints) : base(stats, randomPath, splinePoints) { }
        public override void TakeDamage(float damage)
        {
            _health -= damage * 0.75f;
            if (_health <= 0f)
            {
                IsActive = false;
            }
        }
    }

    public class SpikeEnemy : Enemy
    {
        private float _spikeDamage = 10f;
        public SpikeEnemy(EnemyStats stats, float randomPath, List<Vector2> splinePoints) : base(stats, randomPath, splinePoints) { }

        public float TakeSpikeDamage()
        {
            return _spikeDamage;
        }
    }
}
