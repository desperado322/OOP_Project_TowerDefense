using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace OOP_Project.Scenes
{
    public class PauseScene : Scene
    {
        private Texture2D _pixel;
        private SpriteFont _fontUI;
        private Rectangle _menuButton;
        private Rectangle _screenBounds;
        private string _menuButtonText = "В меню";
        private Vector2 _pivot;
        private float _transparency = 0.8f;
        public PauseScene(Game1 game) : base(game)
        {
            _menuButton = new Rectangle((int)(_game.ScreenWidth / 2 - 100), (int)(_game.ScreenHeight / 2 - 100), 100, 50);
            _screenBounds = new Rectangle(0, 0, _game.ScreenWidth, _game.ScreenHeight);
        }

        public override void Initialize()
        {
            _contentManager = new ContentManager(_game.Services, "Content");
        }

        public override void LoadContent()
        {
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _fontUI = _contentManager.Load<SpriteFont>("Fonts/UI Font");
            _pivot = _fontUI.MeasureString(_menuButtonText) / 2f;
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardExtended.Update();
            MouseExtended.Update();
            if (KeyboardExtended.GetState().WasKeyPressed(Keys.Escape))
            {
                _game.SceneManager.PopScene(this);
                return;
            }
           
            Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
            if (MouseExtended.GetState().WasButtonPressed(MouseButton.Left) && _menuButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new MenuScene(_game));
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, _screenBounds, Color.Black * _transparency);
            spriteBatch.Draw(_pixel, _menuButton, Color.White);
            spriteBatch.DrawString(_fontUI, _menuButtonText, new Vector2(_menuButton.X + _menuButton.Width / 2, _menuButton.Y + _menuButton.Height / 2), Color.Black, 0f, _pivot, 1f, SpriteEffects.None, 0f);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }
    }
}
