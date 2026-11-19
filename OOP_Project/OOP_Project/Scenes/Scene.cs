using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace OOP_Project.Scenes
{
    public abstract class Scene
    {
        protected Game1 _game;
        protected GraphicsDevice _graphicsDevice;
        protected ContentManager _contentManager;
        protected SpriteFont _font;

        public Scene(Game1 game)
        {
            _game = game;
            _graphicsDevice = game.GraphicsDevice;
        }

        public abstract void Initialize();

        public abstract void LoadContent();

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public virtual void UnloadContent() { }
    }
}
