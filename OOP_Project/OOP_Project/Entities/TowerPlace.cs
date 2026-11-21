using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OOP_Project.Scenes;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace OOP_Project.Entities
{
    public class TowerPlace : Entity
    {
        private Rectangle _mousePosition;
        private MouseClick _mouseClick;
        private List<TowerMenuElement> _menuElements = new List<TowerMenuElement>();
        public event Action<TowerPlace, Texture2D> CreateTower;
        public TowerPlace(Vector2 position, Texture2D texture, List<Texture2D> towerTextures)
        {
            SetStats(position, texture);
            _mouseClick = new MouseClick();

            InitializeMenu(towerTextures);

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
                spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Pivot, Scale, SpriteEffects.None, 0f);
                foreach (var element in _menuElements)
                {
                    element.Draw(spriteBatch);
                }
            }
        }

        private void InitializeMenu(List<Texture2D> towerTextures)
        {
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(125, 0) * Scale, Pivot, towerTextures[0], Scale, ElementType.Sell));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(-125, 0) * Scale, Pivot, towerTextures[1], Scale, ElementType.Upgrade));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, 125) * Scale, Pivot, towerTextures[2], Scale, ElementType.Repair));
            _menuElements.Add(new TowerMenuElement(Position - new Vector2(0, -125) * Scale, Pivot, towerTextures[3], Scale, ElementType.Strategy));
        }

        private void SetStats(Vector2 position, Texture2D texture)
        {
            IsActive = true;
            Scale = 0.5f;
            Position = position;
            Texture = texture;
            Pivot = new Vector2(Texture.Width / 2, Texture.Height / 2);
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
