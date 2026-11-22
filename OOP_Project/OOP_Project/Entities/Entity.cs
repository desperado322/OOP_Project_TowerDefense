using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace OOP_Project.Entities
{
    public abstract class Entity
    {
        public Texture2D Texture { get; protected set; }
        public Vector2 Position { get; protected set; }
        public float Scale { get; protected set; }
        public float Rotation { get; protected set; }
        public Vector2 Pivot { get; protected set; }
        public bool IsActive { get; protected set; }
        public Rectangle Bounds { get; protected set; }

        protected virtual void UpdateBounds()
        {
            Bounds = new Rectangle(
                (int)(Position.X - Pivot.X * Scale),
                (int)(Position.Y - Pivot.Y * Scale),
                (int)(Texture.Width * Scale),
                (int)(Texture.Height * Scale)
            );
        }
    }
}
