using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

namespace MultiplayerSnake
{
    public class ScoreScreen
    {
        private readonly List<Snake> snakes;
        private readonly GameMode gameMode;
        private readonly string[] options = { "Play Again", "Main Menu" };
        private int selectedOption = 0;
        private const int buttonWidth = 200;
        private const int buttonHeight = 50;
        private const int buttonSpacing = 20;
        private readonly Color buttonColor = new Color((byte)169, (byte)169, (byte)169, (byte)255);
        private readonly Color selectedColor = new Color((byte)0, (byte)255, (byte)0, (byte)255);

        public ScoreScreen(List<Snake> snakes, GameMode gameMode)
        {
            this.snakes = snakes;
            this.gameMode = gameMode;
        }

        public GameState Update()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                selectedOption = (selectedOption - 1 + options.Length) % options.Length;
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Down))
            {
                selectedOption = (selectedOption + 1) % options.Length;
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                return selectedOption == 0 ? GameState.Playing : GameState.Menu;
            }

            Vector2 mousePos = Raylib.GetMousePosition();
            for (int i = 0; i < options.Length; i++)
            {
                Rectangle buttonRect = new Rectangle(
                    (Raylib.GetScreenWidth() - buttonWidth) / 2,
                    400 + i * (buttonHeight + buttonSpacing),
                    buttonWidth,
                    buttonHeight
                );
                if (Raylib.CheckCollisionPointRec(mousePos, buttonRect) && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    selectedOption = i;
                    return selectedOption == 0 ? GameState.Playing : GameState.Menu;
                }
            }

            return GameState.ScoreScreen;
        }

        public void Draw()
        {
            string title = "Game Over - Score Screen";
            int titleWidth = Raylib.MeasureText(title, 40);
            Raylib.DrawText(title, (Raylib.GetScreenWidth() - titleWidth) / 2, 50, 40, new Color((byte)255, (byte)255, (byte)255, (byte)255));

            int yOffset = 100;
            if (gameMode == GameMode.Teams)
            {
                Dictionary<int, int> teamScores = new Dictionary<int, int>();
                Dictionary<int, int> teamFoodEaten = new Dictionary<int, int>();
                Dictionary<int, int> teamSpecialFoodEaten = new Dictionary<int, int>();
                foreach (var snake in snakes)
                {
                    if (!teamScores.ContainsKey(snake.TeamId))
                    {
                        teamScores[snake.TeamId] = 0;
                        teamFoodEaten[snake.TeamId] = 0;
                        teamSpecialFoodEaten[snake.TeamId] = 0;
                    }
                    teamScores[snake.TeamId] += snake.Score;
                    teamFoodEaten[snake.TeamId] += snake.FoodEaten;
                    teamSpecialFoodEaten[snake.TeamId] += snake.SpecialFoodEaten;
                }

                foreach (var team in teamScores)
                {
                    Color teamColor = team.Key == 1 ? new Color((byte)0, (byte)255, (byte)0, (byte)255) : new Color((byte)255, (byte)0, (byte)0, (byte)255);
                    string teamText = $"Team {team.Key}";
                    Raylib.DrawText(teamText, 100, yOffset, 30, teamColor);
                    Raylib.DrawText($"Score: {team.Value}", 300, yOffset, 20, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    Raylib.DrawText($"Regular Food: {teamFoodEaten[team.Key]}", 300, yOffset + 25, 20, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    Raylib.DrawText($"Special Food: {teamSpecialFoodEaten[team.Key]}", 300, yOffset + 50, 20, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    yOffset += 100;
                }
            }
            else
            {
                for (int i = 0; i < snakes.Count; i++)
                {
                    var snake = snakes[i];
                    string playerName = gameMode == GameMode.SinglePlayer ? "Player" : $"Player {i + 1}";
                    Raylib.DrawText(playerName, 100, yOffset, 30, snake.BaseColor);
                    Raylib.DrawText($"Score: {snake.Score}", 300, yOffset, 20, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    Raylib.DrawText($"Regular Food: {snake.FoodEaten}", 300, yOffset + 25, 20, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    Raylib.DrawText($"Special Food: {snake.SpecialFoodEaten}", 300, yOffset + 50, 20, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    yOffset += 100;
                }
            }

            for (int i = 0; i < options.Length; i++)
            {
                Color color = (i == selectedOption) ? selectedColor : buttonColor;
                Rectangle buttonRect = new Rectangle(
                    (Raylib.GetScreenWidth() - buttonWidth) / 2,
                    400 + i * (buttonHeight + buttonSpacing),
                    buttonWidth,
                    buttonHeight
                );

                Raylib.DrawRectangleRec(buttonRect, color);
                int textWidth = Raylib.MeasureText(options[i], 20);
                Raylib.DrawText(
                    options[i],
                    (int)(buttonRect.X + (buttonWidth - textWidth) / 2),
                    (int)(buttonRect.Y + (buttonHeight - 20) / 2),
                    20,
                    new Color((byte)255, (byte)255, (byte)255, (byte)255)
                );
            }
        }
    }
}