using Raylib_cs;
using System;
using System.Collections.Generic;

namespace MultiplayerSnake
{
    public enum GameState
    {
        Menu,
        Playing,
        ScoreScreen
    }

    public class Menu
    {
        private int selectedOption = 0;
        private readonly string[] options = { "Single Player", "VS AI", "Versus (FFA)", "Teams" };
        private bool selectingBots = false;
        private List<bool> botSelections = new List<bool>();
        private int currentBotIndex = 0;
        private GameMode? pendingMode = null;

        public (GameMode? SelectedMode, List<bool> BotSelections) Update()
        {
            if (!selectingBots)
            {
                // Main menu navigation
                if (Raylib.IsKeyPressed(KeyboardKey.Up))
                    selectedOption = (selectedOption - 1 + options.Length) % options.Length;
                if (Raylib.IsKeyPressed(KeyboardKey.Down))
                    selectedOption = (selectedOption + 1) % options.Length;

                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    GameMode mode = (GameMode)selectedOption;
                    if (mode == GameMode.Versus || mode == GameMode.Teams)
                    {
                        // Enter bot selection for Versus/Teams
                        selectingBots = true;
                        pendingMode = mode;
                        botSelections.Clear();
                        int numPlayers = mode == GameMode.Versus ? 3 : 4;
                        for (int i = 0; i < numPlayers; i++)
                            botSelections.Add(false); // Default to human
                        currentBotIndex = 0;
                    }
                    else
                    {
                        // Start SinglePlayer or VsAI immediately
                        return (mode, null);
                    }
                }
            }
            else
            {
                // Bot selection screen
                int numPlayers = pendingMode == GameMode.Versus ? 3 : 4;
                if (Raylib.IsKeyPressed(KeyboardKey.Up) && currentBotIndex > 0)
                    currentBotIndex--;
                if (Raylib.IsKeyPressed(KeyboardKey.Down) && currentBotIndex < numPlayers - 1)
                    currentBotIndex++;
                if (Raylib.IsKeyPressed(KeyboardKey.Space))
                    botSelections[currentBotIndex] = !botSelections[currentBotIndex]; // Toggle bot/human
                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    selectingBots = false;
                    var mode = pendingMode;
                    pendingMode = null;
                    return (mode, new List<bool>(botSelections));
                }
                if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                {
                    selectingBots = false;
                    pendingMode = null;
                    botSelections.Clear();
                }
            }

            return (null, null);
        }

        public void Draw()
        {
            if (!selectingBots)
            {
                // Draw main menu
                Raylib.DrawText("Neon Snake Game", 300, 100, 40, Color.White);
                for (int i = 0; i < options.Length; i++)
                {
                    Color color = i == selectedOption ? Color.Yellow : Color.White;
                    Raylib.DrawText(options[i], 350, 200 + i * 50, 30, color);
                }
                Raylib.DrawText("Use UP/DOWN to navigate, ENTER to select", 200, 500, 20, Color.Gray);
            }
            else
            {
                // Draw bot selection screen
                int numPlayers = pendingMode == GameMode.Versus ? 3 : 4;
                Raylib.DrawText($"Select Bots for {pendingMode}", 300, 100, 40, Color.White);
                for (int i = 0; i < numPlayers; i++)
                {
                    string text = $"Player {i + 1}: {(botSelections[i] ? "Bot" : "Human")}";
                    Color color = i == currentBotIndex ? Color.Yellow : Color.White;
                    Raylib.DrawText(text, 350, 200 + i * 50, 30, color);
                }
                Raylib.DrawText("UP/DOWN to select, SPACE to toggle, ENTER to start, ESC to cancel", 150, 500, 20, Color.Gray);
            }
        }
    }
}