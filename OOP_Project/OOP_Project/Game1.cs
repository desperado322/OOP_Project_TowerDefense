using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OOP_Project
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _enemyTexture;
        private Texture2D _enemyTexture2;
        private Texture2D _enemyTexture3;
        private Texture2D _enemyTexture4;
        private Texture2D _bulletTexture;
        private Texture2D _bulletTexture2;
        private Texture2D _towerPlaceTexture;
        private Texture2D _towerTexture;
        private Texture2D _towerTexture2;
        private Texture2D _towerTexture3;
        private Texture2D _towerTexture4;
        private Texture2D _sellTexture;
        private Texture2D _upgradeTexture;
        private Texture2D _repairTexture;
        private List<Texture2D> _towerTextures = new List<Texture2D>();
        private List<Texture2D> _enemyTextures = new List<Texture2D>();
        private List<Texture2D> _bulletTextures = new List<Texture2D>();
        private List<Texture2D> _menuElementsTextures = new List<Texture2D>();
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
            _enemyTexture2 = Content.Load<Texture2D>("Enemy2");
            _enemyTexture3 = Content.Load<Texture2D>("Enemy3");
            _enemyTexture4 = Content.Load<Texture2D>("Enemy4");
            _towerTexture = Content.Load<Texture2D>("Tower");
            _towerTexture2 = Content.Load<Texture2D>("Tower2");
            _towerTexture3 = Content.Load<Texture2D>("Tower3");
            _towerTexture4 = Content.Load<Texture2D>("Tower4");
            _bulletTexture = Content.Load<Texture2D>("Bullet");
            _bulletTexture2 = Content.Load<Texture2D>("Bullet2");
            _towerPlaceTexture = Content.Load<Texture2D>("TowerPlace");
            _sellTexture = Content.Load<Texture2D>("Sell");
            _upgradeTexture = Content.Load<Texture2D>("Upgrade");
            _repairTexture = Content.Load<Texture2D>("Repair");
            _towerTextures.Add(_towerTexture);
            _towerTextures.Add(_towerTexture2);
            _towerTextures.Add(_towerTexture3);
            _towerTextures.Add(_towerTexture4);
            _enemyTextures.Add(_enemyTexture);
            _enemyTextures.Add(_enemyTexture2);
            _enemyTextures.Add(_enemyTexture3);
            _enemyTextures.Add(_enemyTexture4);
            _bulletTextures.Add(_bulletTexture);
            _bulletTextures.Add(_bulletTexture2);
            _menuElementsTextures.Add(_sellTexture);
            _menuElementsTextures.Add(_upgradeTexture);
            _menuElementsTextures.Add(_repairTexture);
            _towerPlaces.Add(new TowerPlace(new Vector2(200, 200), _towerPlaceTexture, _towerTextures));
            _towerPlaces.Add(new TowerPlace(new Vector2(900, 400), _towerPlaceTexture, _towerTextures));
            _towerPlaces.Add(new TowerPlace(new Vector2(1200, 200), _towerPlaceTexture, _towerTextures));
            _gameManager = new GameManager(_towerPlaces, _towers, _menuElementsTextures, _towerTextures, _bulletTextures, _enemyTextures);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                GameOver();

            _gameManager.Update(gameTime, _enemies, _enemyTextures, _bullets);
            _enemies.RemoveAll(enemy => enemy.GetActive() == false);
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
        T FindTarget<T>(Tower tower, List<Enemy> enemies);
    }

    public abstract class Enemy
    {
        protected float _speed = 100f;
        protected float _traveledDistance = 0f;
        protected float _health;
        protected bool _isActive = true;
        protected ArcLengthParametrization _arcLengthParametrization;
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public float Scale { get; private set; } = 1f;
        public float Rotation { get; private set; }
        public Vector2 Pivot { get; private set; }
        public List<Vector2> Points { get; private set; } = new List<Vector2>() { Vector2.Zero, Vector2.Zero, new Vector2(300, 900), new Vector2(800, 100), new Vector2(1700, 700) };
        
        public Enemy(Texture2D texture)
        {
            Texture = texture;
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
        public virtual void TakeDamage(float damage)
        {
            _health -= damage;
            if (_health <= 0f)
            {
                _isActive = false;
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

        public void SetActive(bool state)
        {
            _isActive = state;
        }
        public bool GetActive()
        {
            return _isActive;
        }

        public float GetHealth()
        {
            return _health;
        }
    }

    public class StandartEnemy : Enemy
    {
        public StandartEnemy(Texture2D texture) : base(texture)
        {
            _health = 100f;
        }
    }

    public class FastEnemy : Enemy
    {
        public FastEnemy(Texture2D texture) : base(texture)
        {
            _health = 50f;
            _speed = 150f;
        }
    }

    public class ResistEnemy : Enemy
    {
        public ResistEnemy(Texture2D texture) : base(texture)
        {
            _health = 80f;
        }

        public override void TakeDamage(float damage)
        {
            _health -= damage * 0.75f;
            if (_health <= 0f)
            {
                _isActive = false;
            }
        }
    }

    public class SpikeEnemy : Enemy
    {
        private float _spikeDamage = 10f;
        public SpikeEnemy(Texture2D texture) : base(texture)
        {
            _health = 50f;
        }

        public float TakeSpikeDamage()
        {
            return _spikeDamage;
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
        protected IStrategy _strategy;
        protected List<IStrategy> _strategies;
        protected List<TowerMenuElement> _menuElements = new List<TowerMenuElement>();
        protected float _MaxHealth = 150f;
        protected float _currentHealth;
        protected float _rangeShooting;
        protected float _fireRate;
        protected float _bulletDamage;
        protected float _bulletSpeed;
        protected int _countOfUpgrades = 0;
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
        public Tower(Vector2 position, TowerStats stats, TowerPlace towerPlace, List<Texture2D> menuElementsTextures, List<IStrategy> strategies)
        {
            Stats = stats;
            Position = position;
            _currentHealth = _MaxHealth;
            _texture = Stats.Texture;
            _rangeShooting = Stats.Range;
            _fireRate = Stats.FireRate;
            _bulletDamage = Stats.BulletDamage;
            _bulletSpeed = Stats.BulletSpeed;
            _place = towerPlace;
            _strategies = strategies;
            _strategy = _strategies[2];
            _bullets = new BulletPool(Stats.BulletTexture, this, _bulletSpeed);
            Pivot = new Vector2(_texture.Width / 2, _texture.Height / 2);
            TowerBounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), _texture.Width, _texture.Height);
            _mouseClick = new MouseClick();

            _menuElements.Add(new TowerMenuElement(Position - new Vector2(100, 0), Pivot, menuElementsTextures[0]));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(-100, 0), Pivot, menuElementsTextures[1]));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, 100), Pivot, menuElementsTextures[2]));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, -100), Pivot, menuElementsTextures[0]));

            foreach (var element in _menuElements)
            {
                element.SellTowerEvent += OnTowerSelled;
                element.UpgradeTowerEvent += OnTowerUpgraded;
                element.RepairTowerEvent += OnTowerRepaired;
            }

            _strategies = strategies;
        }


        public virtual void Update(GameTime gameTime, List<Enemy> enemies)
        {
            if (IsActive)
            {
                foreach (var element in _menuElements)
                {
                    element.Update(gameTime, null, this);
                }
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
                    else
                    {
                        CloseMenu();
                    }
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
                    element.Draw(spriteBatch);
                }
            }
        } 
        protected void OnTowerSelled()
        {
            SellTower?.Invoke(this, _place);
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
                element.UpgradeTowerEvent -= OnTowerUpgraded;
                element.RepairTowerEvent -= OnTowerRepaired;
            }
        }
        protected void OnTowerRepaired()
        {
            _currentHealth = _MaxHealth;
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
            }
        }

        protected void OnTowerUpgraded()
        {
            if (_countOfUpgrades < 5) 
            {
                _rangeShooting *= 1.1f;
                _fireRate /= 1.1f;
                _bulletDamage *= 1.1f;
                _MaxHealth += 10f;
                foreach (var element in _menuElements)
                {
                    element.SetActive(false);
                }
                _countOfUpgrades += 1;
            }
        }
        protected void CloseMenu()
        {
                foreach (var element in _menuElements)
                {
                    element.SetActive(false);
                }   
        }
        
        protected virtual void FireAtCurrentTarget(GameTime gameTime, List<Enemy> enemies)
        {
            if (_currentTarget != null)
            {
                if (!_currentTarget.GetActive())
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
                _currentTarget = _strategy.FindTarget<Enemy>(this, enemies);
            }
        }

        protected virtual void Aim(Enemy enemy)
        {
            Vector2 direction = enemy.Position - Position;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        protected virtual void Shoot(Enemy enemy)
        {
            _bullets.GetBullet(enemy, _bulletDamage);
        }
        protected virtual void CheckCurrentTargetPosition(Enemy enemy)
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

        public void GetDamage(float damage)
        {
            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                IsActive = false;
                _place.SetActive(true);
            }
        }
    }  

    public class BasicTower : Tower
    {
        public BasicTower(Vector2 position, TowerStats stats, TowerPlace towerPlace, List<Texture2D> menuElementsTextures, List<IStrategy> strategies) 
            : base(position, stats, towerPlace, menuElementsTextures, strategies) { }
    }

    public class CannonTower : Tower
    {
        public CannonTower(Vector2 position, TowerStats stats, TowerPlace towerPlace, List<Texture2D> menuElementsTextures, List<IStrategy> strategies) 
            : base(position, stats, towerPlace, menuElementsTextures, strategies) { }

    }

    public class MachineGunTower : Tower
    {
        private float _fireRateLevel;
        public MachineGunTower(Vector2 position, TowerStats stats, TowerPlace towerPlace, List<Texture2D> menuElementsTextures, List<IStrategy> strategies)
            : base(position, stats, towerPlace, menuElementsTextures, strategies) 
        {
            _fireRateLevel = _fireRate;
        }

        protected override void FireAtCurrentTarget(GameTime gameTime, List<Enemy> enemies)
        {
            if (_currentTarget != null)
            {
                if (!_currentTarget.GetActive())
                {
                    _currentTarget = null;
                    return;
                }
                Aim(_currentTarget);
                if (_fireRate >= _fireRateLevel)
                {
                    Shoot(_currentTarget);
                    _fireRate = 0f;
                    if (_fireRateLevel >= 0.5f)
                    {
                        _fireRateLevel -= 5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
                CheckCurrentTargetPosition(_currentTarget);
                _fireRate += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            }
            else if (_currentTarget == null)
            {
                _currentTarget = _strategy.FindTarget<Enemy>(this, enemies);
            }
        }
    }

    public class ManyTargetsTower : Tower
    {
        private List<Enemy> _currentTargets = new List<Enemy>();
        public ManyTargetsTower(Vector2 position, TowerStats stats, TowerPlace towerPlace, List<Texture2D> menuElementsTextures, List<IStrategy> strategies) 
            : base(position, stats, towerPlace, menuElementsTextures, strategies ) 
        {
            _strategy = new AllInRangeAttackStrategy();
        }

        protected override void FireAtCurrentTarget(GameTime gameTime, List<Enemy> enemies)
        {
            if (_currentTargets.Count > 0)
            {
                if (_fireRate >= 1f)
                {
                    foreach (var target in _currentTargets)
                    {
                        Shoot(target);
                        CheckCurrentTargetPosition(target);
                        _fireRate = 0f;
                    }
                }
                _currentTargets.Clear();
            }
            else if (_currentTargets.Count == 0)
            {
                _currentTargets = _strategy.FindTarget<List<Enemy>>(this, enemies);
            }
            _fireRate += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public class ClosestAttackStrategy : IStrategy
    {
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

    public class FirstAttackStrategy : IStrategy
    {
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
                    break;
                }
            }
            return (T)(object)target;
        }
    }

    public class MaxHPAttackStrategy : IStrategy
    {
        public T FindTarget<T>(Tower tower, List<Enemy> enemies)
        {
            Enemy target = null;
            float maxHP = float.MinValue;

            foreach (var enemy in enemies)
            {
                float squareDistance = Vector2.DistanceSquared(tower.Position, enemy.Position);
                if (squareDistance <= Math.Pow(tower.Stats.Range, 2) * tower.Scale && enemy.GetHealth() > maxHP)
                {
                    target = enemy;
                }
            }
            return (T)(object)target;
        }
    }

    public class AllInRangeAttackStrategy : IStrategy
    {
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
            _menuElements.Add(new TowerMenuElement(Position + new Vector2(-100, 0), Pivot, towerTextures[1]));
            _menuElements.Add(new TowerMenuElement(Position + new Vector2(0, 100), Pivot, towerTextures[2]));
            _menuElements.Add(new TowerMenuElement(Position + new Vector2(0, -100), Pivot, towerTextures[3]));

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
        public Texture2D Texture { get; private set; }
        public bool IsActive { get; private set; } = false;
        public event Action<Texture2D> CreateTower;
        public event Action SellTowerEvent;
        public event Action UpgradeTowerEvent;
        public event Action RepairTowerEvent;

        public TowerMenuElement(Vector2 position, Vector2 pivot, Texture2D menuElementTexture)
        {
            Position = position;
            Pivot = pivot;
            Texture = menuElementTexture;
            _mouseClick = new MouseClick();
            _bounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), Texture.Width, Texture.Height);
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
                        CreateTower?.Invoke(this.Texture);
                    }
                    else if (tower != null && tower.IsActive)
                    {
                        switch (this.Texture.Name)
                        {
                            case "Sell": SellTowerEvent?.Invoke(); break;
                            case "Upgrade": UpgradeTowerEvent?.Invoke(); break;
                            case "Repair": RepairTowerEvent?.Invoke(); break;
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Pivot, 0.5f, SpriteEffects.None, 0f);
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

        public Bullet(Texture2D texture, Tower tower, float speed)
        {
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
            _enemyBounds = new Rectangle((int)(_enemy.Position.X - Pivot.X / 2), (int)(_enemy.Position.Y - Pivot.Y / 2), _enemy.Texture.Width / 2, _enemy.Texture.Height / 2);
            _bulletBounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), Texture.Width, Texture.Height);
            if (_bulletBounds.Intersects(_enemyBounds) || !_gameBounds.Contains(_bulletBounds))
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

    public class BulletPool
    {
        private List<Bullet> _bullets = new List<Bullet>();

        public BulletPool(Texture2D bulletTexture, Tower tower, float speed)
        {
            for (int i = 0; i < 50; i++)
            {
                _bullets.Add(new Bullet(bulletTexture, tower, speed));
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

        public Bullet GetBullet(Enemy enemy, float damage)
        {
            var bullet = _bullets.Find(bullet => !bullet.IsActive);
            if (bullet != null)
            {
                bullet.ResetBullet(enemy, damage);
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
        private List<IStrategy> _strategies = new List<IStrategy>() { new ClosestAttackStrategy(), new MaxHPAttackStrategy(), new FirstAttackStrategy(), new AllInRangeAttackStrategy() };
        private List<Texture2D> _menuElementsTextures;
        private float _timer = 0f;

        public GameManager(List<TowerPlace> places, List<Tower> towers, List<Texture2D> menuElementsTextures, List<Texture2D> towerTextures, List<Texture2D> bulletTextures, List<Texture2D> enemyTextures)
        {
            _places = places;
            _towers = towers;
            _menuElementsTextures = menuElementsTextures;
            CreateTowerConfigs(towerTextures, bulletTextures);
            foreach (var place in _places)
            {
                place.CreateTower += OnTowerCreated;
            }
        }

        public void Update(GameTime gameTime, List<Enemy> enemies, List<Texture2D> enemyTextures, List<Bullet> bullets)
        {
            CreateEnemies(gameTime, enemies, enemyTextures);
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
                case "Tower": tower = new BasicTower(towerPlace.Position, stats, towerPlace, _menuElementsTextures, _strategies); break;
                case "Tower2": tower = new CannonTower(towerPlace.Position, stats, towerPlace, _menuElementsTextures, _strategies); break;
                case "Tower3": tower = new MachineGunTower(towerPlace.Position, stats, towerPlace, _menuElementsTextures, _strategies); break;
                case "Tower4": tower = new ManyTargetsTower(towerPlace.Position, stats, towerPlace, _menuElementsTextures, _strategies); break;
            }
            tower.SellTower += OnTowerSelled;
            _towers.Add(tower);

            towerPlace.SetActive(false);
        }
        private void CreateEnemies(GameTime gameTime, List<Enemy> enemies, List<Texture2D> enemyTextures)
        {
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer <= 0)
            {
                Enemy newEnemy = null;
                switch(new Random().Next(0, enemyTextures.Count))
                {
                    case 0: newEnemy = new StandartEnemy(enemyTextures[0]); break;
                    case 1: newEnemy = new FastEnemy(enemyTextures[1]); break;
                    case 2: newEnemy = new ResistEnemy(enemyTextures[2]); break;
                    case 3: newEnemy = new SpikeEnemy(enemyTextures[3]); break;
                }
                enemies.Add(newEnemy);
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
        public void CreateTowerConfigs(List<Texture2D> towerTextures, List<Texture2D> bulletTextures)
        {
            _towerConfigs.Add(towerTextures[0], new TowerStats(towerTextures[0], 400f, 2f, bulletTextures[0], 10f, 750f));
            _towerConfigs.Add(towerTextures[1], new TowerStats(towerTextures[1], 400f, 5f, bulletTextures[1], 50f, 500f));
            _towerConfigs.Add(towerTextures[2], new TowerStats(towerTextures[2], 500f, 2f, bulletTextures[0], 10f, 750f));
            _towerConfigs.Add(towerTextures[3], new TowerStats(towerTextures[3], 600f, 3f, bulletTextures[0], 10f, 600f));
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
