using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace OOP_Project.Entities
{
    public class TowerMenuElement : Entity
    {
        private ElementType _type;
        private float? _cost;
        public event Action<Texture2D> CreateTower;
        public event Action SellTowerEvent;
        public event Action UpgradeTowerEvent;
        public event Action RepairTowerEvent;
        public event Action ChangeStrategyTowerEvent;

        public TowerMenuElement(Vector2 position, Vector2 pivot, Texture2D menuElementTexture, float parentScale, ElementType type, Tower tower = null, float cost = 0f)
        {
            Position = position;
            Pivot = pivot;
            Texture = menuElementTexture;
            Scale = parentScale * 0.5f;
            UpdateBounds();
            _type = type;
            _cost = cost;
            SetCost(tower, _cost);
        }

        public void Update(GameTime gameTime, TowerPlace place = null, Tower tower = null)
        {
            if (IsActive)
            {
                SetCost(tower, _cost);
                Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
                if (MouseExtended.GetState().WasButtonPressed(MouseButton.Left))
                {
                    if (Bounds.Intersects(mousePosition))
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
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Pivot, Scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(font, _cost.ToString(), Position + new Vector2(0, 15), Color.Coral, 0f, font.MeasureString(_cost.ToString()) / 2f, 1f, SpriteEffects.None, 0f);
            }
        }

        private void SetCost(Tower tower, float? cost)
        {
            switch (_type)
            {
                case ElementType.Sell: _cost = tower.Stats.Cost / 2; break;
                case ElementType.Upgrade: _cost = tower.Stats.Cost / 2; break;
                case ElementType.Repair: _cost = tower.MaxHealth - tower.CurrentHealth; break;
                case ElementType.Strategy: _cost = null; break;
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
