using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MultiplayerSnake
{
    public class SpecialFoodManager
    {
        public List<SpecialFood> SpecialFoods { get; private set; }
        private Random rand;
        private float spawnTimer;
        private readonly GameMode gameMode;
        private const float spawnInterval = 10.0f;
        private readonly List<SpecialFoodType> singlePlayerRestrictedPowerUps = new List<SpecialFoodType>
        {
            SpecialFoodType.ShrinkEnemies
            // Add future power-ups here
        };

        public SpecialFoodManager(GameMode gameMode)
        {
            SpecialFoods = new List<SpecialFood>();
            rand = new Random();
            spawnTimer = spawnInterval;
            this.gameMode = gameMode;
        }

        public void Update()
        {
            spawnTimer -= Raylib.GetFrameTime();
            if (spawnTimer <= 0)
            {
                TrySpawnSpecialFood(GenerateRandomPosition(), gameMode: gameMode);
                spawnTimer = spawnInterval;
            }

            foreach (var food in SpecialFoods)
            {
                food.Update(Raylib.GetFrameTime());
            }
        }

        public void TrySpawnSpecialFood(Vector2 position, Vector2? foodPosition = null, List<Vector2> snakePositions = null, GameMode gameMode = GameMode.SinglePlayer)
        {
            if (rand.NextDouble() < 0.3)
            {
                bool overlaps = (foodPosition.HasValue && position == foodPosition.Value) ||
                               (snakePositions != null && snakePositions.Contains(position));
                if (overlaps)
                {
                    Console.WriteLine($"Avoided spawning special food at {position} (overlaps food or snake)");
                    position = GenerateRandomPosition();
                    int retries = 5;
                    while (retries > 0 && (
                        (foodPosition.HasValue && position == foodPosition.Value) ||
                        (snakePositions != null && snakePositions.Contains(position))))
                    {
                        position = GenerateRandomPosition();
                        retries--;
                    }
                }

                foreach (var existingFood in SpecialFoods)
                {
                    if (existingFood.Position == position)
                    {
                        Console.WriteLine($"Avoided spawning special food at {position} (overlaps existing special food)");
                        return;
                    }
                }

                var allTypes = Enum.GetValues(typeof(SpecialFoodType)).Cast<SpecialFoodType>().ToList();
                // Restrict power-ups only in SinglePlayer mode
                if (gameMode == GameMode.SinglePlayer)
                {
                    allTypes = allTypes.Except(singlePlayerRestrictedPowerUps).ToList();
                }
                if (allTypes.Count == 0)
                {
                    Console.WriteLine("No valid special food types available for spawning");
                    return;
                }

                SpecialFoodType type = allTypes[rand.Next(allTypes.Count)];
                SpecialFoods.Add(new SpecialFood(position, type));
                Console.WriteLine($"Spawned {type} special food at {position} with color {SpecialFoods[SpecialFoods.Count - 1].BaseColor}");
            }
        }

        public void Remove(SpecialFood food)
        {
            SpecialFoods.Remove(food);
        }

        private Vector2 GenerateRandomPosition()
        {
            int x = 40 + rand.Next(0, (760 - 40) / 10) * 10;
            int y = 90 + rand.Next(0, (580 - 90) / 10) * 10;
            return new Vector2(x, y);
        }
    }
}