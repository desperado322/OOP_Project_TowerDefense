using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using OOP_Project.Scenes;
using System.Linq;

namespace OOP_Project
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public int ScreenWidth { get; private set; } = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        public int ScreenHeight { get; private set; } = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        public SpriteFont Font { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public SceneManager SceneManager { get; private set; }
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.ApplyChanges();
            Window.Title = "Tower Defense";
        }

        protected override void Initialize()
        {
            SceneManager = new SceneManager();
            SceneManager.PushScene(new MenuScene(this));
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Font1");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                GameOver();

            SceneManager.Update(gameTime);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();
            SceneManager.Draw(gameTime, SpriteBatch);
            SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void GameOver()
        {
            Exit();
        }
        
    }

    public class SceneManager
    {
        private Stack<Scene> _stack = new Stack<Scene>();
        public Scene _currentScene => _stack.Count > 0 ? _stack.Peek() : null;

        public void PushScene(Scene scene)
        {
            _stack.Push(scene);
            scene.Initialize();
            scene.LoadContent();
        }

        public void PopScene(Scene scene)
        {
            if (_stack.Count > 0)
            {
                var oldScene = _stack.Pop();
                oldScene.UnloadContent();
            }
        }

        public void ChangeScene(Scene scene)
        {
            while (_stack.Count > 0)
            {
                _stack.Pop();
            }
            PushScene(scene);
        }

        public void Update(GameTime gameTime)
        {
            _stack.Peek().Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var scene in _stack.Reverse())
            {
                scene.Draw(gameTime, spriteBatch);
            }
        }
    }

    public class MouseClick
    {
        private MouseState _previousState;
        public bool IsClick { get; private set; } = false;

        public MouseClick()
        {
            _previousState = Mouse.GetState();
        }

        public void Update(GameTime gameTime)
        {
            MouseState currentState = Mouse.GetState();
            IsClick = currentState.LeftButton == ButtonState.Pressed && _previousState.LeftButton == ButtonState.Released;
            _previousState = currentState;
        }
    }
}
