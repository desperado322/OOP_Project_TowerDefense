using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Timers;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OOP_Project
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _enemyTexture;
        private Texture2D _bulletTexture;
        private Texture2D _bulletTexture2;
        private Texture2D _towerPlaceTexture;
        private Texture2D _towerTexture;
        private Texture2D _towerTexture2;
        private Texture2D _sellTexture;
        private List<Texture2D> _towerTextures = new List<Texture2D>();
        private List<Texture2D> _bulletTextures = new List<Texture2D>();
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Bullet> _bullets = new List<Bullet>();
        private List<Tower> _towers = new List<Tower>();
        private List<TowerPlace> _towerPlaces = new List<TowerPlace>();
        private GameManager _gameManager;
        private int _screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        private int _screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
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
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _enemyTexture = Content.Load<Texture2D>("Enemy");
            _towerTexture = Content.Load<Texture2D>("Tower");
            _towerTexture2 = Content.Load<Texture2D>("Tower2");
            _bulletTexture = Content.Load<Texture2D>("Bullet");
            _bulletTexture2 = Content.Load<Texture2D>("Bullet2");
            _towerPlaceTexture = Content.Load<Texture2D>("TowerPlace");
            _sellTexture = Content.Load<Texture2D>("Sell");
            _towerTextures.Add(_towerTexture);
            _towerTextures.Add(_towerTexture2);
            _bulletTextures.Add(_bulletTexture);
            _bulletTextures.Add(_bulletTexture2);
            _towerPlaces.Add(new TowerPlace(new Vector2(200, 200), _towerPlaceTexture, _towerTextures));
            _towerPlaces.Add(new TowerPlace(new Vector2(900, 400), _towerPlaceTexture, _towerTextures));
            _towerPlaces.Add(new TowerPlace(new Vector2(1200, 200), _towerPlaceTexture, _towerTextures));
            _gameManager = new GameManager(_towerPlaces, _towers, _sellTexture, _towerTextures, _bulletTextures);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                GameOver();

            _gameManager.Update(gameTime, _enemies, _enemyTexture, _bullets);
            _enemies.RemoveAll(enemy => enemy.IsActive == false);
            for(int i = 0; i < _enemies.Count; i++)
            {

                _enemies[i].Update(gameTime);
                if (_enemies[i].Position == _enemies[i].Points[^1])
                {
                    GameOver();
                }
            }   
            if (_enemies.Count > 0)
            {
                foreach(var tower in _towers)
                {
                    if (tower.IsActive)
                    {
                        tower.Update(gameTime, _enemies);
                    }

                }
            }

            foreach(var bullet in _bullets)
            {
                bullet.Update(gameTime);
            }

            foreach (var towerPlace in _towerPlaces)
            {
                towerPlace.Update(gameTime);
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
            foreach (var bullet in _bullets)
            {
                bullet.Draw(_spriteBatch);
            }
            foreach (var towerPlace in _towerPlaces)
            {
                towerPlace.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void GameOver()
        {
            _gameManager.UnloadLevel();
            Exit();
        }
        
    }

    public interface IStrategy
    {
        Enemy FindTarget(List<Enemy> enemies, Tower tower);
    }

    public class Enemy
    {
        private float _speed = 150f;
        private float _traveledDistance = 0f;
        private ArcLengthParametrization _arcLengthParametrization;
        public float Health;
        public Texture2D Texture {  get; private set; }
        public Vector2 Position { get; private set; }
        public float Scale { get; private set; } = 1f;
        public float Rotation { get; private set; }
        public Vector2 Pivot { get; private set; }
        public bool IsActive { get; private set; } = true;
        public float Timer { get; private set; }
        public List<Vector2> Points { get; private set; } = new List<Vector2>() { Vector2.Zero, Vector2.Zero, new Vector2(300, 900), new Vector2(800, 100), new Vector2(1700, 700) };
        
        public Enemy(Texture2D texture, float health)
        {
            Texture = texture;
            Health = health;
            Pivot = new Vector2(Texture.Width / 2, Texture.Height / 2);
            _arcLengthParametrization = new ArcLengthParametrization(Points);
        }


        public void Update(GameTime gameTime)
        {
            SplineMover(gameTime);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, Pivot, Scale, SpriteEffects.None, 0f);
        }
        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0f)
            {
                IsActive = false;
            }
        }

        private void SplineMover(GameTime gameTime)
        {
            
            var previousPosition = Position;
            _traveledDistance += (float)gameTime.ElapsedGameTime.TotalSeconds * _speed;
            Position = _arcLengthParametrization.GetPositionAtDistance(_traveledDistance);
            var direction = Position - previousPosition;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
    }

    public class SplinePoint
    {
        public Vector2 Position { get; private set; }
        public float Distance { get; private set; }
        public SplinePoint(Vector2 position, float distance)
        {
            Position = position;
            Distance = distance;
        }
    }

    public class ArcLengthParametrization
    {
        private List<SplinePoint> _parts = new List<SplinePoint>();
        public float TotalDistance { get; private set; }
        public ArcLengthParametrization(List<Vector2> points, int countOfParts = 100)
        {
            _parts.Clear();
            Vector2 previousPosition = Vector2.CatmullRom(points[0], points[1], points[2], points[3], 0f);
            _parts.Add(new SplinePoint(previousPosition, 0f));

            float currentDistance = 0f;

            for (int i = 0; i <= countOfParts; i++)
            {
                float timer = (float)i / countOfParts;
                Vector2 currentPosition = Vector2.CatmullRom(points[0], points[1], points[2], points[3], timer);
                currentDistance += Vector2.Distance(previousPosition, currentPosition);
                _parts.Add(new SplinePoint(currentPosition, currentDistance));

                previousPosition = currentPosition;
                if (timer == 1f && currentPosition != points[^1])
                {
                    for (int j = 1; j < points.Count; j++)
                    {
                        points[j - 1] = points[j];
                    }
                    i = 0;
                }
            }
            TotalDistance = currentDistance;
        }
        public Vector2 GetPositionAtDistance(float distance)
        {
            if (distance <= 0)
            {
                return _parts[0].Position;
            }
            if (distance >= TotalDistance)
            {
                return _parts[^1].Position;
            }
            for (int i = 0; i < _parts.Count - 1; i++)
            {
                if (distance >= _parts[i].Distance &&  distance <= _parts[i + 1].Distance)
                {
                    float partLength = _parts[i + 1].Distance - _parts[i].Distance;
                    float traveledDistance = (distance - _parts[i].Distance) / partLength;
                    return Vector2.Lerp(_parts[i].Position, _parts[i + 1].Position, traveledDistance);
                }
            }
            return _parts[^1].Position;
        }
    }

    public abstract class Tower
    {
        protected TowerPlace _place;
        protected MouseClick _mouseClick;
        protected BulletPool _bullets;
        protected object _strategy;
        protected List<object> _strategies;
        protected List<TowerMenuElement> _menuElements = new List<TowerMenuElement>();
        protected float _rangeShooting;
        protected float _fireRate;
        protected float _bulletDamage;
        protected float _bulletSpeed;
        protected Enemy _currentTarget;
        private Texture2D _texture;
        public TowerStats Stats { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Pivot {  get; private set; }
        public float Scale { get; private set; } = 1f;
        public float Rotation { get; private set; }
        public bool IsActive { get; private set; } = true;
        public Rectangle TowerBounds { get; private set; }
        public event Action<Tower, TowerPlace> SellTower;
        public Tower(Vector2 position, TowerStats stats, TowerPlace towerPlace, Texture2D sellTexture, List<object> strategies)
        {
            Stats = stats;
            Position = position;
            _texture = Stats.Texture;
            _rangeShooting = Stats.Range;
            _fireRate = Stats.FireRate;
            _bulletDamage = Stats.BulletDamage;
            _bulletSpeed = Stats.BulletSpeed;
            _place = towerPlace;
            _strategies = strategies;
            _strategy = _strategies[1];
            _bullets = new BulletPool(Stats.BulletTexture, this, _bulletDamage, _bulletSpeed);
            Pivot = new Vector2(_texture.Width / 2, _texture.Height / 2);
            TowerBounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), _texture.Width, _texture.Height);
            _mouseClick = new MouseClick();

            _menuElements.Add(new TowerMenuElement(Position - new Vector2(100, 0), Pivot, sellTexture));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(-100, 0), Pivot, sellTexture));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, 100), Pivot, sellTexture));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, -100), Pivot, sellTexture));

            foreach (var element in _menuElements)
            {
                element.SellTower += OnTowerSelled;
            }

            _strategies = strategies;
        }


        public virtual void Update(GameTime gameTime, List<Enemy> enemies)
        {
            if (IsActive)
            {
                _bullets.Update(gameTime);
                FireAtCurrentTarget(gameTime, enemies);
                Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
                if (_mouseClick.IsClick)
                {
                    if (mousePosition.Intersects(TowerBounds))
                    {
                        foreach (var element in _menuElements)
                        {
                            element.SetActive(true);
                        }
                    }
                }
                foreach (var element in _menuElements)
                {
                    element.Update(gameTime, null, this);
                }
                _mouseClick.Update(gameTime);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                _bullets.Draw(spriteBatch);
                spriteBatch.Draw(_texture, Position, null, Color.White, Rotation, Pivot, Scale, SpriteEffects.None, 0f);
                foreach (var element in _menuElements)
                {
                    element.Draw(spriteBatch, 45f);
                }
            }
        } 
        protected void OnTowerSelled()
        {
            SellTower?.Invoke(this, _place);
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
            }
        }
        
        protected virtual void FireAtCurrentTarget(GameTime gameTime, List<Enemy> enemies)
        {
            if (_currentTarget != null)
            {
                if (!_currentTarget.IsActive)
                {
                    _currentTarget = null;
                    return;
                }
                Aim(_currentTarget);
                if (_fireRate >= 2f)
                {
                    Shoot(_currentTarget);
                    _fireRate = 0f;
                }
                CheckCurrentTargetPosition(_currentTarget);
                _fireRate += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            }
            else if (_currentTarget == null)
            {
                dynamic strategy = _strategy;

                _currentTarget = strategy.FindTarget(enemies, this);
            }
        }

        protected void Aim(Enemy enemy)
        {
            Vector2 direction = enemy.Position - Position;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        protected void Shoot(Enemy enemy)
        {
            _bullets.GetBullet(enemy);
            _fireRate = 0f;
        }
        protected void CheckCurrentTargetPosition(Enemy enemy)
        {
            if (Vector2.DistanceSquared(Position, enemy.Position) > _rangeShooting * _rangeShooting)
            {
                _currentTarget = null;
            }
        }
        public void SetActive(bool state)
        {
            IsActive = state;
        }
    }  

    public class CannonTower : Tower
    {
        public CannonTower(Vector2 position, TowerStats stats, TowerPlace towerPlace, Texture2D sellTexture, List<object> strategies) 
            : base(position, stats, towerPlace, sellTexture, strategies) { }

    }

    public class MachineGunTower : Tower
    {
        private float _fireRateLevel;
        private float _fireRateCounter = 10f;
        public MachineGunTower(Vector2 position, TowerStats stats, TowerPlace towerPlace, Texture2D sellTexture, List<object> strategies)
            : base(position, stats, towerPlace, sellTexture, strategies) 
        {
            _fireRateLevel = _fireRate;
        }

        protected override void FireAtCurrentTarget(GameTime gameTime, List<Enemy> enemies)
        {
            if (_currentTarget != null)
            {
                if (!_currentTarget.IsActive)
                {
                    _currentTarget = null;
                    return;
                }
                Aim(_currentTarget);
                if (_fireRate >= _fireRateLevel)
                {
                    Shoot(_currentTarget);
                    System.Diagnostics.Debug.WriteLine((float)gameTime.ElapsedGameTime.TotalSeconds);
                    _fireRate = 0f;
                    _fireRateLevel -= _fireRateCounter * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_fireRateLevel <= 0f)
                    {
                        _fireRateLevel = 2f;
                    }
                    else if (_fireRateLevel <= 1f)
                    {
                        _fireRateCounter = 5f;
                    }
                }
                CheckCurrentTargetPosition(_currentTarget);
                _fireRate += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            }
            else if (_currentTarget == null)
            {
                dynamic strategy = _strategy;

                _currentTarget = strategy.FindTarget(enemies, this);
            }
        }
    }

    public class ClosestAttackStrategy : IStrategy
    {
        public Enemy FindTarget(List<Enemy> enemies, Tower tower)
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
            return target;
        }
    }
    public class FirstAttackStrategy : IStrategy
    {
        public Enemy FindTarget(List<Enemy> enemies, Tower tower)
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
                    break;
                }
            }
            return target;
        }
    }
    public class MaxHPAttackStrategy : IStrategy
    {
        public Enemy FindTarget(List<Enemy> enemies, Tower tower)
        {
            Enemy target = null;
            float lastTargetPosition = float.MaxValue;
            float maxHP = float.MinValue;

            foreach (var enemy in enemies)
            {
                float squareDistance = Vector2.DistanceSquared(tower.Position, enemy.Position);
                if (squareDistance <= Math.Pow(tower.Stats.Range, 2) * tower.Scale && enemy.Health > maxHP)
                {
                    lastTargetPosition = squareDistance;
                    target = enemy;
                }
            }
            return target;
        }
    }

    public class TowerStats
    {
        public Texture2D Texture {  get; private set; }
        public float Range {  get; private set; }
        public float FireRate {  get; private set; }
        public Texture2D BulletTexture { get; private set; }
        public float BulletDamage { get; private set; }
        public float BulletSpeed { get; private set; }

        public TowerStats(Texture2D texture, float range, float fireRate, Texture2D bulletTexture, float bulletDamage, float bulletSpeed)
        {
            Texture = texture;
            Range = range;
            FireRate = fireRate;
            BulletTexture = bulletTexture;
            BulletDamage = bulletDamage;
            BulletSpeed = bulletSpeed;
        }
    }

    public class TowerPlace 
    {
        private Rectangle _mousePosition;
        private MouseClick _mouseClick;
        private List<TowerMenuElement> _menuElements = new List<TowerMenuElement>();
        public Vector2 Position { get; private set; }
        public Texture2D Texture { get; private set; }
        public Vector2 Pivot { get; private set; }
        public bool IsActive { get; private set; } = true;
        public Rectangle TowerPlaceBounds { get; private set; }
        public event Action<TowerPlace, Texture2D> CreateTower;
        public TowerPlace(Vector2 position, Texture2D texture, List<Texture2D> towerTextures) 
        {
            Position = position;
            Texture = texture;
            Pivot = new Vector2(Texture.Width / 2, Texture.Height / 2);
            TowerPlaceBounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), Texture.Width, Texture.Height);
            _mouseClick = new MouseClick();

            _menuElements.Add(new TowerMenuElement(Position + new Vector2(100, 0), Pivot, towerTextures[0]));
            _menuElements.Add(new TowerMenuElement(Position + new Vector2(-100, 0), Pivot, towerTextures[0]));
            _menuElements.Add(new TowerMenuElement(Position + new Vector2(0, 100), Pivot, towerTextures[1]));
            _menuElements.Add(new TowerMenuElement(Position + new Vector2(0, -100), Pivot, towerTextures[1]));

            foreach (var element in _menuElements)
            {
                element.CreateTower += OnTowerCreated;
            }
            
        }


        public void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                _mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
                if (_mouseClick.IsClick)
                {
                    if (_mousePosition.Intersects(TowerPlaceBounds))
                    {
                        foreach(var element  in _menuElements)
                        {
                            element.SetActive(true);
                        }
                    }
                }
                foreach (var element in _menuElements)
                {
                    element.Update(gameTime, this);
                }
                _mouseClick.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Pivot, 1f, SpriteEffects.None, 0f);
                foreach (var element in _menuElements)
                {
                    element.Draw(spriteBatch);
                }
            }
        }
        private void OnTowerCreated(Texture2D texture)
        {
            CreateTower?.Invoke(this, texture);
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
            }
        }

        public void SetActive(bool state)
        {
            IsActive = state;
        }
    }

    public class TowerMenuElement
    {
        private MouseClick _mouseClick;
        private Rectangle _bounds;
        public Vector2 Position { get; private set; }
        public Vector2 Pivot { get; private set; }
        public Texture2D PlaceMenuTexture { get; private set; }
        public Texture2D TowerMenuTexture { get; private set; }
        public bool IsActive { get; private set; } = false;
        public event Action<Texture2D> CreateTower;
        public event Action SellTower;

        public TowerMenuElement(Vector2 position, Vector2 pivot, Texture2D placeMenuTexture = null, Texture2D towerMenuTexture = null)
        {
            Position = position;
            Pivot = pivot;
            PlaceMenuTexture = placeMenuTexture;
            TowerMenuTexture = towerMenuTexture;
            _mouseClick = new MouseClick();
            _bounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), PlaceMenuTexture.Width, PlaceMenuTexture.Height);
        }

        public void Update(GameTime gameTime, TowerPlace place = null, Tower tower = null)
        {
            if (IsActive)
            {
                _mouseClick.Update(gameTime);
                Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
                if (_mouseClick.IsClick && mousePosition.Intersects(_bounds))
                {
                    if (place != null && place.IsActive)
                    {
                        CreateTower?.Invoke(this.PlaceMenuTexture);
                    }
                    else if (tower != null && tower.IsActive)
                    {
                        SellTower?.Invoke();
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, float rotation = 0f)
        {
            if (IsActive)
            {
                spriteBatch.Draw(PlaceMenuTexture, Position, null, Color.White, rotation, Pivot, 0.5f, SpriteEffects.None, 0f);
            }
        }
        public void SetActive(bool state)
        {
            IsActive = state;
        }

    }

    public class Bullet
    {
        private Tower _tower;
        private Enemy _enemy;
        private Rectangle _gameBounds = new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        private Rectangle _bulletBounds;
        private Rectangle _enemyBounds;
        private float _speed;
        public Texture2D Texture { get; private set; }
        public Vector2 Pivot { get; private set; }
        public Vector2 Position { get; private set; }
        public float Rotation { get; private set; }
        public bool IsActive { get; private set; }
        public float Damage { get; private set; }

        public Bullet(Texture2D texture, Tower tower, float damage, float speed)
        {
            Texture = texture;
            Pivot = new Vector2(Texture.Height / 2, Texture.Width / 2);
            _tower = tower;
            Position = _tower.Position;
            Damage = damage;
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
            _enemyBounds = new Rectangle((int)(_enemy.Position.X - Pivot.X / 2), (int)(_enemy.Position.Y - Pivot.Y / 2), _enemy.Texture.Width / 2, _enemy.Texture.Height / 2);
            _bulletBounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), Texture.Width, Texture.Height);
            if (_bulletBounds.Intersects(_enemyBounds) || !_gameBounds.Contains(_bulletBounds))
            {
                DestroyBullet(this);
                _enemy.TakeDamage(Damage);
            }
        }
        private void DestroyBullet(Bullet bullet)
        {
            IsActive = false;
        }

        public void ResetBullet(Enemy enemy)
        {
            Position = _tower.Position;
            _enemy = enemy;
            IsActive = true;
        }
    }

    public class BulletPool
    {
        private List<Bullet> _bullets = new List<Bullet>();

        public BulletPool(Texture2D bulletTexture, Tower tower, float damage, float speed)
        {
            for (int i = 0; i < 50; i++)
            {
                _bullets.Add(new Bullet(bulletTexture, tower, damage, speed));
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Bullet bullet in _bullets)
            {
                if (bullet.IsActive)
                {
                    bullet.Update(gameTime);
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Bullet bullet in _bullets)
            {
                if (bullet.IsActive)
                {
                    bullet.Draw(spriteBatch);
                }
            }
        }

        public Bullet GetBullet(Enemy enemy)
        {
            var bullet = _bullets.Find(bullet => !bullet.IsActive);
            if (bullet != null)
            {
                bullet.ResetBullet(enemy);
                return bullet;
            }
            return null;
        }
    }

    public class GameManager
    {
        private Dictionary<Texture2D, TowerStats> _towerConfigs = new Dictionary<Texture2D, TowerStats>();
        private List<TowerPlace> _places;
        private List<Tower> _towers;
        private List<object> _strategies = new List<object>() { new ClosestAttackStrategy(), new MaxHPAttackStrategy(), new FirstAttackStrategy() };
        private Texture2D _sellTexture;
        private float _timer = 0f;

        public GameManager(List<TowerPlace> places, List<Tower> towers, Texture2D sellTexture, List<Texture2D> towerTextures, List<Texture2D> bulletTextures)
        {
            _places = places;
            _towers = towers;
            _sellTexture = sellTexture;
            CreateConfigs(towerTextures[0], towerTextures[1], bulletTextures[0], bulletTextures[1]);
            foreach (var place in _places)
            {
                place.CreateTower += OnTowerCreated;
            }
        }

        public void Update(GameTime gameTime, List<Enemy> enemies, Texture2D enemyTexture, List<Bullet> bullets)
        {
            CreateEnemies(gameTime, enemies, enemyTexture);
        }
        private void OnTowerSelled(Tower tower, TowerPlace place)
        {
            tower.SetActive(false);
            place.SetActive(true);
        }
        private void OnTowerCreated(TowerPlace towerPlace, Texture2D towerTexture)
        {
            TowerStats stats = GetStats(towerTexture);
            Tower tower = null;
            if (stats == null)
            {
                return;
            }
            switch (stats.Texture.Name)
            {
                case "Tower":
                    tower = new CannonTower(towerPlace.Position, stats, towerPlace, _sellTexture, _strategies);
                    break;
                case "Tower2":
                    tower = new MachineGunTower(towerPlace.Position, stats, towerPlace, _sellTexture, _strategies);
                    break;
            }
            tower.SellTower += OnTowerSelled;
            _towers.Add(tower);

            towerPlace.SetActive(false);
        }
        private void CreateEnemies(GameTime gameTime, List<Enemy> enemies, Texture2D enemyTexture)
        {
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer <= 0)
            {
                float health = new Random().Next(1, 100);
                enemies.Add(new Enemy(enemyTexture, health));
                _timer = 1f;
            }
        }
        private TowerStats GetStats(Texture2D towerTexture)
        {
            if (_towerConfigs.TryGetValue(towerTexture, out var stats))
            {
                return stats;
            }
            return null;
        }
        public void CreateConfigs(Texture2D towerTexture1, Texture2D towerTexture2, Texture2D bulletTexture1, Texture2D bulletTexture2)
        {
            _towerConfigs.Add(towerTexture1, new TowerStats(towerTexture1, 400f, 5f, bulletTexture1, 50f, 500f));
            _towerConfigs.Add(towerTexture2, new TowerStats(towerTexture2, 400f, 2f, bulletTexture2, 1f, 750f));
        }
        public void UnloadLevel()
        {
            foreach (var place in _places)
            {
                place.CreateTower -= OnTowerCreated;
            }
            foreach (var tower in _towers)
            {
                tower.SellTower -= OnTowerSelled;
            }
        }
    }

    public class MouseClick
    {
        private MouseState _previousState;
        public bool IsClick { get; private set; } = false;

        public MouseClick()
        {
            _previousState = Mouse.GetState();
        }

        public void Update(GameTime gameTime)
        {
            MouseState currentState = Mouse.GetState();
            IsClick = currentState.LeftButton == ButtonState.Pressed && _previousState.LeftButton == ButtonState.Released;
            _previousState = currentState;
        }
    }
}
