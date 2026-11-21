using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using OOP_Project.Entities;


namespace OOP_Project.Scenes
{
    public class GamePlayScene : Scene
    {
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Bullet> _bullets = new List<Bullet>();
        private List<Tower> _towers = new List<Tower>();
        private List<TowerPlace> _towerPlaces = new List<TowerPlace>();
        private List<Vector2> _towerPositions = new List<Vector2>()
        {
            new Vector2(100, 100),
            new Vector2(200, 200),
            new Vector2(300, 300),
            new Vector2(400, 900),
            new Vector2(500, 800),
            new Vector2(600, 700),
            new Vector2(1000, 700),
            new Vector2(200, 700),
            new Vector2(100, 900),
            new Vector2(1200, 800),
        };
        private GameManager _gameManager;
        private WaveManager _waveManager;
        private GameAssets _gameAssets;

        public GamePlayScene(Game1 game) : base(game)
        { 
            _game = game;
        }

        public override void Initialize()
        {
            _contentManager = new ContentManager(_game.Services, "Content");
            _gameAssets = new GameAssets();
            _waveManager = new WaveManager();
            _waveManager.WinLevel += OnLevelWon;
        }


        public override void LoadContent()
        {
            _gameAssets.Load(_contentManager, _graphicsDevice); 
            _font = _contentManager.Load<SpriteFont>("Font1");
            foreach (var towerPosition in _towerPositions)
            {
                _towerPlaces.Add(new TowerPlace(towerPosition, _gameAssets.TowerPlaceTexture, _gameAssets.TowerTextures));
            }
            _gameManager = new GameManager(_towerPlaces, _towers, _gameAssets.MenuElementsTextures, _gameAssets.TowerTextures, _gameAssets.BulletTextures, _gameAssets.EnemyTextures, _font, _gameAssets.Atlas);
        }
        public override void Update(GameTime gameTime)
        {
            _waveManager.Update(gameTime);
            _gameManager.Update(gameTime, _enemies, _gameAssets.EnemyTextures, _bullets, _waveManager.IsWaveActive);
            _enemies.RemoveAll(enemy => enemy.GetActive() == false);
            for (int i = 0; i < _enemies.Count; i++)
            {

                _enemies[i].Update(gameTime);
                if (_enemies[i].Position == _enemies[i].Points[^1])
                {
                    _game.SceneManager.PushScene(new GameOverScene(_game));
                    return;
                }
            }

            foreach (var tower in _towers)
            {
                if (tower.IsActive)
                {
                    tower.Update(gameTime, _enemies);
                }

            }
            foreach (var bullet in _bullets)
            {
                bullet.Update(gameTime);
            }

            foreach (var towerPlace in _towerPlaces)
            {
                towerPlace.Update(gameTime);
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _waveManager.Draw(spriteBatch, _gameAssets.FontUI);

            foreach (var enemy in _enemies)
            {
                enemy.Draw(spriteBatch);
            }
            foreach (var bullet in _bullets)
            {
                bullet.Draw(spriteBatch);
            }
            foreach (var towerPlace in _towerPlaces)
            {
                towerPlace.Draw(spriteBatch);
            }
            foreach (var tower in _towers)
            {
                tower.Draw(spriteBatch);
            }
            foreach (var tower in _towers)
            {
                tower.DrawUI(spriteBatch, _gameAssets.Pixel);
            }
        }
        private void OnLevelWon()
        {
            _game.SceneManager.ChangeScene(new GameOverScene(_game));
            _waveManager.WinLevel -= OnLevelWon;
        }

        public override void UnloadContent()
        {
            _contentManager.Unload();
            base.UnloadContent();
        }
    }

    public class GameAssets
    {
        public List<Texture2D> TowerTextures { get; private set; }
        public List<Texture2D> EnemyTextures { get; private set; }
        public List<Texture2D> BulletTextures { get; private set; }
        public List<Texture2D> MenuElementsTextures { get; private set; }
        public Texture2D Pixel { get; private set; }
        public Texture2DAtlas Atlas { get; private set; }
        public Texture2D TowerPlaceTexture { get; private set; }
        public SpriteFont FontUI { get; private set; }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            TowerTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("Tower"),
                content.Load<Texture2D>("Tower2"),
                content.Load<Texture2D>("Tower3"),
                content.Load<Texture2D>("Tower4")
            };
            EnemyTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("Enemy"),
                content.Load<Texture2D>("Enemy2"),
                content.Load<Texture2D>("Enemy3"),
                content.Load<Texture2D>("Enemy4")
            };
            BulletTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("Bullet"),
                content.Load<Texture2D>("Bullet2")
            };
            MenuElementsTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("Sell"),
                content.Load<Texture2D>("Upgrade"),
                content.Load<Texture2D>("Repair"),
                content.Load<Texture2D>("Strategy"),
                content.Load<Texture2D>("BlockStrategy")
            };
            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
            TowerPlaceTexture = content.Load<Texture2D>("TowerPlace");
            FontUI = content.Load<SpriteFont>("UI Font");
            Atlas = content.Load<Texture2DAtlas>("spritesheet");
        }

    }

    public interface IStrategy
    {
        string Name { get; }
        T FindTarget<T>(Tower tower, List<Enemy> enemies);
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
        public Texture2D Texture {  get; private set; }
        public float Health { get; private set; }
        public float Speed { get; private set; }
        public EnemyType Type { get; private set; }

        public EnemyStats(Texture2D texture, float health, float speed, EnemyType type)
        {
            Texture = texture;
            Health = health;
            Speed = speed;
            Type = type;
        }
    }

    public class StandartEnemy : Enemy
    {
        public StandartEnemy(EnemyStats stats, float randomPath, Texture2DAtlas atlas) : base(stats, randomPath, atlas) { }
    }

    public class FastEnemy : Enemy
    {
        public FastEnemy(EnemyStats stats, float randomPath) : base(stats, randomPath) { }
    }

    public class ResistEnemy : Enemy
    {
        public ResistEnemy(EnemyStats stats, float randomPath) : base(stats, randomPath) { }
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
        public SpikeEnemy(EnemyStats stats, float randomPath) : base(stats, randomPath) { }

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
                    for (int j = 2; j < points.Count;)
                    {
                        points[j - 2] = points[j];
                        points[j - 1] = points[j + 1];
                        j += 2;
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
                if (distance >= _parts[i].Distance && distance <= _parts[i + 1].Distance)
                {
                    float partLength = _parts[i + 1].Distance - _parts[i].Distance;
                    float traveledDistance = (distance - _parts[i].Distance) / partLength;
                    return Vector2.Lerp(_parts[i].Position, _parts[i + 1].Position, traveledDistance);
                }
            }
            return _parts[^1].Position;
        }
    }

    public enum TowerType
    {
        Basic,
        Cannon,
        MachineGun,
        ManyTargets
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

        public override void GetDamage(float damage)
        {
            damage *= 3;
            base.GetDamage(damage);
        }
    }

    public class MachineGunTower : Tower
    {
        public MachineGunTower(Vector2 position, TowerStats stats, TowerPlace towerPlace, List<Texture2D> menuElementsTextures, List<IStrategy> strategies)
            : base(position, stats, towerPlace, menuElementsTextures, strategies)
        {
            _attackCooldown = _fireRate;
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
                if (_fireRate >= _attackCooldown)
                {
                    Shoot(_currentTarget);
                    _fireRate = 0f;
                    if (_attackCooldown >= 0.5f)
                    {
                        _attackCooldown = MathF.Round(_attackCooldown - 5f * (float)gameTime.ElapsedGameTime.TotalSeconds, 2);
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
            : base(position, stats, towerPlace, menuElementsTextures, strategies)
        {
            _menuElements[^1].ChangeTexture(menuElementsTextures[4]);
            _strategy = strategies[^1];
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

        protected override void SubscribeEvents(TowerMenuElement element)
        {
            element.SellTowerEvent += OnTowerSelled;
            element.UpgradeTowerEvent += OnTowerUpgraded;
            element.RepairTowerEvent += OnTowerRepaired;
        }

        protected override void UnSubscribeEvents(TowerMenuElement element)
        {
            element.UpgradeTowerEvent -= OnTowerUpgraded;
            element.RepairTowerEvent -= OnTowerRepaired;
        }
    }

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

    public class FirstAttackStrategy : IStrategy
    {
        public string Name { get; private set; } = "Первая цель";
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
        public string Name { get; private set; } = "Цель с наибольшим здоровьем";
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

    public enum ElementType
    {
        BasicTower,
        CannonTower,
        MachineGunTower,
        ManyTargetsTower,
        Sell,
        Upgrade,
        Repair,
        Strategy
    }

    public class TowerStats
    {
        public TowerType Type { get; private set; }
        public SpriteFont Font { get; private set; }
        public Texture2D Texture { get; private set; }
        public float Range { get; private set; }
        public float FireRate { get; private set; }
        public Texture2D BulletTexture { get; private set; }
        public float BulletDamage { get; private set; }
        public float BulletSpeed { get; private set; }
        public float MaxHealth { get; private set; }

        public TowerStats(Texture2D texture, float range, float fireRate, Texture2D bulletTexture, float bulletDamage, float bulletSpeed, SpriteFont font, TowerType type, float maxHealth)
        {
            Type = type;
            Texture = texture;
            Range = range;
            FireRate = fireRate;
            BulletTexture = bulletTexture;
            BulletDamage = bulletDamage;
            BulletSpeed = bulletSpeed;
            Font = font;
            MaxHealth = maxHealth;
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
        private Dictionary<Texture2D, EnemyStats> _enemyConfigs = new Dictionary<Texture2D, EnemyStats>();
        private Dictionary<TowerType, Func<TowerPlace, TowerStats, Tower>> _towerConfigsFactory;
        private Dictionary<EnemyType, Func<EnemyStats, Enemy>> _enemyConfigsFactory;
        private Texture2DAtlas _atlas;
        private List<TowerPlace> _places;
        private List<Tower> _towers;
        private List<IStrategy> _strategies = new List<IStrategy>() { new ClosestAttackStrategy(), new MaxHPAttackStrategy(), new FirstAttackStrategy(), new AllInRangeAttackStrategy() };
        private List<Texture2D> _menuElementsTextures;
        private float _timer = 0f;
        private Random _randomizedEnemy;

        public GameManager(List<TowerPlace> places, List<Tower> towers, List<Texture2D> menuElementsTextures, List<Texture2D> towerTextures, List<Texture2D> bulletTextures, List<Texture2D> enemyTextures, SpriteFont font, Texture2DAtlas atlas)
        {
            _randomizedEnemy = new Random();
            _places = places;
            _towers = towers;
            _menuElementsTextures = menuElementsTextures;
            _atlas = atlas;
            CreateTowerConfigs(towerTextures, bulletTextures, font);
            CreateTowerFactories();
            CreateEnemyConfigs(enemyTextures);
            CreateEnemyFactories();
            foreach (var place in _places)
            {
                place.CreateTower += OnTowerCreated;
            }
        }


        public void Update(GameTime gameTime, List<Enemy> enemies, List<Texture2D> enemyTextures, List<Bullet> bullets, bool isWaveActive)
        {
            if (isWaveActive)
            {
                CreateEnemies(gameTime, enemies, enemyTextures);
            }
        }

        private void OnTowerSelled(Tower tower, TowerPlace place)
        {
            tower.SetActive(false);
            place.SetActive(true);
        }
        private void OnTowerCreated(TowerPlace towerPlace, Texture2D towerTexture)
        {
            if (!_towerConfigs.TryGetValue(towerTexture, out TowerStats stats))
            {
                return;
            }
            if (_towerConfigsFactory.TryGetValue(stats.Type, out var factory))
            {
                Tower tower = factory(towerPlace, stats);
                tower.SellTower += OnTowerSelled;
                _towers.Add(tower);
                towerPlace.SetActive(false);
            }

        }

        private void CreateEnemies(GameTime gameTime, List<Enemy> enemies, List<Texture2D> enemyTextures)
        {
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer <= 0)
            {
                Texture2D enemyTexture = enemyTextures[_randomizedEnemy.Next(0, enemyTextures.Count)];
                if (!_enemyConfigs.TryGetValue(enemyTexture, out EnemyStats stats))
                {
                    return;
                }
                if (_enemyConfigsFactory.TryGetValue(stats.Type, out var factory))
                {
                    Enemy enemy = factory(stats);
                    enemies.Add(enemy);
                }
                _timer = 1f;
            }
        }
        private void CreateTowerConfigs(List<Texture2D> towerTextures, List<Texture2D> bulletTextures, SpriteFont font)
        {
            _towerConfigs.Add(towerTextures[0], new TowerStats(towerTextures[0], 400f, 2f, bulletTextures[0], 10f, 750f, font, TowerType.Basic, 150f));
            _towerConfigs.Add(towerTextures[1], new TowerStats(towerTextures[1], 400f, 5f, bulletTextures[1], 50f, 500f, font, TowerType.Cannon, 100f));
            _towerConfigs.Add(towerTextures[2], new TowerStats(towerTextures[2], 500f, 2f, bulletTextures[0], 10f, 750f, font, TowerType.MachineGun, 120f));
            _towerConfigs.Add(towerTextures[3], new TowerStats(towerTextures[3], 600f, 3f, bulletTextures[0], 10f, 600f, font, TowerType.ManyTargets, 200f));
        }

        private void CreateTowerFactories()
        {
            _towerConfigsFactory = new Dictionary<TowerType, Func<TowerPlace, TowerStats, Tower>>();
            _towerConfigsFactory.Add(TowerType.Basic, (place, stats) => new BasicTower(place.Position, stats, place, _menuElementsTextures, _strategies));
            _towerConfigsFactory.Add(TowerType.Cannon, (place, stats) => new CannonTower(place.Position, stats, place, _menuElementsTextures, _strategies));
            _towerConfigsFactory.Add(TowerType.MachineGun, (place, stats) => new MachineGunTower(place.Position, stats, place, _menuElementsTextures, _strategies));
            _towerConfigsFactory.Add(TowerType.ManyTargets, (place, stats) => new ManyTargetsTower(place.Position, stats, place, _menuElementsTextures, _strategies));
        }

        private void CreateEnemyConfigs(List<Texture2D> enemyTextures)
        {
            _enemyConfigs.Add(enemyTextures[0], new EnemyStats(enemyTextures[0], 100f, 100f, EnemyType.Standart));
            _enemyConfigs.Add(enemyTextures[1], new EnemyStats(enemyTextures[1], 50f, 150f, EnemyType.Fast));
            _enemyConfigs.Add(enemyTextures[2], new EnemyStats(enemyTextures[2], 200f, 50f, EnemyType.Resist));
            _enemyConfigs.Add(enemyTextures[3], new EnemyStats(enemyTextures[3], 80f, 100f, EnemyType.Spike));
        }

        private void CreateEnemyFactories()
        {
            _enemyConfigsFactory = new Dictionary<EnemyType, Func<EnemyStats, Enemy>>();
            _enemyConfigsFactory.Add(EnemyType.Standart, (stats) => new StandartEnemy(stats, _randomizedEnemy.Next(-25, 26), _atlas));
            _enemyConfigsFactory.Add(EnemyType.Fast, (stats) => new FastEnemy(stats, _randomizedEnemy.Next(-25, 26)));
            _enemyConfigsFactory.Add(EnemyType.Resist, (stats) => new ResistEnemy(stats, _randomizedEnemy.Next(-25, 26)));
            _enemyConfigsFactory.Add(EnemyType.Spike, (stats) => new SpikeEnemy(stats, _randomizedEnemy.Next(-25, 26)));
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

    public class WaveManager
    {
        private float _timeToStartWave = 5f;
        private float _timeToEndWave = 5f;
        private float _timer;
        private float _countOfWaves = 0f;
        private float _maxCountOfWaves = 5f;
        private bool _hasWon = false;
        public bool IsWaveActive { get; private set; }
        public event Action WinLevel;

        public WaveManager()
        {
            _timer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            if (_countOfWaves >= _maxCountOfWaves && !_hasWon)
            {
                _hasWon = true;
                WinLevel?.Invoke();
            }

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer >= _timeToStartWave && _countOfWaves < _maxCountOfWaves)
            {
                IsWaveActive = true;
                if (_timer >= (_timeToEndWave + _timeToStartWave))
                {
                    IsWaveActive = false;
                    _timer = 0f;
                    _countOfWaves += 1f;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (IsWaveActive) 
            {
                spriteBatch.DrawString(font, $"До конца волны: {(int)((_timeToEndWave + _timeToStartWave) - _timer)}", new Vector2(800, 50), Color.White);
            }
            else
            {
                spriteBatch.DrawString(font, $"До начала волны: {(int)(_timeToStartWave - _timer)}", new Vector2(800, 50), Color.White);
                spriteBatch.DrawString(font, $"Оставшихся волн: {_maxCountOfWaves - _countOfWaves}", new Vector2(800, 75), Color.White);
            }
        }
    }
}
