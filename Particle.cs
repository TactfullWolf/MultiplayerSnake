using Raylib_cs;
using System.Numerics;

namespace MultiplayerSnake
{
    public class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }
        public Color Color { get; set; }

        public Particle(Vector2 position, Vector2 velocity, float lifetime, Color color)
        {
            Position = position;
            Velocity = velocity;
            Lifetime = lifetime;
            MaxLifetime = lifetime;
            Color = color;
        }

        public void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
            Lifetime -= deltaTime;
        }

        public bool IsDead => Lifetime <= 0;
    }
}