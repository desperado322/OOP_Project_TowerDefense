using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OOP_Project.Scenes;
using System;
using Microsoft.Xna.Framework.Input;

namespace OOP_Project.Entities
{
    public class TowerMenuElement : Entity
    {
        private ElementType _type;
        private MouseClick _mouseClick;
        public event Action<Texture2D> CreateTower;
        public event Action SellTowerEvent;
        public event Action UpgradeTowerEvent;
        public event Action RepairTowerEvent;
        public event Action ChangeStrategyTowerEvent;

        public TowerMenuElement(Vector2 position, Vector2 pivot, Texture2D menuElementTexture, float parentScale, ElementType type)
        {
            Position = position;
            Pivot = pivot;
            Texture = menuElementTexture;
            Scale = parentScale * 0.5f;
            _mouseClick = new MouseClick();
            UpdateBounds();
            _type = type;
        }

        public void Update(GameTime gameTime, TowerPlace place = null, Tower tower = null)
        {
            if (IsActive)
            {
                _mouseClick.Update(gameTime);
                Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
                if (_mouseClick.IsClick && mousePosition.Intersects(Bounds))
                {
                    if (place != null && place.IsActive)
                    {
                        CreateTower?.Invoke(Texture);
                    }
                    else if (tower != null && tower.IsActive)
                    {
                        switch (_type)
                        {
                            case ElementType.Sell: SellTowerEvent?.Invoke(); break;
                            case ElementType.Upgrade: UpgradeTowerEvent?.Invoke(); break;
                            case ElementType.Repair: RepairTowerEvent?.Invoke(); break;
                            case ElementType.Strategy: ChangeStrategyTowerEvent?.Invoke(); break;
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Pivot, Scale, SpriteEffects.None, 0f);
            }
        }

        public void SetActive(bool state)
        {
            IsActive = state;
        }

        public void ChangeTexture(Texture2D texture)
        {
            Texture = texture;
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
}
