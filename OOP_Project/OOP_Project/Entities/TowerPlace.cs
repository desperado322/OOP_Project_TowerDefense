using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace OOP_Project.Entities
{
    public class TowerPlace : Entity
    {
        private Rectangle _mousePosition;
        private List<TowerMenuElement> _menuElements = new List<TowerMenuElement>();
        public event Action<TowerPlace, Texture2D> CreateTower;
        public TowerPlace(Vector2 position, Texture2D texture, List<Texture2D> towerTextures, Dictionary<Texture2D, TowerStats> towerConfigs)
        {
            SetStats(position, texture);

            InitializeMenu(towerTextures, towerConfigs);

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
                foreach (var element in _menuElements)
                {
                    element.Update(gameTime, this);
                }
                if (MouseExtended.GetState().WasButtonPressed(MouseButton.Left))
                {
                    if (_mousePosition.Intersects(Bounds))
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
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Pivot, Scale, SpriteEffects.None, 0f);
                foreach (var element in _menuElements)
                {
                    element.Draw(spriteBatch, font);
                }
            }
        }

        private void InitializeMenu(List<Texture2D> towerTextures, Dictionary<Texture2D, TowerStats> towerConfigs)
        {
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(100, 0) * Scale, Pivot, towerTextures[0], Scale, ElementType.BasicTower, null, towerConfigs[towerTextures[0]].Cost));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(-100, 0) * Scale, Pivot, towerTextures[1], Scale, ElementType.CannonTower, null, towerConfigs[towerTextures[1]].Cost));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, 100) * Scale, Pivot, towerTextures[2], Scale, ElementType.MachineGunTower, null, towerConfigs[towerTextures[2]].Cost));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, -100) * Scale, Pivot, towerTextures[3], Scale, ElementType.ManyTargetsTower, null, towerConfigs[towerTextures[3]].Cost));
        }

        private void SetStats(Vector2 position, Texture2D texture)
        {
            IsActive = true;
            Scale = 0.5f;
            Position = position;
            Texture = texture;
            SetPivot();
            UpdateBounds();
        }

        private void CloseMenu()
        {
            foreach (var element in _menuElements)
            {
                element.SetActive(false);
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
}
