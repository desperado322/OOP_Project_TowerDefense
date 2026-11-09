using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace OOP_Project
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private float _timer = 2f;
        private Texture2D _enemyTexture;
        private Texture2D _towerTexture;
        private Texture2D _bulletTexture;
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Bullet> _bullets = new List<Bullet>();
        private List<Tower> _towers = new List<Tower>();
        private int _screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        private int _screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        public static Rectangle Bounds { get; private set; }
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferHeight = _screenHeight;
            _graphics.PreferredBackBufferWidth = _screenWidth;
            _graphics.ApplyChanges();
            Window.Title = "Tower Defense";
            Bounds = new Rectangle(0, 0, _screenWidth, _screenHeight);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Font1");
            _enemyTexture = Content.Load<Texture2D>("Enemy");
            _towerTexture = Content.Load<Texture2D>("Tower");
            _bulletTexture = Content.Load<Texture2D>("Bullet");
            _towers.Add(new Tower(new Vector2(200, 200), _towerTexture));
            _towers.Add(new Tower(new Vector2(900, 600), _towerTexture));
            _towers.Add(new Tower(new Vector2(1200, 200), _towerTexture));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            CreateEnemies(gameTime);
            for(int i = 0; i < _enemies.Count; i++)
            {
                if (!_enemies[i].IsActive)
                {
                    _enemies.RemoveAt(i);
                    break;
                }
                _enemies[i].Update(gameTime);
                if (_enemies[i].EnemyPosition == _enemies[i].Points[^1])
                {
                    GameOver();
                }
            }
            if (_enemies.Count > 0)
            {
                foreach(var tower in _towers)
                {
                    tower.Update(gameTime, _enemies, _bulletTexture, _bullets);
                }
            }
            foreach(var bullet in _bullets)
            {
                bullet.Update(gameTime);
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            foreach (var enemy in _enemies)
            {
                enemy.Draw(_spriteBatch);
            }
            foreach (var tower in _towers)
            {
                tower.Draw(_spriteBatch);
            }
            foreach(var bullet in _bullets)
            {
                bullet.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void GameOver()
        {
            Exit();
        }
        private void CreateEnemies(GameTime gameTime)
        {
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer <= 0)
            {
                _enemies.Add(new Enemy(_enemyTexture));
                _timer = 2f;
            }
        }
    }

    public class Enemy
    {
        public Texture2D EnemyTexture {  get; private set; }
        public Vector2 EnemyPosition { get; private set; }
        public float Scale { get; private set; } = 1f;
        public float Rotation { get; private set; }
        public Vector2 Pivot { get; private set; }
        public bool IsActive { get; private set; } = true;
        public float Timer { get; private set; }
        private float _speed = 0.2f;
        public List<Vector2> Points { get; private set; } = new List<Vector2>() { Vector2.Zero, Vector2.Zero, new Vector2(300, 900), new Vector2(800, 100), new Vector2(1700, 700) };
        private float _health = 100f;
        
        public Enemy(Texture2D enemyTexture)
        {
            EnemyTexture = enemyTexture;
            Pivot = new Vector2(EnemyTexture.Width / 2, EnemyTexture.Height / 2);
        }

        public void Update(GameTime gameTime)
        {
            SplineMover(gameTime);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(EnemyTexture, EnemyPosition, null, Color.White, Rotation, Pivot, Scale, SpriteEffects.None, 0f);
        }
        public void TakeDamage(float damage)
        {
            _health -= damage;
            if (_health <= 0f)
            {
                IsActive = false;

            }
        }

        private void SplineMover(GameTime gameTime)
        {
            Timer += _speed * (float)gameTime.ElapsedGameTime.TotalSeconds / Vector2.DistanceSquared(Points[1], Points[2]) * 500000;

            if (Timer >= 1f)
            {
                Timer = 0f;
                ChangeCurrentPoint();
            }
            var previousPosition = EnemyPosition;
            EnemyPosition = Vector2.CatmullRom(Points[0], Points[1], Points[2], Points[3], Timer);
            var direction = EnemyPosition - previousPosition;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        private void ChangeCurrentPoint()
        {
            for (int i = 1; i < Points.Count; i++)
            {
                Points[i - 1] = Points[i];
            }
        }
    }

    public class Tower
    {
        public Texture2D TowerTexture { get; private set; }
        public Vector2 TowerPosition { get; private set; }
        private float _rangeShooting = 300f;
        private Enemy _currentTarget;
        public float Scale { get; private set; } = 1f;
        public float Rotation { get; private set; }
        public Vector2 Pivot {  get; private set; }
        private float _fireRate = 1f;
        public Tower(Vector2 towerPosition, Texture2D towerTexture)
        {
            TowerPosition = towerPosition;
            TowerTexture = towerTexture;
            Pivot = new Vector2(TowerTexture.Width / 2, TowerTexture.Height / 2);
        }
        public void Update(GameTime gameTime, List<Enemy> enemies, Texture2D bulletTexture, List<Bullet> bullets)
        {
            bullets.RemoveAll(bullet => bullet.IsActive == false);
            if (_currentTarget != null)
            {
                if (!_currentTarget.IsActive)
                {
                    _currentTarget = null;
                    return;
                }
                Aim(_currentTarget);
                if (_fireRate >= 1f)
                {
                    Shoot(bulletTexture, _currentTarget, bullets);
                }
                CheckCurrentTargetPosition(_currentTarget);
                _fireRate += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            }
            else if (_currentTarget == null)
            {
                _currentTarget = FindTarget(enemies);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TowerTexture, TowerPosition, null, Color.White, Rotation, Pivot, Scale, SpriteEffects.None, 0f);
        }   
        private Enemy FindTarget(List<Enemy> enemies)
        {
            Enemy target = null;
            float lastTargetPosition = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float squareDistance = Vector2.DistanceSquared(TowerPosition, enemy.EnemyPosition);
                if (squareDistance <= _rangeShooting * _rangeShooting * Scale && squareDistance < lastTargetPosition)
                {
                    lastTargetPosition = squareDistance;
                    target = enemy;
                }
            }
            return target;
        }
        private void Aim(Enemy enemy)
        {
            Vector2 direction = enemy.EnemyPosition - TowerPosition;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        private void Shoot(Texture2D bulletTexture, Enemy enemy, List<Bullet> bullets)
        {
            var bullet = new Bullet(bulletTexture, this, enemy, 20f);
            bullets.Add(bullet);
            _fireRate = 0f;
        }
        private void CheckCurrentTargetPosition(Enemy enemy)
        {
            if (Vector2.DistanceSquared(TowerPosition, enemy.EnemyPosition) > _rangeShooting * _rangeShooting)
            {
                _currentTarget = null;
            }
        }
    }

    public class Bullet
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Pivot { get; private set; }
        public Vector2 Position { get; private set; }
        public float Rotation { get; private set; }
        public bool IsActive { get; private set; } = true;
        public float Damage { get; private set; }
        private Tower _tower;
        private Enemy _enemy;
        private Rectangle _bulletBounds;
        private Rectangle _enemyBounds;
        private float _speed = 750f;
        public Bullet(Texture2D texture, Tower tower, Enemy enemy, float damage)
        {
            Texture = texture;
            Pivot = new Vector2(Texture.Height / 2, Texture.Width / 2);
            _tower = tower;
            _enemy = enemy;
            Position += _tower.TowerPosition;
            Damage = damage;
            
        }
        public void Update(GameTime gameTime)
        {
            BulletMove(gameTime);
            CheckCollision();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, Pivot, 1f, SpriteEffects.None, 0f);
        }

        private void BulletMove(GameTime gameTime)
        {
            Vector2 direction = (_enemy.EnemyPosition + _enemy.Pivot * 2) - (_tower.TowerPosition + _tower.Pivot);
            direction.Normalize();
            Position += direction * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        private void CheckCollision()
        {
            _enemyBounds = new Rectangle((int)_enemy.EnemyPosition.X, (int)_enemy.EnemyPosition.Y, _enemy.EnemyTexture.Width, _enemy.EnemyTexture.Height);
            _bulletBounds = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            if (_bulletBounds.Intersects(_enemyBounds) || !Game1.Bounds.Contains(_bulletBounds))
            {
                DestroyBullet(this);
                _enemy.TakeDamage(Damage);
            }
        }
        private void DestroyBullet(Bullet bullet)
        {
            IsActive = false;
        }
    }
}
