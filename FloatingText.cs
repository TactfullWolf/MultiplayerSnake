using Raylib_cs;
using System.Numerics;

namespace MultiplayerSnake
{
    public class FloatingText
    {
        public string Text { get; private set; }
        public Vector2 Position { get; set; }
        public Color Color { get; private set; }
        public float Lifetime { get; private set; }
        private float maxLifetime;

        public FloatingText(string text, Vector2 position, Color color)
        {
            Text = text;
            Position = position;
            Color = color;
            Lifetime = 2.0f; // Display for 2 seconds
            maxLifetime = Lifetime;
        }

        public void Update(float deltaTime)
        {
            // Move upward and fade out
            Position -= new Vector2(0, 30 * deltaTime); // Move up
            Lifetime -= deltaTime;
        }

        public bool IsDead => Lifetime <= 0;
    }
}