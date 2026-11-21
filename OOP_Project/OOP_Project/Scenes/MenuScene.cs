using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace OOP_Project.Scenes
{
    public class MenuScene : Scene
    {
        private Texture2D _pixel;
        private MouseClick _mouseClick;
        private Rectangle _startButton;
        private Vector2 _pivot;
        private string _startButtonText = "Старт";
        public MenuScene(Game1 game) : base(game) 
        { 
            _game = game;
            _mouseClick = new MouseClick();
            _startButton = new Rectangle((int)(game.ScreenWidth / 2 - 100), (int)(game.ScreenHeight / 2 - 100), 100, 50);
        }

        public override void Initialize()
        {
            _contentManager = new ContentManager(_game.Services, "Content"); 
        }

        public override void LoadContent()
        {
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _font = _contentManager.Load<SpriteFont>("UI Font");
            _pivot = _font.MeasureString(_startButtonText) / 2f;
        }

        public override void Update(GameTime gameTime)
        {
            _mouseClick.Update(gameTime);
            Rectangle mousePosition = new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 2, 2);
            if (_mouseClick.IsClick && _startButton.Contains(mousePosition))
            {
                _game.SceneManager.ChangeScene(new GamePlayScene(_game));
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawStartButton(spriteBatch);
        }

        public override void UnloadContent()
        {
            _contentManager.Unload();
            base.UnloadContent();
        }

        private void DrawStartButton(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, _startButton, Color.White);
            spriteBatch.DrawString(_font, _startButtonText, new Vector2(_startButton.X + _startButton.Width / 2, _startButton.Y + _startButton.Height / 2), Color.Black, 0f, _pivot, 1f, SpriteEffects.None, 0f);
        }
    }
}
