using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

namespace MultiplayerSnake
{
    public class Snake
    {
        public List<Vector2> Segments { get; private set; }
        public Vector2 Direction { get; set; }
        public int GrowthRemaining { get; set; }
        public bool IsGhostMode { get; set; }
        public bool IsSpeedBoosted { get; set; }
        public Color BaseColor { get; private set; }
        public SnakeEffectManager EffectManager { get; private set; }
        public int TeamId { get; private set; }
        private float ghostAlphaTimer;
        public List<Particle> Particles { get; private set; }
        public int Score { get; private set; }
        public int FoodEaten { get; private set; }
        public int SpecialFoodEaten { get; private set; }

        public Snake(Vector2 startPos, int initialLength, int gridSize, Color color, int teamId)
        {
            Segments = new List<Vector2>();
            Direction = new Vector2(1, 0);
            BaseColor = color;
            EffectManager = new SnakeEffectManager(this);
            TeamId = teamId;
            ghostAlphaTimer = 0f;
            Particles = new List<Particle>();
            Score = 0;
            FoodEaten = 0;
            SpecialFoodEaten = 0;

            for (int i = 0; i < initialLength; i++)
            {
                Segments.Add(startPos - Direction * i * gridSize);
            }
        }

        public void Update(float deltaTime)
        {
            EffectManager.Update(deltaTime);
            if (IsGhostMode)
            {
                ghostAlphaTimer += deltaTime * 4f;
            }

            if (IsSpeedBoosted)
            {
                Random rand = new Random();
                int sparkSegmentCount = (Segments.Count * 2) / 3;
                for (int i = 0; i < sparkSegmentCount; i++)
                {
                    Vector2 pos = Segments[i];
                    Vector2 perp = new Vector2(-Direction.Y, Direction.X);
                    Vector2 edge1 = perp * 10f;
                    Vector2 edge2 = -perp * 10f;
                    Vector2 edge3 = perp * 5f + Direction * 5f;
                    Vector2 edge4 = -perp * 5f + Direction * 5f;

                    foreach (var edgePos in new[] { pos + edge1, pos + edge2, pos + edge3, pos + edge4 })
                    {
                        Vector2 velocity = (edgePos - pos).Normalized() * 30f +
                            new Vector2(
                                (float)(rand.NextDouble() - 0.5) * 20f,
                                (float)(rand.NextDouble() - 0.5) * 20f
                            );
                        Particles.Add(new Particle(edgePos, velocity, 0.5f, new Color((byte)255, (byte)255, (byte)0, (byte)255)));
                        Console.WriteLine($"Emitting spark at {edgePos} for snake at {pos}");
                    }
                }
            }

            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                Particles[i].Update(deltaTime);
                if (Particles[i].IsDead)
                {
                    Particles.RemoveAt(i);
                }
            }
        }

        public byte GetGhostAlpha()
        {
            if (!IsGhostMode) return 255;
            float alpha = 0.3f + 0.5f * (float)System.Math.Sin(ghostAlphaTimer);
            return (byte)(alpha * 255);
        }

        public void Move(int gridSize)
        {
            Vector2 newHead = Segments[0] + Direction * gridSize;
            Segments.Insert(0, newHead);

            if (GrowthRemaining > 0)
            {
                GrowthRemaining--;
            }
            else
            {
                Segments.RemoveAt(Segments.Count - 1);
            }
        }

        public bool EatFood(Vector2 foodPosition)
        {
            Vector2 head = Segments[0];
            if (System.Math.Abs(head.X - foodPosition.X) <= 10f &&
                System.Math.Abs(head.Y - foodPosition.Y) <= 10f)
            {
                Console.WriteLine($"Ate food at {foodPosition} with head at {head}");
                GrowthRemaining++;
                Score += 1;
                FoodEaten++;
                return true;
            }
            return false;
        }

        public bool CheckCollisions(int screenWidth, int screenHeight, ObstacleManager obstacleManager)
        {
            Vector2 head = Segments[0];

            if (!IsGhostMode)
            {
                for (int i = 1; i < Segments.Count; i++)
                {
                    if (head == Segments[i])
                    {
                        return true;
                    }
                }
            }

            if (head.X < 40 || head.X >= screenWidth || head.Y < 90 || head.Y >= screenHeight)
            {
                return true;
            }

            foreach (var obstacle in obstacleManager.Obstacles)
            {
                bool collision = false;
                if (obstacle.Type == ObstacleType.PlusSign)
                {
                    Rectangle hRect = new Rectangle(obstacle.Position.X - obstacle.Width / 2, obstacle.Position.Y - 10, obstacle.Width, 20);
                    Rectangle vRect = new Rectangle(obstacle.Position.X - 10, obstacle.Position.Y - obstacle.Height / 2, 20, obstacle.Height);
                    collision = CheckRectangleCollision(head, hRect) || CheckRectangleCollision(head, vRect);
                }
                else if (obstacle.Type == ObstacleType.Box)
                {
                    Rectangle rect = new Rectangle(obstacle.Position.X - obstacle.Width / 2, obstacle.Position.Y - obstacle.Height / 2, obstacle.Width, obstacle.Height);
                    collision = CheckBoxCollision(head, rect);
                }
                else if (obstacle.Type == ObstacleType.Triangle)
                {
                    Vector2[] points = GetTrianglePoints(obstacle.Position, obstacle.Width / 2);
                    collision = CheckTriangleCollision(head, points);
                }
                else if (obstacle.Type == ObstacleType.DashedLine)
                {
                    if (obstacle.IsHorizontal)
                    {
                        float startX = 40;
                        float endX = 760;
                        float y = obstacle.Position.Y;
                        for (float x = startX; x < endX; x += 15)
                        {
                            float segmentLength = System.Math.Min(10, endX - x);
                            Rectangle rect = new Rectangle(x, y - 5, segmentLength, 10);
                            if (CheckRectangleCollision(head, rect))
                            {
                                collision = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        float startY = 90;
                        float endY = 580;
                        float x = obstacle.Position.X;
                        for (float y = startY; y < endY; y += 15)
                        {
                            float segmentLength = System.Math.Min(10, endY - y);
                            Rectangle rect = new Rectangle(x - 5, y, 10, segmentLength);
                            if (CheckRectangleCollision(head, rect))
                            {
                                collision = true;
                                break;
                            }
                        }
                    }
                }

                if (collision)
                {
                    Console.WriteLine($"Snake collided with obstacle at {obstacle.Position} (Type: {obstacle.Type})");
                    return true;
                }
            }

            return false;
        }

        public void CheckSpecialFoodCollision(SpecialFoodManager specialFoodManager, List<FloatingText> floatingTexts)
        {
            List<SpecialFood> foodsToRemove = new List<SpecialFood>();
            Vector2 head = Segments[0];

            foreach (var specialFood in specialFoodManager.SpecialFoods)
            {
                if (System.Math.Abs(head.X - specialFood.Position.X) <= 10f &&
                    System.Math.Abs(head.Y - specialFood.Position.Y) <= 10f)
                {
                    Vector2 textPos = new Vector2(
                        System.Math.Clamp(head.X, 40, 760 - 80),
                        System.Math.Clamp(head.Y - 20, 90, 560 - 20)
                    );

                    switch (specialFood.Type)
                    {
                        case SpecialFoodType.GhostMode:
                            EffectManager.ApplyEffect(SnakeEffectType.GhostMode, 5.0f);
                            floatingTexts.Add(new FloatingText("Ghost Mode!", textPos, new Color((byte)128, (byte)0, (byte)128, (byte)255)));
                            break;

                        case SpecialFoodType.SpeedBoost:
                            EffectManager.ApplyEffect(SnakeEffectType.SpeedBoost, 5.0f);
                            floatingTexts.Add(new FloatingText("Speed Boost!", textPos, new Color((byte)255, (byte)255, (byte)0, (byte)255)));
                            break;

                        case SpecialFoodType.DoubleLength:
                            GrowthRemaining += 2;
                            floatingTexts.Add(new FloatingText("Double Length!", textPos, new Color((byte)0, (byte)255, (byte)0, (byte)255)));
                            break;

                        case SpecialFoodType.ShrinkEnemies:
                            EffectManager.ApplyEffect(SnakeEffectType.ShrinkEnemies, 5.0f);
                            floatingTexts.Add(new FloatingText("Shrink Enemies!", textPos, new Color((byte)255, (byte)0, (byte)0, (byte)255)));
                            break;
                    }

                    SpecialFoodEaten++;
                    foodsToRemove.Add(specialFood);
                    Console.WriteLine($"Collected {specialFood.Type} at {specialFood.Position} with head at {head}, text at {textPos}, no score change");
                }
            }

            foreach (var food in foodsToRemove)
            {
                specialFoodManager.Remove(food);
            }
        }

        public void Shrink(int amount)
        {
            if (Segments.Count > 1)
            {
                int segmentsToRemove = System.Math.Min(amount, Segments.Count - 1);
                Segments.RemoveRange(Segments.Count - segmentsToRemove, segmentsToRemove);
            }

            Vector2 textPos = new Vector2(
                System.Math.Clamp(Segments[0].X, 40, 760 - 80),
                System.Math.Clamp(Segments[0].Y - 20, 90, 560 - 20)
            );
            Console.WriteLine($"Shrunk snake, text at {textPos}");
        }

        private bool CheckRectangleCollision(Vector2 point, Rectangle rect)
        {
            return point.X >= rect.X && point.X <= rect.X + rect.Width &&
                   point.Y >= rect.Y && point.Y <= rect.Y + rect.Height;
        }

        private bool CheckBoxCollision(Vector2 point, Rectangle rect)
        {
            float left = rect.X;
            float right = rect.X + rect.Width;
            float top = rect.Y;
            float bottom = rect.Y + rect.Height;
            float thickness = 2f;

            return (point.X >= left && point.X <= right && (System.Math.Abs(point.Y - top) <= thickness || System.Math.Abs(point.Y - bottom) <= thickness)) ||
                   (point.Y >= top && point.Y <= bottom && (System.Math.Abs(point.X - left) <= thickness || System.Math.Abs(point.X - right) <= thickness));
        }

        private bool CheckTriangleCollision(Vector2 point, Vector2[] points)
        {
            float area = 0.5f * (-points[1].Y * points[2].X + points[0].Y * (-points[1].X + points[2].X) + points[0].X * (points[1].Y - points[2].Y) + points[1].X * points[2].Y);
            float s = 1 / (2 * area) * (points[0].Y * points[2].X - points[0].X * points[2].Y + (points[2].Y - points[0].Y) * point.X + (points[0].X - points[2].X) * point.Y);
            float t = 1 / (2 * area) * (points[0].X * points[1].Y - points[0].Y * points[1].X + (points[0].Y - points[1].Y) * point.X + (points[1].X - points[0].X) * point.Y);
            float u = 1 - s - t;

            return s >= 0 && t >= 0 && u >= 0;
        }

        private Vector2[] GetTrianglePoints(Vector2 center, float radius)
        {
            Vector2[] points = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120 * (float)System.Math.PI / 180;
                points[i] = center + new Vector2(
                    radius * (float)System.Math.Cos(angle),
                    radius * (float)System.Math.Sin(angle)
                );
            }
            return points;
        }
    }

    public static class Vector2Extensions
    {
        public static Vector2 Normalized(this Vector2 vector)
        {
            float length = (float)System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            return length > 0 ? vector / length : vector;
        }
    }
}