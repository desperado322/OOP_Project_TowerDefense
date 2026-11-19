using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace OOP_Project.Scenes
{
    public class GameOverScene : Scene
    {
        private int _screenWidth;
        private int _screenHeight;
        private Texture2D _pixel;
        private Rectangle _restartButton;
        private MouseClick _mouseClick;
        public GameOverScene(Game1 game) : base(game)
        {
            _game = game;
            _screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _restartButton = new Rectangle((int)(_screenWidth / 2 - 50), (int)(_screenHeight / 2 - 50), 100, 50);
            _mouseClick = new MouseClick();
        }

        public override void Initialize()
        {
            _contentManager = new ContentManager(_game.Services, "Content");
        }
        public override void LoadContent()
        {
            _font = _contentManager.Load<SpriteFont>("Font1");
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }
        public override void Update(GameTime gameTime)
        {
            _mouseClick.Update(gameTime);
            Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
            if (_mouseClick.IsClick && _restartButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new GamePlayScene(_game));
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "Game Over", new Vector2(_screenWidth / 2, 
                _screenHeight / 2), Color.White, 0f, 
                new Vector2(_font.Texture.Width / 2, _font.Texture.Height / 2), 2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(_pixel, _restartButton, Color.White);
        }
        public override void UnloadContent()
        {
            _contentManager.Unload();
            base.UnloadContent();
        }
    }
}
