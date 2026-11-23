using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using OOP_Project.Entities;
using OOP_Project.Strategies;
using System;
using System.Collections.Generic;
using MonoGame.Extended.Input;


namespace OOP_Project.Scenes
{
    public class GamePlayScene : Scene
    {
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Bullet> _bullets = new List<Bullet>();
        private List<Tower> _towers = new List<Tower>();
        private List<TowerPlace> _towerPlaces = new List<TowerPlace>();
        private List<Vector2> _towerPositions;
        private List<Vector2> _splinePoints;
        private RoadType _roadType;
        private GameManager _gameManager;
        private WaveManager _waveManager;
        private EconomyManager _economyManager;
        private GameAssets _gameAssets;

        public GamePlayScene(Game1 game, List<Vector2> towerPositions, List<Vector2> splinePoints, RoadType roadType) : base(game)
        { 
            _towerPositions = towerPositions;
            _splinePoints = splinePoints;
            _roadType = roadType;
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
            _font = _contentManager.Load<SpriteFont>("Fonts/Font1");
            _gameManager = new GameManager(_towerPlaces, _towerPositions, _gameAssets.TowerPlaceTexture, _towers, _gameAssets.MenuElementsTextures, 
                _gameAssets.TowerTextures, _gameAssets.BulletTextures, _gameAssets.EnemyTextures, 
                _font, _gameAssets.Atlases, _splinePoints);
            _economyManager = new EconomyManager(_gameManager);
        }
        public override void Update(GameTime gameTime)
        {
            KeyboardExtended.Update();
            MouseExtended.Update();
            if (KeyboardExtended.GetState().WasKeyPressed(Keys.Escape))
            {
                _game.SceneManager.PushScene(new PauseScene(_game));
                return;
            }
                
            _waveManager.Update(gameTime, _enemies.Count);
            _gameManager.Update(gameTime, _enemies, _gameAssets.EnemyTextures, _bullets, _waveManager.IsWaveActive, _economyManager.GetBalance());
            foreach (var enemy in _enemies)
            {
                enemy.Update(gameTime);
                if (enemy.Position == enemy.Points[^1])
                {
                    _game.SceneManager.PushScene(new GameOverScene(_game, "Игра окончена"));
                    return;
                }
            }

            foreach (var tower in _towers)
            {
                if (tower.IsActive)
                {
                    tower.Update(gameTime, _enemies, _economyManager.GetBalance());
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
            spriteBatch.Draw(_gameAssets.RoadTextures[_roadType], Vector2.Zero, Color.Brown);
            _waveManager.Draw(spriteBatch, _gameAssets.FontUI);
            _economyManager.Draw(spriteBatch, _gameAssets.FontUI);

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
                towerPlace.Draw(spriteBatch, _font);
            }
            foreach (var tower in _towers)
            {
                tower.Draw(spriteBatch);
            }
            foreach (var tower in _towers)
            {
                tower.DrawUI(spriteBatch, _gameAssets.Pixel, _font);
            }
        }
        private void OnLevelWon()
        {
            _game.SceneManager.PushScene(new GameOverScene(_game, "Вы победили"));
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
        public Dictionary<RoadType, Texture2D> RoadTextures { get; private set; }
        public Texture2D Pixel { get; private set; }
        public List<Texture2DAtlas> Atlases { get; private set; }
        public Texture2D TowerPlaceTexture { get; private set; }
        public SpriteFont FontUI { get; private set; }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            RoadTextures = new Dictionary<RoadType, Texture2D>
            {
                { RoadType.Road1, content.Load<Texture2D>("Roads/Road1") },
                { RoadType.Road2, content.Load<Texture2D>("Roads/Road2") },
                { RoadType.Road3, content.Load<Texture2D>("Roads/Road3") },
                { RoadType.Road4, content.Load<Texture2D>("Roads/Road4") },
                { RoadType.Road5, content.Load<Texture2D>("Roads/Road5") },
            };
            TowerTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("Towers/Tower"),
                content.Load<Texture2D>("Towers/Tower2"),
                content.Load<Texture2D>("Towers/Tower3"),
                content.Load<Texture2D>("Towers/Tower4")
            };
            EnemyTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("Enemies/Enemy"),
                content.Load<Texture2D>("Enemies/Enemy2"),
                content.Load<Texture2D>("Enemies/Enemy3"),
                content.Load<Texture2D>("Enemies/Enemy4")
            };
            BulletTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("Bullets/Bullet"),
                content.Load<Texture2D>("Bullets/Bullet2")
            };
            MenuElementsTextures = new List<Texture2D>()
            {
                content.Load<Texture2D>("TowerUI/Sell"),
                content.Load<Texture2D>("TowerUI/Upgrade"),
                content.Load<Texture2D>("TowerUI/Repair"),
                content.Load<Texture2D>("TowerUI/Strategy"),
                content.Load<Texture2D>("TowerUI/BlockStrategy")
            };
            Atlases = new List<Texture2DAtlas>()
            {
                content.Load<Texture2DAtlas>("Enemies/spritesheet1"),
                content.Load<Texture2DAtlas>("Enemies/spritesheet2")
            };
            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
            TowerPlaceTexture = content.Load<Texture2D>("Towers/TowerPlace");
            FontUI = content.Load<SpriteFont>("Fonts/UI Font");
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
        private Dictionary<Texture2D, EnemyStats> _enemyConfigs = new Dictionary<Texture2D, EnemyStats>();
        private Dictionary<TowerType, Func<TowerPlace, TowerStats, Tower>> _towerConfigsFactory;
        private Dictionary<EnemyType, Func<EnemyStats, Enemy>> _enemyConfigsFactory;
        private List<Texture2DAtlas> _atlases;
        private List<TowerPlace> _places;
        private List<Tower> _towers;
        private List<IStrategy> _strategies = new List<IStrategy>() { new ClosestAttackStrategy(), new MaxHPAttackStrategy(), new FirstAttackStrategy(), new AllInRangeAttackStrategy() };
        private List<Texture2D> _menuElementsTextures;
        private float _timer = 0f;
        private float _currentBalance = 0f;
        private Random _randomizedEnemy;
        public Dictionary<Texture2D, TowerStats> TowerConfigs { get; private set; } = new Dictionary<Texture2D, TowerStats>();
        public event Action<float> KillEnemy;
        public event Action<float> BuyTower;
        public event Action<float> RepairTower;
        public event Action<float> UpgradeTower;
        public event Action<float> SellTower;

        public GameManager(List<TowerPlace> places, List<Vector2> towerPositions, Texture2D placeTexture, List<Tower> towers, List<Texture2D> menuElementsTextures, List<Texture2D> towerTextures, List<Texture2D> bulletTextures, List<Texture2D> enemyTextures, SpriteFont font, List<Texture2DAtlas> atlases, List<Vector2> splinePoints)
        {
            _randomizedEnemy = new Random();
            _towers = towers;
            _menuElementsTextures = menuElementsTextures;
            _atlases = atlases;
            CreateTowerConfigs(towerTextures, bulletTextures, font);
            CreateTowerFactories();
            CreateEnemyConfigs(enemyTextures);
            CreateEnemyFactories(splinePoints);
            InitializeTowerPlaces(places, towerPositions, placeTexture, towerTextures);
        }

        public void InitializeTowerPlaces(List<TowerPlace> places, List<Vector2> towerPositions, Texture2D placeTexture, List<Texture2D> towerTextures)
        {
            foreach (var towerPosition in towerPositions)
            {
                places.Add(new TowerPlace(towerPosition, placeTexture, towerTextures, TowerConfigs));
            }
            _places = places;
            foreach (var place in  _places)
            {
                place.CreateTower += OnTowerCreated;
            }
        }

        public void Update(GameTime gameTime, List<Enemy> enemies, List<Texture2D> enemyTextures, List<Bullet> bullets, bool isWaveActive, float balance)
        {
            _currentBalance = balance;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i].IsActive)
                {
                    KillEnemy?.Invoke(enemies[i].Stats.Bounty);
                    enemies.RemoveAt(i);
                }
            }
            
            if (isWaveActive)
            {
                CreateEnemies(gameTime, enemies, enemyTextures);
            }
        }

        private void OnTowerCreated(TowerPlace towerPlace, Texture2D towerTexture)
        {
            if (!TowerConfigs.TryGetValue(towerTexture, out TowerStats stats))
            {
                return;
            }
            if (_towerConfigsFactory.TryGetValue(stats.Type, out var factory) && _currentBalance >= stats.Cost)
            {
                Tower tower = factory(towerPlace, stats);
                tower.SellTower += OnTowerSelled;
                tower.RepairTower += OnTowerRepaired;
                tower.UpgradeTower += OnTowerUpgraded;
                _towers.Add(tower);
                towerPlace.SetActive(false);
                BuyTower?.Invoke(stats.Cost);
            }

        }
        private void OnTowerSelled(Tower tower, TowerPlace place)
        {
            SellTower?.Invoke(tower.Stats.Cost / 2);
            tower.SetActive(false);
            place.SetActive(true);
        }

        private void OnTowerUpgraded(Tower tower)
        {
            UpgradeTower?.Invoke(tower.Stats.Cost / 2);
        }

        private void OnTowerRepaired(Tower tower)
        {
            RepairTower?.Invoke(tower.MaxHealth - tower.CurrentHealth);
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
            TowerConfigs.Add(towerTextures[0], new TowerStats(towerTextures[0], 400f, 2f, bulletTextures[0], 20f, 750f, font, TowerType.Basic, 150f, 50));
            TowerConfigs.Add(towerTextures[1], new TowerStats(towerTextures[1], 400f, 5f, bulletTextures[1], 50f, 500f, font, TowerType.Cannon, 100f, 150));
            TowerConfigs.Add(towerTextures[2], new TowerStats(towerTextures[2], 500f, 2f, bulletTextures[0], 15f, 750f, font, TowerType.MachineGun, 120f, 200));
            TowerConfigs.Add(towerTextures[3], new TowerStats(towerTextures[3], 600f, 3f, bulletTextures[0], 10f, 600f, font, TowerType.ManyTargets, 200f, 300));
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
            _enemyConfigs.Add(enemyTextures[0], new EnemyStats(enemyTextures[0], 100f, 100f, EnemyType.Standart, 20));
            _enemyConfigs.Add(enemyTextures[1], new EnemyStats(enemyTextures[1], 50f, 150f, EnemyType.Fast, 30));
            _enemyConfigs.Add(enemyTextures[2], new EnemyStats(enemyTextures[2], 200f, 50f, EnemyType.Resist, 50));
            _enemyConfigs.Add(enemyTextures[3], new EnemyStats(enemyTextures[3], 80f, 100f, EnemyType.Spike, 40));
        }

        private void CreateEnemyFactories(List<Vector2> splinePoints)
        {
            _enemyConfigsFactory = new Dictionary<EnemyType, Func<EnemyStats, Enemy>>();
            _enemyConfigsFactory.Add(EnemyType.Standart, (stats) => new StandartEnemy(stats, _randomizedEnemy.Next(-25, 26), splinePoints, _atlases[(int)EnemyType.Standart]));
            _enemyConfigsFactory.Add(EnemyType.Fast, (stats) => new FastEnemy(stats, _randomizedEnemy.Next(-25, 26), splinePoints, _atlases[(int)EnemyType.Fast]));
            _enemyConfigsFactory.Add(EnemyType.Resist, (stats) => new ResistEnemy(stats, _randomizedEnemy.Next(-25, 26), splinePoints));
            _enemyConfigsFactory.Add(EnemyType.Spike, (stats) => new SpikeEnemy(stats, _randomizedEnemy.Next(-25, 26), splinePoints));
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
                tower.UpgradeTower -= OnTowerUpgraded;
                tower.RepairTower -= OnTowerRepaired;
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
        private int _countOfEnemies;
        public bool IsWaveActive { get; private set; }
        public event Action WinLevel;

        public WaveManager()
        {
            _timer = 0f;
        }

        public void Update(GameTime gameTime, int enemiesCount)
        {
            _countOfEnemies = enemiesCount;
            if (_countOfWaves >= _maxCountOfWaves && !_hasWon && _countOfEnemies == 0)
            {
                _hasWon = true;
                WinLevel?.Invoke();
            }
            if (_timer < (_timeToEndWave + _timeToStartWave))
            {
                _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (_timer >= (_timeToEndWave + _timeToStartWave))
            {
                if (_countOfEnemies == 0)
                {
                    _timer = 0f;
                    _countOfWaves += 1f;
                    _timeToEndWave *= 2;
                }
                IsWaveActive = false;
            }
            else if (_timer >= _timeToStartWave && _countOfWaves < _maxCountOfWaves && _countOfEnemies == 0)
            {
                IsWaveActive = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (IsWaveActive || _countOfEnemies > 0) 
            {
                spriteBatch.DrawString(font, $"До конца волны: {(int)((_timeToEndWave + _timeToStartWave) - _timer)}", new Vector2(800, 50), Color.White);
                spriteBatch.DrawString(font, $"Осталось врагов: {_countOfEnemies}", new Vector2(800, 75), Color.White);
            }
            else
            {
                spriteBatch.DrawString(font, $"До начала волны: {(int)(_timeToStartWave - _timer)}", new Vector2(800, 50), Color.White);
                spriteBatch.DrawString(font, $"Оставшихся волн: {_maxCountOfWaves - _countOfWaves}", new Vector2(800, 75), Color.White);
            }
        }
    }

    public class EconomyManager
    {
        private float _balance = 150f;
        public EconomyManager(GameManager gameManager)
        {
            gameManager.KillEnemy += OnEnemyKilled;
            gameManager.BuyTower += OnTowerBought;
            gameManager.RepairTower += OnTowerRepaired;
            gameManager.UpgradeTower += OnTowerUpgraded;
            gameManager.SellTower += OnTowerSelled;
        }

        private void OnTowerSelled(float cost)
        {
            _balance += cost;
        }

        private void OnTowerUpgraded(float cost)
        {
            _balance -= cost;
        }

        private void OnTowerRepaired(float cost)
        {
            _balance -= cost;
        }

        private void OnTowerBought(float cost)
        {
            _balance -= cost;
        }

        private void OnEnemyKilled(float bounty)
        {
            _balance += bounty;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.DrawString(font, _balance.ToString(), new Vector2(1800, 100), Color.Coral);
        }

        public float GetBalance() 
        { 
            return _balance; 
        }
    }
}
