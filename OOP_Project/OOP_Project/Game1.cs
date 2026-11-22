using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using OOP_Project.Scenes;
using System.Linq;
using System;

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
        public LevelSettings LevelSettings { get; private set; }
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
            LevelSettings = new LevelSettings();
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

    public class LevelSettings
    {
        private List<List<Vector2>> _towerPositions;

        private List<List<Vector2>> _splinePoints;

        public int CurrentLevel { get; private set; }

        public LevelSettings()
        {
            FillTowerPositions();
            FillSplinePoints();
            CurrentLevel = 0;
        }

        private void FillTowerPositions()
        {
            _towerPositions = new () {
                new () { new(250, 350), new(250, 750), new(650, 350), new (650, 700), new (1250, 800), new (1750, 600), new (1500, 250)},
                new () { new(400, 200), new(400, 550), new(750, 250), new (1050, 250), new (1300, 500), new (1600, 800)},
                new () { new(200, 800), new(650, 300), new(550, 750), new (1050, 500), new (1500, 500), new (1800, 200)},
                new () { new(250, 400), new(250, 700), new(700, 400), new (1150, 800), new (1500, 800), new (1800, 400)},
                new () { new(350, 900), new(350, 400), new(700, 400), new (1200, 400), new (1500, 600), new (1500, 900)}
            };
        }

        private void FillSplinePoints()
        {
            _splinePoints = new() {
                new () {
                    new (100, 0), new (100, 0), new (100, 900),
                    new (100, 900), new (400, 900), new (400, 900),
                    new (400, 200),  new (400, 200), new (900, 200),
                    new (900, 200), new (900, 900), new (900, 900),
                    new (1600, 900), new (1600, 900), new (1600, 0), new (1600, 0)
                },
                new () {
                    new (200, 0), new (200, 0), new (200, 700),
                    new (200, 700), new (600, 700), new (600, 700),
                    new (600, 100),  new (600, 100), new (1200, 100),
                    new (1200, 100), new (1200, 900), new (1200, 900),
                    new (1900, 900), new (1900, 900)
                },
                new () {
                    new (0, 900), new (0, 900), new (400, 900),
                    new (400, 900), new (400, 200), new (400, 200),
                    new (900, 200),  new (900, 200), new (900, 600),
                    new (900, 600), new (1700, 600), new (1700, 600),
                    new (1700, 0), new (1700, 0)
                },
                new () {
                    new (1920, 300), new (1920, 300), new (1600, 300),
                    new (1600, 300), new (1600, 900), new (1600, 900),
                    new (1000, 900),  new (1000, 900), new (1000, 300),
                    new (1000, 300), new (400, 300), new (400, 300),
                    new (400, 900), new (400, 900), new (100, 900),
                    new (100, 900), new (100, 0), new (100, 0)
                },
                new () {
                    new (200, 1100), new (200, 1100), new (200, 300),
                    new (200, 300), new (1600, 300), new (1600, 300),
                    new (1600, 1100),  new (1600, 1100)
                }
            };
        }

        public List<Vector2> GetTowerPositions()
        {
            return _towerPositions[CurrentLevel];
        }

        public List<Vector2> GetSplinePoints()
        {   
            return _splinePoints[CurrentLevel];
        }

        public RoadType GetRoadType()
        {
            return Enum.GetValues<RoadType>()[CurrentLevel];
        }

        public void ChangeLevel(int level)
        {
            if (CurrentLevel < _towerPositions.Count)
            {
                CurrentLevel = level;
            }
        }
    }

    public enum RoadType
    {
        Road1,
        Road2,
        Road3,
        Road4,
        Road5
    }
}
