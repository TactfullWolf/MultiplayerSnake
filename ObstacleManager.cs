using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MultiplayerSnake
{
    public enum ObstacleType
    {
        PlusSign,
        Box,
        Triangle,
        DashedLine
    }

    public class Preview
    {
        public Vector2 Position { get; private set; }
        public ObstacleType Type { get; private set; }
        public float Lifetime { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public bool IsHorizontal { get; private set; }

        public Preview(Vector2 position, ObstacleType type, float lifetime, float width, float height, bool isHorizontal = false)
        {
            Position = position;
            Type = type;
            Lifetime = lifetime;
            Width = width;
            Height = height;
            IsHorizontal = isHorizontal;
        }

        public void Update(float deltaTime)
        {
            Lifetime -= deltaTime;
        }

        public bool IsDead => Lifetime <= 0;
    }

    public class Obstacle
    {
        public Vector2 Position { get; private set; }
        public ObstacleType Type { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Lifetime { get; private set; }
        public Color BaseColor { get; private set; }
        public bool IsHorizontal { get; private set; }

        public Obstacle(Vector2 position, ObstacleType type, float width, float height, float lifetime, bool isHorizontal = false)
        {
            Position = position;
            Type = type;
            Width = width;
            Height = height;
            Lifetime = lifetime;
            BaseColor = new Color((byte)0, (byte)255, (byte)255, (byte)255);
            IsHorizontal = isHorizontal;
        }

        public void Update(float deltaTime)
        {
            Lifetime -= deltaTime;
        }

        public bool IsDead => Lifetime <= 0;
    }

    public class ObstacleManager
    {
        public List<Obstacle> Obstacles { get; private set; }
        public List<Preview> Previews { get; private set; }
        private Random rand;
        private float spawnTimer;
        private const float spawnInterval = 10.0f;
        private const float obstacleLifetime = 15.0f;
        private const float previewLifetime = 2.0f; // Increased from 1.0f to 2.0f
        private const float symmetryChance = 0.8f;

        public ObstacleManager()
        {
            Obstacles = new List<Obstacle>();
            Previews = new List<Preview>();
            rand = new Random();
            spawnTimer = spawnInterval;
        }

        public void Update(float deltaTime)
        {
            spawnTimer -= deltaTime;
            if (spawnTimer <= 0)
            {
                TrySpawnPreview();
                spawnTimer = spawnInterval;
            }

            for (int i = Obstacles.Count - 1; i >= 0; i--)
            {
                Obstacles[i].Update(deltaTime);
                if (Obstacles[i].IsDead)
                {
                    Console.WriteLine($"Despawning obstacle at {Obstacles[i].Position} (Type: {Obstacles[i].Type})");
                    Obstacles.RemoveAt(i);
                }
            }
        }

        public void UpdatePreviews(float deltaTime)
        {
            for (int i = Previews.Count - 1; i >= 0; i--)
            {
                Previews[i].Update(deltaTime);
                if (Previews[i].IsDead)
                {
                    Vector2 pos = Previews[i].Position;
                    ObstacleType type = Previews[i].Type;
                    float width = Previews[i].Width;
                    float height = Previews[i].Height;
                    bool isHorizontal = Previews[i].IsHorizontal;
                    bool overlaps = Obstacles.Exists(o => IsOverlap(pos, o.Position, width, height, o.Width, o.Height));

                    if (!overlaps)
                    {
                        Obstacles.Add(new Obstacle(pos, type, width, height, obstacleLifetime, isHorizontal));
                        Console.WriteLine($"Spawned obstacle at {pos} (Type: {type}, Size: {width}x{height})");
                    }
                    else
                    {
                        Console.WriteLine($"Skipped spawning obstacle at {pos} (overlaps existing obstacle)");
                    }
                    Previews.RemoveAt(i);
                }
            }
        }

        public void TrySpawnPreview(Vector2? foodPosition = null, List<Vector2> snakePositions = null, List<Vector2> specialFoodPositions = null)
        {
            if (rand.NextDouble() < 0.5)
            {
                Vector2 pos = GenerateRandomPosition();
                ObstacleType type = (ObstacleType)rand.Next(Enum.GetValues(typeof(ObstacleType)).Length);

                // Variable sizes for obstacles
                float width, height;
                bool isHorizontal = false;
                if (type == ObstacleType.DashedLine)
                {
                    isHorizontal = rand.Next(0, 2) == 0;
                    width = isHorizontal ? 720 : rand.Next(8, 13); // Dashed line thickness 8–12
                    height = isHorizontal ? rand.Next(8, 13) : 490; // Dashed line thickness 8–12
                }
                else
                {
                    width = rand.Next(20, 61); // Random width 20–60
                    height = rand.Next(20, 61); // Random height 20–60
                }

                bool overlaps = false;
                if (foodPosition.HasValue)
                    overlaps |= IsOverlap(pos, foodPosition.Value, width, height, 10, 10);
                if (snakePositions != null)
                    overlaps |= snakePositions.Exists(sp => IsOverlap(pos, sp, width, height, 10, 10));
                if (specialFoodPositions != null)
                    overlaps |= specialFoodPositions.Exists(sfp => IsOverlap(pos, sfp, width, height, 10, 10));
                overlaps |= Obstacles.Exists(o => IsOverlap(pos, o.Position, width, height, o.Width, o.Height));

                if (overlaps)
                {
                    Console.WriteLine($"Avoided spawning preview at {pos} (overlaps)");
                    pos = GenerateRandomPosition();
                    int retries = 5;
                    while (retries > 0 && (
                        (foodPosition.HasValue && IsOverlap(pos, foodPosition.Value, width, height, 10, 10)) ||
                        (snakePositions != null && snakePositions.Exists(sp => IsOverlap(pos, sp, width, height, 10, 10))) ||
                        (specialFoodPositions != null && specialFoodPositions.Exists(sfp => IsOverlap(pos, sfp, width, height, 10, 10))) ||
                        Obstacles.Exists(o => IsOverlap(pos, o.Position, width, height, o.Width, o.Height))))
                    {
                        pos = GenerateRandomPosition();
                        retries--;
                    }
                    if (retries == 0) return;
                }

                Previews.Add(new Preview(pos, type, previewLifetime, width, height, isHorizontal));
                Console.WriteLine($"Spawned preview at {pos} (Type: {type}, Size: {width}x{height})");

                if (rand.NextDouble() < symmetryChance)
                {
                    Vector2 symPos = new Vector2(760 - pos.X + 40, pos.Y);
                    bool symOverlaps = false;
                    if (foodPosition.HasValue)
                        symOverlaps |= IsOverlap(symPos, foodPosition.Value, width, height, 10, 10);
                    if (snakePositions != null)
                        symOverlaps |= snakePositions.Exists(sp => IsOverlap(symPos, sp, width, height, 10, 10));
                    if (specialFoodPositions != null)
                        symOverlaps |= specialFoodPositions.Exists(sfp => IsOverlap(symPos, sfp, width, height, 10, 10));
                    symOverlaps |= Obstacles.Exists(o => IsOverlap(symPos, o.Position, width, height, o.Width, o.Height));

                    if (!symOverlaps)
                    {
                        Previews.Add(new Preview(symPos, type, previewLifetime, width, height, isHorizontal));
                        Console.WriteLine($"Spawned symmetrical preview at {symPos} (Type: {type}, Size: {width}x{height})");
                    }
                    else
                    {
                        Console.WriteLine($"Skipped symmetrical preview at {symPos} (overlaps)");
                    }
                }
            }
        }

        private Vector2 GenerateRandomPosition()
        {
            int x = 40 + rand.Next(0, (760 - 40) / 10) * 10;
            int y = 90 + rand.Next(0, (580 - 90) / 10) * 10;
            return new Vector2(x, y);
        }

        private bool IsOverlap(Vector2 pos1, Vector2 pos2, float width1, float height1, float width2, float height2)
        {
            float left1 = pos1.X - width1 / 2;
            float right1 = pos1.X + width1 / 2;
            float top1 = pos1.Y - height1 / 2;
            float bottom1 = pos1.Y + height1 / 2;

            float left2 = pos2.X - width2 / 2;
            float right2 = pos2.X + width2 / 2;
            float top2 = pos2.Y - height2 / 2;
            float bottom2 = pos2.Y + height2 / 2;

            return left1 < right2 && right1 > left2 && top1 < bottom2 && bottom1 > top2;
        }
    }
}