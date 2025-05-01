using System.Numerics;
using Raylib_cs;

namespace MultiplayerSnake
{
    public enum SpecialFoodType
    {
        GhostMode,
        SpeedBoost,
        DoubleLength,
        ShrinkEnemies
    }

    public class SpecialFood
    {
        public Vector2 Position { get; private set; }
        public SpecialFoodType Type { get; private set; }
        public Color BaseColor { get; private set; }
        public float Radius { get; private set; }
        public int Sides { get; private set; }
        public float Scale { get; set; }
        public float scaleTimer;

        public SpecialFood(Vector2 position, SpecialFoodType type)
        {
            Position = position;
            Type = type;
            Radius = 10f;
            Scale = 1.0f;
            scaleTimer = 0f;

            switch (type)
            {
                case SpecialFoodType.GhostMode:
                    BaseColor = new Color(128, 0, 128, 255);
                    Sides = 5;
                    break;
                case SpecialFoodType.SpeedBoost:
                    BaseColor = new Color(255, 255, 0, 255);
                    Sides = 0;
                    break;
                case SpecialFoodType.DoubleLength:
                    BaseColor = new Color(0, 255, 0, 255);
                    Sides = 0;
                    break;
                case SpecialFoodType.ShrinkEnemies:
                    BaseColor = new Color(255, 0, 0, 255);
                    Sides = 0;
                    break;
            }
        }

        public void Update(float deltaTime)
        {
            scaleTimer += deltaTime * 2f;
            if (Type == SpecialFoodType.ShrinkEnemies || Type == SpecialFoodType.GhostMode)
            {
                Scale = 0.95f + 0.1f * (float)System.Math.Sin(scaleTimer);
            }
        }
    }
}