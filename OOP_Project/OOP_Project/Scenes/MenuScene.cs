using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;

namespace OOP_Project.Scenes
{
    public class MenuScene : Scene
    {
        private List<LevelButton> _levelButtons;
        private Texture2D _pixel;
        private Rectangle _startButton;
        private Vector2 _pivot;
        private string _startButtonText = "Старт";
        private SpriteFont _fontUI;
        public MenuScene(Game1 game) : base(game) 
        { 
            _startButton = new Rectangle((int)(_game.ScreenWidth / 2 - 100), (int)(_game.ScreenHeight / 2 - 100), 100, 50);
        }

        public override void Initialize()
        {
            _contentManager = new ContentManager(_game.Services, "Content"); 
            _levelButtons = new List<LevelButton>();
        }

        public override void LoadContent()
        {
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _fontUI = _contentManager.Load<SpriteFont>("Fonts/UI Font");
            _pivot = _fontUI.MeasureString(_startButtonText) / 2f;

            InitializeLevelButtons();
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                _game.Exit();

            MouseExtended.Update();
            Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
            if (MouseExtended.GetState().WasButtonPressed(MouseButton.Left) && _startButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new GamePlayScene(_game, _game.LevelSettings.GetTowerPositions(), _game.LevelSettings.GetSplinePoints(), _game.LevelSettings.GetRoadType(), _game.LevelSettings.GetBasePosition()));
            }

            foreach (var levelButton in _levelButtons)
            {
                levelButton.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawStartButton(spriteBatch);

            foreach (var levelButton in _levelButtons)
            {
                levelButton.Draw(spriteBatch, _pixel);
            }
        }

        public override void UnloadContent()
        {
            foreach(var levelButton in _levelButtons)
            {
                levelButton.ChangeLevel -= OnLevelChanged;
            }
            _contentManager.Unload();
            base.UnloadContent();
        }

        private void DrawStartButton(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, _startButton, Color.White);
            spriteBatch.DrawString(_fontUI, _startButtonText, new Vector2(_startButton.X + _startButton.Width / 2, _startButton.Y + _startButton.Height / 2), Color.Black, 0f, _pivot, 1f, SpriteEffects.None, 0f);
        }

        private void InitializeLevelButtons()
        {
            _levelButtons.Add(new LevelButton(new(500, 300), LevelType.FirstLevel, _fontUI));
            _levelButtons.Add(new LevelButton(new(500, 450), LevelType.SecondLevel, _fontUI));
            _levelButtons.Add(new LevelButton(new(500, 600), LevelType.ThirdLevel, _fontUI));
            _levelButtons.Add(new LevelButton(new(500, 750), LevelType.FourthLevel, _fontUI));
            _levelButtons.Add(new LevelButton(new(500, 900), LevelType.FifthLevel, _fontUI));

            foreach (var levelButton in _levelButtons)
            {
                levelButton.ChangeLevel += OnLevelChanged;
            }
        }

        private void OnLevelChanged(LevelType type)
        {
            _game.LevelSettings.ChangeLevel((int)type);
        }
    }

    public class LevelButton
    {
        private Vector2 _position;
        private LevelType _type;
        private Rectangle _bounds;
        private SpriteFont _font;
        private Color _color;

        public event Action<LevelType> ChangeLevel;

        public LevelButton(Vector2 position, LevelType type, SpriteFont font)
        {
            _color = Color.White;
            _position = position;
            _type = type;
            _font = font;
            _bounds = new Rectangle((int)(_position.X), (int)(_position.Y), 100, 50);
        }

        public void Update(GameTime gameTime)
        {
            Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);

            if (MouseExtended.GetState().WasButtonPressed(MouseButton.Left))
            {
                if (_bounds.Contains(mousePosition))
                {
                    _color = Color.Yellow;
                    ChangeLevel?.Invoke(_type);
                }
                else
                {
                    _color = Color.White;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            spriteBatch.Draw(pixel, _bounds, _color);
            spriteBatch.DrawString(_font, $"{(int)_type + 1}", _position, Color.Black);
        }
    }
    public enum LevelType
    {
        FirstLevel,
        SecondLevel,
        ThirdLevel,
        FourthLevel,
        FifthLevel
    }
}
