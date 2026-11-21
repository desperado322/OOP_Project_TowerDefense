using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace OOP_Project.Scenes
{
    public class GameOverScene : Scene
    {
        private Texture2D _pixel;
        private Rectangle _restartButton;
        private Rectangle _menuButton;
        private MouseClick _mouseClick;
        private Vector2 _pivot;
        private string _restartButtonText = "Рестарт";
        private string _menuButtonText = "В меню";
        private string _gameOver = "Игра окончена";
        public GameOverScene(Game1 game) : base(game)
        {
            _game = game;
            _restartButton = new Rectangle((int)(game.ScreenWidth / 2 - 100), (int)(game.ScreenHeight / 2 - 100), 100, 50);
            _menuButton = new Rectangle((int)(game.ScreenWidth / 2 - 100), (int)(game.ScreenHeight / 2), 100, 50);
            _mouseClick = new MouseClick();
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
            _mouseClick.Update(gameTime);
            Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
            if (_mouseClick.IsClick && _restartButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new GamePlayScene(_game));
            }
            else if (_mouseClick.IsClick && _menuButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new MenuScene(_game));
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
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
