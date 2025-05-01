using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace MultiplayerSnake
{
    public enum GameMode
    {
        SinglePlayer,
        Versus,
        Teams,
        VsAI // New mode
    }

    public class Game
    {
        static void Main(string[] args)
        {
            Raylib.InitWindow(800, 600, "Neon Snake Game");
            Raylib.SetTargetFPS(60);

            GameState gameState = GameState.Menu;
            Menu menu = new Menu();
            GameMode? selectedMode = null;
            List<bool> botSelections = null; // Store bot flags for Versus/Teams
            ScoreScreen scoreScreen = null;

            List<Snake> snakes = new List<Snake>();
            List<SnakeController> snakeControllers = new List<SnakeController>();
            Food regularFood = null;
            SpecialFoodManager specialFoodManager = null;
            ObstacleManager obstacleManager = new ObstacleManager();
            List<FloatingText> floatingTexts = new List<FloatingText>();
            Renderer renderer = new Renderer();
            List<float> snakeMoveTimers = new List<float>();

            int frameCount = 0;
            while (!Raylib.WindowShouldClose())
            {
                frameCount++;
                Console.WriteLine($"--- Frame {frameCount} Start ---");
                float deltaTime = Raylib.GetFrameTime();

                if (gameState == GameState.Menu)
                {
                    var menuResult = menu.Update();
                    selectedMode = menuResult.SelectedMode;
                    botSelections = menuResult.BotSelections;
                    if (selectedMode.HasValue)
                    {
                        snakes.Clear();
                        snakeControllers.Clear();
                        snakeMoveTimers.Clear();
                        regularFood = new Food(GenerateFoodPosition(), FoodType.Normal);
                        specialFoodManager = new SpecialFoodManager(selectedMode.Value);
                        obstacleManager = new ObstacleManager();
                        floatingTexts.Clear();
                        gameState = GameState.Playing;

                        if (selectedMode == GameMode.SinglePlayer)
                        {
                            // Only player snake
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakeControllers.Add(new SnakeController(snakes[0], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                            snakeMoveTimers.Add(0f);
                        }
                        else if (selectedMode == GameMode.VsAI)
                        {
                            // Player vs AI
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(350, 300), 5, 10, new Color((byte)255, (byte)0, (byte)0, (byte)255), 2));
                            snakeControllers.Add(new SnakeController(snakes[0], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                            snakeControllers.Add(new AIController(snakes[1], regularFood, specialFoodManager, obstacleManager, snakes));
                            snakeMoveTimers.AddRange(new float[] { 0f, 0f });
                        }
                        else if (selectedMode == GameMode.Versus)
                        {
                            // Initialize 3 snakes, using botSelections
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(350, 350), 5, 10, new Color((byte)255, (byte)0, (byte)0, (byte)255), 2));
                            snakes.Add(new Snake(new Vector2(300, 350), 5, 10, new Color((byte)0, (byte)0, (byte)255, (byte)255), 3));
                            for (int i = 0; i < 3; i++)
                            {
                                if (botSelections != null && botSelections.Count > i && botSelections[i])
                                {
                                    snakeControllers.Add(new AIController(snakes[i], regularFood, specialFoodManager, obstacleManager, snakes));
                                }
                                else
                                {
                                    if (i == 0)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                                    else if (i == 1)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.Up, KeyboardKey.Down, KeyboardKey.Left, KeyboardKey.Right));
                                    else
                                        snakeControllers.Add(new NetworkSnakeController(snakes[i], $"Player{i + 1}"));
                                }
                            }
                            snakeMoveTimers.AddRange(new float[] { 0f, 0f, 0f });
                        }
                        else if (selectedMode == GameMode.Teams)
                        {
                            // Initialize 4 snakes, using botSelections
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(350, 350), 5, 10, new Color((byte)0, (byte)200, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(300, 350), 5, 10, new Color((byte)255, (byte)0, (byte)0, (byte)255), 2));
                            snakes.Add(new Snake(new Vector2(250, 350), 5, 10, new Color((byte)200, (byte)0, (byte)0, (byte)255), 2));
                            for (int i = 0; i < 4; i++)
                            {
                                if (botSelections != null && botSelections.Count > i && botSelections[i])
                                {
                                    snakeControllers.Add(new AIController(snakes[i], regularFood, specialFoodManager, obstacleManager, snakes));
                                }
                                else
                                {
                                    if (i == 0)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                                    else if (i == 1)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.Up, KeyboardKey.Down, KeyboardKey.Left, KeyboardKey.Right));
                                    else if (i == 2)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.I, KeyboardKey.K, KeyboardKey.J, KeyboardKey.L));
                                    else
                                        snakeControllers.Add(new NetworkSnakeController(snakes[i], $"Player{i + 1}"));
                                }
                            }
                            snakeMoveTimers.AddRange(new float[] { 0f, 0f, 0f, 0f });
                        }
                    }
                }
                else if (gameState == GameState.Playing)
                {
                    for (int i = floatingTexts.Count - 1; i >= 0; i--)
                    {
                        floatingTexts[i].Update(deltaTime);
                        if (floatingTexts[i].IsDead)
                            floatingTexts.RemoveAt(i);
                    }

                    foreach (var snake in snakes)
                    {
                        snake.Update(deltaTime);
                    }

                    for (int i = 0; i < snakeControllers.Count; i++)
                    {
                        var snakeController = snakeControllers[i];
                        var snake = snakes[i];
                        Console.WriteLine($"Processing Controller {i} (Snake Color: {snake.BaseColor}, MoveCooldown: {snake.EffectManager.MoveCooldown})");
                        snakeController.HandleInput();
                        snakeMoveTimers[i] += deltaTime;
                        if (snakeMoveTimers[i] >= snake.EffectManager.MoveCooldown)
                        {
                            snakeController.Update(10);
                            snakeMoveTimers[i] = 0f;
                            Console.WriteLine($"Snake {i} moved (Color: {snake.BaseColor})");
                        }
                    }

                    foreach (var snake in snakes)
                    {
                        if (snake.EffectManager.HasEffect(SnakeEffectType.ShrinkEnemies) && selectedMode.HasValue)
                        {
                            foreach (var otherSnake in snakes)
                            {
                                bool shouldShrink = selectedMode == GameMode.Versus || selectedMode == GameMode.VsAI ||
                                                   (selectedMode == GameMode.Teams && snake.TeamId != otherSnake.TeamId);
                                if (otherSnake != snake && shouldShrink)
                                {
                                    Console.WriteLine($"Shrinking snake (Team {otherSnake.TeamId}) due to ShrinkEnemies by Team {snake.TeamId}");
                                    otherSnake.Shrink(2);
                                    floatingTexts.Add(new FloatingText("Shrunk!", otherSnake.Segments[0], new Color((byte)255, (byte)0, (byte)0, (byte)255)));
                                }
                            }
                        }
                    }

                    List<int> resetIndices = new List<int>();
                    for (int i = 0; i < snakes.Count; i++)
                    {
                        if (snakes[i].CheckCollisions(760, 580, obstacleManager))
                        {
                            resetIndices.Add(i);
                        }
                    }

                    if (resetIndices.Count > 0 && selectedMode.HasValue)
                    {
                        gameState = GameState.ScoreScreen;
                        scoreScreen = new ScoreScreen(snakes, selectedMode.Value);
                    }

                    List<Vector2> snakePositions = new List<Vector2>();
                    foreach (var snake in snakes)
                    {
                        snakePositions.AddRange(snake.Segments);
                    }
                    foreach (var snake in snakes)
                    {
                        if (snake.EatFood(regularFood.Position))
                        {
                            Vector2 newFoodPos = GenerateFoodPosition(snakePositions);
                            regularFood = new Food(newFoodPos, FoodType.Normal);
                            List<Vector2> specialFoodPositions = new List<Vector2>();
                            foreach (var sf in specialFoodManager.SpecialFoods)
                            {
                                specialFoodPositions.Add(sf.Position);
                            }
                            specialFoodManager.TrySpawnSpecialFood(newFoodPos, regularFood.Position, snakePositions, selectedMode ?? GameMode.SinglePlayer);
                            obstacleManager.TrySpawnPreview(regularFood.Position, snakePositions, specialFoodPositions);
                        }
                    }

                    specialFoodManager.Update();
                    obstacleManager.Update(deltaTime);
                    obstacleManager.UpdatePreviews(deltaTime);
                    foreach (var snake in snakes)
                    {
                        snake.CheckSpecialFoodCollision(specialFoodManager, floatingTexts);
                    }

                    foreach (var controller in snakeControllers)
                    {
                        if (controller is AIController aiController)
                        {
                            aiController.UpdateGameState(regularFood, specialFoodManager, obstacleManager, snakes);
                        }
                    }
                }
                else if (gameState == GameState.ScoreScreen && scoreScreen != null)
                {
                    GameState nextState = scoreScreen.Update();
                    if (nextState == GameState.Playing)
                    {
                        snakes.Clear();
                        snakeControllers.Clear();
                        snakeMoveTimers.Clear();
                        regularFood = new Food(GenerateFoodPosition(), FoodType.Normal);
                        specialFoodManager = new SpecialFoodManager(selectedMode ?? GameMode.SinglePlayer);
                        obstacleManager = new ObstacleManager();
                        floatingTexts.Clear();
                        gameState = GameState.Playing;

                        if (selectedMode == GameMode.SinglePlayer)
                        {
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakeControllers.Add(new SnakeController(snakes[0], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                            snakeMoveTimers.Add(0f);
                        }
                        else if (selectedMode == GameMode.VsAI)
                        {
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(350, 300), 5, 10, new Color((byte)255, (byte)0, (byte)0, (byte)255), 2));
                            snakeControllers.Add(new SnakeController(snakes[0], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                            snakeControllers.Add(new AIController(snakes[1], regularFood, specialFoodManager, obstacleManager, snakes));
                            snakeMoveTimers.AddRange(new float[] { 0f, 0f });
                        }
                        else if (selectedMode == GameMode.Versus)
                        {
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(350, 350), 5, 10, new Color((byte)255, (byte)0, (byte)0, (byte)255), 2));
                            snakes.Add(new Snake(new Vector2(300, 350), 5, 10, new Color((byte)0, (byte)0, (byte)255, (byte)255), 3));
                            for (int i = 0; i < 3; i++)
                            {
                                if (botSelections != null && botSelections.Count > i && botSelections[i])
                                {
                                    snakeControllers.Add(new AIController(snakes[i], regularFood, specialFoodManager, obstacleManager, snakes));
                                }
                                else
                                {
                                    if (i == 0)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                                    else if (i == 1)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.Up, KeyboardKey.Down, KeyboardKey.Left, KeyboardKey.Right));
                                    else
                                        snakeControllers.Add(new NetworkSnakeController(snakes[i], $"Player{i + 1}"));
                                }
                            }
                            snakeMoveTimers.AddRange(new float[] { 0f, 0f, 0f });
                        }
                        else if (selectedMode == GameMode.Teams)
                        {
                            snakes.Add(new Snake(new Vector2(400, 350), 5, 10, new Color((byte)0, (byte)255, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(350, 350), 5, 10, new Color((byte)0, (byte)200, (byte)0, (byte)255), 1));
                            snakes.Add(new Snake(new Vector2(300, 350), 5, 10, new Color((byte)255, (byte)0, (byte)0, (byte)255), 2));
                            snakes.Add(new Snake(new Vector2(250, 350), 5, 10, new Color((byte)200, (byte)0, (byte)0, (byte)255), 2));
                            for (int i = 0; i < 4; i++)
                            {
                                if (botSelections != null && botSelections.Count > i && botSelections[i])
                                {
                                    snakeControllers.Add(new AIController(snakes[i], regularFood, specialFoodManager, obstacleManager, snakes));
                                }
                                else
                                {
                                    if (i == 0)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.W, KeyboardKey.S, KeyboardKey.A, KeyboardKey.D));
                                    else if (i == 1)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.Up, KeyboardKey.Down, KeyboardKey.Left, KeyboardKey.Right));
                                    else if (i == 2)
                                        snakeControllers.Add(new SnakeController(snakes[i], KeyboardKey.I, KeyboardKey.K, KeyboardKey.J, KeyboardKey.L));
                                    else
                                        snakeControllers.Add(new NetworkSnakeController(snakes[i], $"Player{i + 1}"));
                                }
                            }
                            snakeMoveTimers.AddRange(new float[] { 0f, 0f, 0f, 0f });
                        }
                    }
                    else if (nextState == GameState.Menu)
                    {
                        gameState = GameState.Menu;
                        selectedMode = null;
                        botSelections = null;
                        scoreScreen = null;
                        snakes.Clear();
                        snakeControllers.Clear();
                        snakeMoveTimers.Clear();
                    }
                }

                Console.WriteLine("Starting Drawing Phase");
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color((byte)0, (byte)0, (byte)0, (byte)255));

                if (gameState == GameState.Menu)
                {
                    menu.Draw();
                }
                else if (gameState == GameState.Playing)
                {
                    renderer.DrawBorder();
                    renderer.DrawPreviews(obstacleManager);
                    renderer.DrawObstacles(obstacleManager);
                    foreach (var snake in snakes)
                    {
                        renderer.DrawSnake(snake);
                    }
                    if (regularFood != null)
                    {
                        renderer.DrawFood(regularFood.Position, regularFood.BaseColor);
                    }
                    renderer.DrawSpecialFood(specialFoodManager);
                    renderer.DrawFloatingTexts(floatingTexts);
                    renderer.DrawScores(snakes, selectedMode);
                }
                else if (gameState == GameState.ScoreScreen && scoreScreen != null)
                {
                    scoreScreen.Draw();
                }

                Raylib.EndDrawing();
                Console.WriteLine($"--- Frame {frameCount} End ---");
            }

            Raylib.CloseWindow();
        }

        private static Vector2 GenerateFoodPosition(List<Vector2> snakePositions = null)
        {
            Random rand = new Random();
            Vector2 pos;
            bool valid;
            int retries = 10;
            do
            {
                int x = 50 + rand.Next(0, (750 - 50) / 10) * 10;
                int y = 100 + rand.Next(0, (570 - 100) / 10) * 10;
                pos = new Vector2(x, y);
                valid = snakePositions == null || !snakePositions.Contains(pos);
                retries--;
            } while (!valid && retries > 0);
            return pos;
        }
    }
}