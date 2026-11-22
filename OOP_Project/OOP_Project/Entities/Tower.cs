using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OOP_Project.Scenes;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using OOP_Project.Strategies;

namespace OOP_Project.Entities
{
    public abstract class Tower : Entity
    {
        protected TowerPlace _place;
        protected MouseClick _mouseClick;
        protected BulletPool _bullets;
        protected IStrategy _strategy;
        protected List<IStrategy> _strategies;
        protected List<TowerMenuElement> _menuElements = new List<TowerMenuElement>();
        protected SpriteFont _font;
        protected float _maxHealth;
        protected float _rangeShooting;
        protected float _attackCooldown;
        protected float _fireRate;
        protected float _bulletDamage;
        protected float _bulletSpeed;
        protected float _health;
        protected int _countOfUpgrades = 0;
        protected Enemy _currentTarget;
        protected bool _statsActive = false;
        public TowerStats Stats { get; private set; }
        public event Action<Tower, TowerPlace> SellTower;
        public Tower(Vector2 position, TowerStats stats, TowerPlace towerPlace, List<Texture2D> menuElementsTextures, List<IStrategy> strategies)
        {
            Position = position;
            _place = towerPlace;
            _strategies = strategies;
            _strategy = _strategies[0];
            SetStats(stats);
            _bullets = new BulletPool(Stats.BulletTexture, this, _bulletSpeed);
            _mouseClick = new MouseClick();

            InitializeMenu(menuElementsTextures);

            foreach (var element in _menuElements)
            {
                SubscribeEvents(element);
            }
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
                    if (mousePosition.Intersects(Bounds))
                    {
                        _statsActive = true;
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
                spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, Pivot, Scale, SpriteEffects.None, 0f);
            }
        }

        public virtual void DrawUI(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (IsActive)
            {
                foreach (var element in _menuElements)
                {
                    element.Draw(spriteBatch);
                }
                if (_statsActive)
                {
                    spriteBatch.DrawString(_font, $"Дальность {_rangeShooting} \n" +
                        $"Урон: {_bulletDamage} \n" +
                        $"Перезарядка: {_attackCooldown} \n" +
                        $"Здоровье: {_health} \n" +
                        $"Максимальное здоровье: {_maxHealth} \n" +
                        $"Скорость пули: {_bulletSpeed} \n" +
                        $"Количество улучшений: {_countOfUpgrades} (max 5) \n" +
                        $"Текущий тип атаки: {_strategy.Name}", Position + new Vector2(150, -50) * Scale, Color.White);
                }
                if (_health < _maxHealth)
                {
                    spriteBatch.Draw(pixel, new Rectangle((int)(Position.X - _maxHealth / 4), (int)Position.Y + 25, (int)(_maxHealth / 2), 10), Color.Black);
                    spriteBatch.Draw(pixel, new Rectangle((int)(Position.X - _maxHealth / 4), (int)Position.Y + 25, (int)(_health / 2), 10), Color.LightGreen);
                }
            }
        }
        protected void InitializeMenu(List<Texture2D> menuElementsTextures)
        {
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(125, 0) * Scale, Pivot, menuElementsTextures[0], Scale, ElementType.Sell));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(-125, 0) * Scale, Pivot, menuElementsTextures[1], Scale, ElementType.Upgrade));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, 125) * Scale, Pivot, menuElementsTextures[2], Scale, ElementType.Repair));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, -125) * Scale, Pivot, menuElementsTextures[3], Scale, ElementType.Strategy));
        }
        protected void SetStats(TowerStats stats)
        {
            Stats = stats;
            _rangeShooting = Stats.Range;
            _fireRate = Stats.FireRate;
            _bulletDamage = Stats.BulletDamage;
            _bulletSpeed = Stats.BulletSpeed;
            _attackCooldown = _fireRate;
            _font = Stats.Font;
            Texture = Stats.Texture;
            _maxHealth = Stats.MaxHealth;
            _health = _maxHealth;
            Scale = 0.5f;
            IsActive = true;
            Pivot = new Vector2(Texture.Width / 2, Texture.Height / 2);
            UpdateBounds();
        }
        protected void OnTowerSelled()
        {
            _statsActive = false;
            SellTower?.Invoke(this, _place);
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
                UnSubscribeEvents(element);
            }
        }
        protected void OnTowerRepaired()
        {
            _health = _maxHealth;
            _statsActive = false;
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
            }
        }

        protected void OnTowerUpgraded()
        {
            if (_countOfUpgrades < 5)
            {
                _rangeShooting = MathF.Round(_rangeShooting * 1.1f);
                _attackCooldown = MathF.Round(_attackCooldown / 1.1f, 2);
                _bulletDamage = MathF.Round(_bulletDamage * 1.1f);
                _maxHealth += 10f;
                _statsActive = false;
                foreach (var element in _menuElements)
                {
                    element.SetActive(false);
                }
                _countOfUpgrades += 1;
                _fireRate = _attackCooldown;
            }
        }
        protected void OnTowerStrategyChanged()
        {
            ChangeStrategy();
            _statsActive = false;
        }
        protected void CloseMenu()
        {
            _statsActive = false;
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

        protected void Aim(Enemy enemy)
        {
            Vector2 direction = enemy.Position - Position;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        protected void Shoot(Enemy enemy)
        {
            _bullets.GetBullet(enemy, _bulletDamage);
        }
        protected void CheckCurrentTargetPosition(Enemy enemy)
        {
            if (Vector2.DistanceSquared(Position, enemy.Position) > _rangeShooting * _rangeShooting)
            {
                _currentTarget = null;
            }
        }
        protected void ChangeStrategy()
        {
            if (_strategy != _strategies[^2])
            {
                _strategy = _strategies[_strategies.IndexOf(_strategy) + 1];
            }
            else
            {
                _strategy = _strategies[0];
            }
        }

        protected virtual void SubscribeEvents(TowerMenuElement element)
        {
            element.SellTowerEvent += OnTowerSelled;
            element.UpgradeTowerEvent += OnTowerUpgraded;
            element.RepairTowerEvent += OnTowerRepaired;
            element.ChangeStrategyTowerEvent += OnTowerStrategyChanged;
        }
        protected virtual void UnSubscribeEvents(TowerMenuElement element)
        {
            element.UpgradeTowerEvent -= OnTowerUpgraded;
            element.RepairTowerEvent -= OnTowerRepaired;
            element.ChangeStrategyTowerEvent -= OnTowerStrategyChanged;
        }
        public void SetActive(bool state)
        {
            IsActive = state;
        }

        public virtual void GetDamage(float damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                IsActive = false;
                _place.SetActive(true);
            }
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
}
