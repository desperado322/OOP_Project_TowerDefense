using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Timers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OOP_Project
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private float _timer = 0f;
        private Texture2D _enemyTexture;
        private Texture2D _bulletTexture;
        private Texture2D _towerPlaceTexture;
        private Texture2D _towerTexture;
        private Texture2D _towerTexture2;
        private Texture2D _sellTexture;
        private List<Texture2D> _towerTextures = new List<Texture2D>();
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
            _font = Content.Load<SpriteFont>("Font1");
            _enemyTexture = Content.Load<Texture2D>("Enemy");
            _towerTexture = Content.Load<Texture2D>("Tower");
            _towerTexture2 = Content.Load<Texture2D>("Tower2");
            _bulletTexture = Content.Load<Texture2D>("Bullet");
            _towerPlaceTexture = Content.Load<Texture2D>("TowerPlace");
            _sellTexture = Content.Load<Texture2D>("Sell");
            _towerTextures.Add(_towerTexture);
            _towerTextures.Add(_towerTexture2);
            _towerPlaces.Add(new TowerPlace(new Vector2(200, 200), _towerPlaceTexture, _towerTextures));
            _towerPlaces.Add(new TowerPlace(new Vector2(900, 400), _towerPlaceTexture, _towerTextures));
            _towerPlaces.Add(new TowerPlace(new Vector2(1200, 200), _towerPlaceTexture, _towerTextures));
            _gameManager = new GameManager(_towerPlaces, _towers, _sellTexture);
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
                _towers.RemoveAll(tower => tower.IsActive == false);
                foreach(var tower in _towers)
                {
                    if (tower.IsActive)
                    {
                        tower.Update(gameTime, _enemies, _bulletTexture, _bullets);
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

    public class Enemy
    {
        private float _speed = 150f;
        private float _traveledDistance = 0f;
        private float _health = 100f;
        private ArcLengthParametrization _arcLengthParametrization;
        public Texture2D Texture {  get; private set; }
        public Vector2 Position { get; private set; }
        public float Scale { get; private set; } = 1f;
        public float Rotation { get; private set; }
        public Vector2 Pivot { get; private set; }
        public bool IsActive { get; private set; } = true;
        public float Timer { get; private set; }
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

    public class Tower
    {
        private Enemy _currentTarget;
        private TowerPlace _place;
        private MouseClick _mouseClick;
        private List<TowerMenuElement> _menuElements = new List<TowerMenuElement>();
        private float _rangeShooting = 300f;
        private float _fireRate = 2f;
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Pivot {  get; private set; }
        public float Scale { get; private set; } = 1f;
        public float Rotation { get; private set; }
        public bool IsActive { get; private set; } = true;
        public Rectangle TowerBounds { get; private set; }
        public event Action<Tower, TowerPlace> SellTower;
        public Tower(Vector2 position, Texture2D texture, TowerPlace towerPlace, Texture2D sellTexture)
        {
            Position = position;
            Texture = texture;
            Pivot = new Vector2(Texture.Width / 2, Texture.Height / 2);
            TowerBounds = new Rectangle((int)(Position.X - Pivot.X), (int)(Position.Y - Pivot.Y), Texture.Width, Texture.Height);
            _place = towerPlace;
            _mouseClick = new MouseClick();

            _menuElements.Add(new TowerMenuElement(Position - new Vector2(100, 0), Pivot, sellTexture));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(-100, 0), Pivot, sellTexture));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, 100), Pivot, sellTexture));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, -100), Pivot, sellTexture));

            foreach (var element in _menuElements)
            {
                element.SellTower += OnTowerSelled;
            }
        }


        public void Update(GameTime gameTime, List<Enemy> enemies, Texture2D bulletTexture, List<Bullet> bullets)
        {
            if (IsActive)
            {
                FireAtCurrentTarget(gameTime, enemies, bulletTexture, bullets);
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

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, Pivot, Scale, SpriteEffects.None, 0f);
                foreach (var element in _menuElements)
                {
                    element.Draw(spriteBatch, 45f);
                }
            }
        } 
        private void OnTowerSelled()
        {
            SellTower?.Invoke(this, _place);
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
            }
        }
        
        private void FireAtCurrentTarget(GameTime gameTime, List<Enemy> enemies, Texture2D bulletTexture, List<Bullet> bullets)
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
                    Shoot(bulletTexture, _currentTarget, bullets);
                    _fireRate = 0f;
                }
                CheckCurrentTargetPosition(_currentTarget);
                _fireRate += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            }
            else if (_currentTarget == null)
            {
                _currentTarget = FindTarget(enemies);
            }
        }
        private Enemy FindTarget(List<Enemy> enemies)
        {
            Enemy target = null;
            float lastTargetPosition = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float squareDistance = Vector2.DistanceSquared(Position, enemy.Position);
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
            Vector2 direction = enemy.Position - Position;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        private void Shoot(Texture2D bulletTexture, Enemy enemy, List<Bullet> bullets)
        {
            var bullet = new Bullet(bulletTexture, this, enemy, 10f);
            bullets.Add(bullet);
            _fireRate = 0f;
        }
        private void CheckCurrentTargetPosition(Enemy enemy)
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

    public class Bullet
    {
        private Tower _tower;
        private Enemy _enemy;
        private Rectangle _gameBounds = new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        private Rectangle _bulletBounds;
        private Rectangle _enemyBounds;
        private float _speed = 750f;
        public Texture2D Texture { get; private set; }
        public Vector2 Pivot { get; private set; }
        public Vector2 Position { get; private set; }
        public float Rotation { get; private set; }
        public bool IsActive { get; private set; } = true;
        public float Damage { get; private set; }
        public Bullet(Texture2D texture, Tower tower, Enemy enemy, float damage)
        {
            Texture = texture;
            Pivot = new Vector2(Texture.Height / 2, Texture.Width / 2);
            _tower = tower;
            _enemy = enemy;
            Position = _tower.Position;
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
            Vector2 direction = (_enemy.Position + _enemy.Pivot * 2) - (_tower.Position + _tower.Pivot);
            direction.Normalize();
            Position += direction * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        private void CheckCollision()
        {
            _enemyBounds = new Rectangle((int)_enemy.Position.X, (int)_enemy.Position.Y, _enemy.Texture.Width, _enemy.Texture.Height);
            _bulletBounds = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
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

    public class GameManager
    {
        private List<TowerPlace> _places;
        private List<Tower> _towers;
        private Texture2D _sellTexture;
        private float _timer = 0f;

        public GameManager(List<TowerPlace> places, List<Tower> towers, Texture2D sellTexture)
        {
            _places = places;
            _towers = towers;
            _sellTexture = sellTexture;
            foreach (var place in _places)
            {
                place.CreateTower += OnTowerCreated;
            }
        }

        public void Update(GameTime gameTime, List<Enemy> enemies, Texture2D enemyTexture, List<Bullet> bullets)
        {
            CreateEnemies(gameTime, enemies, enemyTexture);
            bullets.RemoveAll(bullet => bullet.IsActive == false);
        }
        private void OnTowerSelled(Tower tower, TowerPlace place)
        {
            tower.SetActive(false);
            place.SetActive(true);
        }

        private void OnTowerCreated(TowerPlace towerPlace, Texture2D texture)
        {
            Tower tower = new Tower(towerPlace.Position, texture, towerPlace, _sellTexture);
            _towers.Add(tower);
            tower.SellTower += OnTowerSelled;
            towerPlace.SetActive(false);
        }

        private void CreateEnemies(GameTime gameTime, List<Enemy> enemies, Texture2D enemyTexture)
        {
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer <= 0)
            {
                enemies.Add(new Enemy(enemyTexture));
                _timer = 2f;
            }
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
