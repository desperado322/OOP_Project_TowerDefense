using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input;


namespace OOP_Project.Scenes
{
    public class GameOverScene : Scene
    {
        private Texture2D _pixel;
        private Rectangle _restartButton;
        private Rectangle _menuButton;
        private Rectangle _screenBounds;
        private Vector2 _pivot;
        private float _transparency = 0.8f;
        private string _restartButtonText = "Рестарт";
        private string _menuButtonText = "В меню";
        private string _gameOverText;
        public GameOverScene(Game1 game, string text) : base(game)
        {
            _gameOverText = text;
            _restartButton = new Rectangle((int)(_game.ScreenWidth / 2 - 100), (int)(_game.ScreenHeight / 2 - 100), 100, 50);
            _menuButton = new Rectangle((int)(_game.ScreenWidth / 2 - 100), (int)(_game.ScreenHeight / 2), 100, 50);
            _screenBounds = new Rectangle(0, 0, _game.ScreenWidth, _game.ScreenHeight);
        }

        public override void Initialize()
        {
            _contentManager = new ContentManager(_game.Services, "Content");
        }
        public override void LoadContent()
        {
            _font = _contentManager.Load<SpriteFont>("UI Font");
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _pivot = _font.MeasureString(_restartButtonText) / 2f;
        }
        public override void Update(GameTime gameTime)
        {
            MouseExtended.Update();
            Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
            if (MouseExtended.GetState().WasButtonPressed(MouseButton.Left) && _restartButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new GamePlayScene(_game, _game.LevelSettings.GetTowerPositions(), _game.LevelSettings.GetSplinePoints(), _game.LevelSettings.GetRoadType()));
            }
            else if (MouseExtended.GetState().WasButtonPressed(MouseButton.Left) && _menuButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new MenuScene(_game));
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, _screenBounds, Color.Black * _transparency);
            spriteBatch.DrawString(_font, _gameOverText, new (_game.ScreenWidth / 2 - 150, _game.ScreenHeight / 2 - 150), Color.Crimson);
            DrawRestartButton(spriteBatch);
            DrawMenuButton(spriteBatch);
        }
        public override void UnloadContent()
        {
            _contentManager.Unload();
            base.UnloadContent();
        }

        private void DrawRestartButton(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, _restartButton, Color.White);
            spriteBatch.DrawString(_font, _restartButtonText, new Vector2(_restartButton.X + _restartButton.Width / 2, _restartButton.Y + _restartButton.Height / 2), Color.Black, 0f, _pivot, 1f, SpriteEffects.None, 0f);
        }
        private void DrawMenuButton(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, _menuButton, Color.White);
            spriteBatch.DrawString(_font, _menuButtonText, new Vector2(_menuButton.X + _menuButton.Width / 2, _menuButton.Y + _menuButton.Height / 2), Color.Black, 0f, _pivot, 1f, SpriteEffects.None, 0f);
        }
    }
}
