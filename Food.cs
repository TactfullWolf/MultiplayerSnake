using Raylib_cs;
using System;
using System.Numerics;
using RLColor = Raylib_cs.Color;

public enum FoodType
{
    Normal,
    DoubleLength,
    SpeedBoost,
    SelfCollisionImmunity,
    EnemyShrink
}

public class Food
{
    public Vector2 Position;
    public FoodType Type;
    public RLColor BaseColor;
    public float PulseTime;
    public bool Consumed;

    public float DespawnTimer = -1f; // Only for special food

    public bool IsSpecial => Type != FoodType.Normal;

    public bool ShouldDespawn => IsSpecial && DespawnTimer <= 0f;


    public Food(Vector2 position, FoodType type)
    {
        Position = position;
        Type = type;
        PulseTime = 0f;
        Consumed = false;

        BaseColor = type switch
        {
            FoodType.Normal => RLColor.Green,
            FoodType.DoubleLength => RLColor.Yellow,
            FoodType.SpeedBoost => RLColor.Blue,
            FoodType.SelfCollisionImmunity => RLColor.Purple,
            FoodType.EnemyShrink => RLColor.Red,
            _ => RLColor.White
        };
    }

    public void Update(float deltaTime)
    {
        PulseTime += deltaTime;
        if (IsSpecial)
            DespawnTimer -= deltaTime;
    }

    public void Draw(float cellSize)
    {
        float pulse = 0.5f + 0.5f * MathF.Sin(PulseTime * 5f);
        byte alpha = (byte)(200 + pulse * 55); // Pulsing alpha

        RLColor pulseColor = new RLColor(BaseColor.R, BaseColor.G, BaseColor.B, alpha);

        Vector2 drawPos = Position * cellSize + new Vector2(cellSize / 2, cellSize / 2);
        float radius = cellSize * 0.4f;

        switch (Type)
        {
            case FoodType.Normal:
                Raylib.DrawCircleV(drawPos, radius, pulseColor);
                break;

            case FoodType.DoubleLength:
                Raylib.DrawCircleV(drawPos, radius + 2, new RLColor(BaseColor.R, BaseColor.G, BaseColor.B, (byte)(alpha / 2)));
                Raylib.DrawCircleV(drawPos, radius - 2, pulseColor);
                break;

            case FoodType.SpeedBoost:
                Raylib.DrawRectangleV(drawPos - new Vector2(radius / 2), new Vector2(radius, radius), pulseColor);
                break;

            case FoodType.SelfCollisionImmunity:
                Raylib.DrawPoly(drawPos, 6, radius, PulseTime * 60f % 360, pulseColor);
                break;

            case FoodType.EnemyShrink:
                Raylib.DrawCircleGradient(
                    (int)drawPos.X, (int)drawPos.Y,
                    radius + 1,
                    new RLColor(BaseColor.R, BaseColor.G, BaseColor.B, alpha),
                    new RLColor(0, 0, 0, 0)
                );
                break;
        }
    }
}
