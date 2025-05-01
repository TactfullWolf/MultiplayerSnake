using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;

namespace MultiplayerSnake
{
    public class Renderer
    {
        public void DrawBorder()
        {
            for (int i = 0; i < 5; i++)
            {
                byte alpha = (byte)(255 - (i * 60));
                float sizeOffset = i * 2f;
                Rectangle rect = new Rectangle(
                    20 - sizeOffset, 70 - sizeOffset,
                    760 + 2 * sizeOffset, 510 + 2 * sizeOffset
                );
                Raylib.DrawRectangleRoundedLines(rect, 0.2f, 20, new Color((byte)0, (byte)255, (byte)255, alpha));
            }

            Rectangle outlineRect = new Rectangle(20, 70, 760, 510);
            Raylib.DrawRectangleRoundedLines(outlineRect, 0.2f, 20, new Color((byte)255, (byte)255, (byte)255, (byte)255));
            Console.WriteLine("Drawing rounded neon border with gradient at (20,70,760,510)");
        }

        public void DrawSnake(Snake snake)
        {
            float radius = 10f * snake.EffectManager.Scale;
            float totalLength = snake.Segments.Count;
            byte ghostAlpha = snake.GetGhostAlpha();

            for (int i = 0; i < snake.Segments.Count; i++)
            {
                float alpha = 1.0f - ((float)i / totalLength);
                byte finalAlpha = (byte)(255 * alpha * (ghostAlpha / 255f));
                Color snakeColor = new Color(snake.BaseColor.R, snake.BaseColor.G, snake.BaseColor.B, finalAlpha);
                Raylib.DrawCircleV(snake.Segments[i], radius, snakeColor);
                Color outlineColor = new Color((byte)255, (byte)255, (byte)255, finalAlpha);
                Raylib.DrawCircleLines((int)snake.Segments[i].X, (int)snake.Segments[i].Y, radius + 2, outlineColor);
                if (snake.IsGhostMode)
                {
                    Console.WriteLine($"Drawing snake with ghost alpha: {ghostAlpha}");
                }
            }

            foreach (var particle in snake.Particles)
            {
                float alphaFactor = particle.Lifetime / particle.MaxLifetime;
                Color particleColor = new Color(particle.Color.R, particle.Color.G, particle.Color.B, (byte)(alphaFactor * 255));
                Raylib.DrawCircle((int)particle.Position.X, (int)particle.Position.Y, 2f, particleColor);
            }
        }

        public void DrawFood(Vector2 position, Color color)
        {
            Raylib.DrawCircleV(position, 10f, color);
            Raylib.DrawCircleLines((int)position.X, (int)position.Y, 12f, new Color((byte)255, (byte)255, (byte)255, (byte)128));
            Console.WriteLine($"Drawing food at {position}");
        }

        public void DrawSpecialFood(SpecialFoodManager specialFoodManager)
        {
            foreach (var food in specialFoodManager.SpecialFoods)
            {
                if (food.Sides >= 3)
                {
                    Raylib.DrawPoly(food.Position, food.Sides, food.Radius * food.Scale * 1.2f, 0f, food.BaseColor);
                    Raylib.DrawPolyLines(food.Position, food.Sides, food.Radius * food.Scale * 1.2f + 2, 0f, new Color((byte)255, (byte)255, (byte)255, (byte)128));
                    Console.WriteLine($"Drawing {food.Type} with {food.Sides} sides at {food.Position}");
                }
                else
                {
                    switch (food.Type)
                    {
                        case SpecialFoodType.GhostMode:
                            float glowAlpha = 50 + 30 * (float)System.Math.Sin(food.scaleTimer * 2f);
                            Raylib.DrawCircleV(food.Position, food.Radius * 1.5f * food.Scale, new Color((byte)128, (byte)0, (byte)128, (byte)glowAlpha));
                            Raylib.DrawPoly(food.Position, 5, food.Radius * food.Scale * 1.2f, 0f, food.BaseColor);
                            Raylib.DrawPolyLines(food.Position, 5, food.Radius * food.Scale * 1.2f + 2, 0f, new Color((byte)255, (byte)255, (byte)255, (byte)128));
                            Vector2 eye1Pos = food.Position + new Vector2(-4f, -2f);
                            Vector2 eye2Pos = food.Position + new Vector2(4f, -2f);
                            Raylib.DrawCircleV(eye1Pos, 2f, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                            Raylib.DrawCircleV(eye2Pos, 2f, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                            Console.WriteLine($"Drawing GhostMode at {food.Position}, scale {food.Scale}, glow alpha {glowAlpha}");
                            break;

                        case SpecialFoodType.SpeedBoost:
                            Vector2 tri1Pos = food.Position + new Vector2(-4f, 0);
                            Vector2 tri2Pos = food.Position + new Vector2(4f, 0);
                            foreach (var pos in new[] { tri1Pos, tri2Pos })
                            {
                                Vector2[] points = new[]
                                {
                                    pos + new Vector2(-6f, -8f),
                                    pos + new Vector2(-6f, 8f),
                                    pos + new Vector2(6f, 0)
                                };
                                Raylib.DrawTriangle(points[0], points[1], points[2], food.BaseColor);
                                for (int i = 0; i < 3; i++)
                                {
                                    Raylib.DrawLineEx(points[i], points[(i + 1) % 3], 1f, new Color((byte)255, (byte)255, (byte)255, (byte)128));
                                }
                            }
                            break;

                        case SpecialFoodType.DoubleLength:
                            Raylib.DrawCircleV(food.Position, food.Radius, food.BaseColor);
                            Raylib.DrawCircleLines((int)food.Position.X, (int)food.Position.Y, food.Radius + 2, new Color((byte)255, (byte)255, (byte)255, (byte)128));
                            Raylib.DrawCircleV(food.Position, food.Radius * 0.5f, new Color((byte)0, (byte)100, (byte)0, (byte)255));
                            Raylib.DrawCircleLines((int)food.Position.X, (int)food.Position.Y, food.Radius * 0.5f + 2, new Color((byte)255, (byte)255, (byte)255, (byte)128));
                            Console.WriteLine($"Drawing DoubleLength bullseye at {food.Position}");
                            break;

                        case SpecialFoodType.ShrinkEnemies:
                            Raylib.DrawCircleV(food.Position, food.Radius * food.Scale, food.BaseColor);
                            Raylib.DrawCircleLines((int)food.Position.X, (int)food.Position.Y, food.Radius * food.Scale + 2, new Color((byte)255, (byte)255, (byte)255, (byte)128));
                            Raylib.DrawRectangle((int)(food.Position.X - 6), (int)(food.Position.Y - 2), 12, 4, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                            break;
                    }
                    Console.WriteLine($"Drawing {food.Type} at {food.Position}");
                }
            }
        }

        public void DrawObstacles(ObstacleManager obstacleManager)
        {
            foreach (var obstacle in obstacleManager.Obstacles)
            {
                // Glowing effect
                for (int i = 0; i < 3; i++)
                {
                    byte alpha = (byte)(100 - (i * 30));
                    float sizeOffset = i * 2f;
                    if (obstacle.Type == ObstacleType.PlusSign)
                    {
                        Rectangle hRect = new Rectangle(
                            obstacle.Position.X - obstacle.Width / 2 - sizeOffset,
                            obstacle.Position.Y - 10 - sizeOffset,
                            obstacle.Width + 2 * sizeOffset,
                            20 + 2 * sizeOffset
                        );
                        Rectangle vRect = new Rectangle(
                            obstacle.Position.X - 10 - sizeOffset,
                            obstacle.Position.Y - obstacle.Height / 2 - sizeOffset,
                            20 + 2 * sizeOffset,
                            obstacle.Height + 2 * sizeOffset
                        );
                        Raylib.DrawRectangleLinesEx(hRect, 1f, new Color((byte)0, (byte)255, (byte)255, alpha));
                        Raylib.DrawRectangleLinesEx(vRect, 1f, new Color((byte)0, (byte)255, (byte)255, alpha));
                    }
                    else if (obstacle.Type == ObstacleType.Box)
                    {
                        Rectangle rect = new Rectangle(
                            obstacle.Position.X - obstacle.Width / 2 - sizeOffset,
                            obstacle.Position.Y - obstacle.Height / 2 - sizeOffset,
                            obstacle.Width + 2 * sizeOffset,
                            obstacle.Height + 2 * sizeOffset
                        );
                        Raylib.DrawRectangleLinesEx(rect, 1f, new Color((byte)0, (byte)255, (byte)255, alpha));
                    }
                    else if (obstacle.Type == ObstacleType.Triangle)
                    {
                        Vector2[] points = GetTrianglePoints(obstacle.Position, obstacle.Width / 2 + sizeOffset);
                        for (int j = 0; j < 3; j++)
                        {
                            Raylib.DrawLineEx(points[j], points[(j + 1) % 3], 1f, new Color((byte)0, (byte)255, (byte)255, alpha));
                        }
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
                                Rectangle rect = new Rectangle(x - sizeOffset, y - 5 - sizeOffset, segmentLength + 2 * sizeOffset, 10 + 2 * sizeOffset);
                                Raylib.DrawRectangleLinesEx(rect, 1f, new Color((byte)0, (byte)255, (byte)255, alpha));
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
                                Rectangle rect = new Rectangle(x - 5 - sizeOffset, y - sizeOffset, 10 + 2 * sizeOffset, segmentLength + 2 * sizeOffset);
                                Raylib.DrawRectangleLinesEx(rect, 1f, new Color((byte)0, (byte)255, (byte)255, alpha));
                            }
                        }
                    }
                }

                // Main obstacle
                if (obstacle.Type == ObstacleType.PlusSign)
                {
                    Rectangle hRect = new Rectangle(obstacle.Position.X - obstacle.Width / 2, obstacle.Position.Y - 10, obstacle.Width, 20);
                    Rectangle vRect = new Rectangle(obstacle.Position.X - 10, obstacle.Position.Y - obstacle.Height / 2, 20, obstacle.Height);
                    Raylib.DrawRectangleRec(hRect, obstacle.BaseColor);
                    Raylib.DrawRectangleRec(vRect, obstacle.BaseColor);
                    Raylib.DrawRectangleLinesEx(hRect, 1f, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    Raylib.DrawRectangleLinesEx(vRect, 1f, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                }
                else if (obstacle.Type == ObstacleType.Box)
                {
                    Rectangle rect = new Rectangle(obstacle.Position.X - obstacle.Width / 2, obstacle.Position.Y - obstacle.Height / 2, obstacle.Width, obstacle.Height);
                    Raylib.DrawRectangleLinesEx(rect, 2f, obstacle.BaseColor);
                    Raylib.DrawRectangleLinesEx(rect, 1f, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                }
                else if (obstacle.Type == ObstacleType.Triangle)
                {
                    Vector2[] points = GetTrianglePoints(obstacle.Position, obstacle.Width / 2);
                    Raylib.DrawTriangle(points[0], points[1], points[2], obstacle.BaseColor);
                    for (int i = 0; i < 3; i++)
                    {
                        Raylib.DrawLineEx(points[i], points[(i + 1) % 3], 1f, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                    }
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
                            Raylib.DrawRectangle((int)x, (int)(y - 5), (int)segmentLength, 10, obstacle.BaseColor);
                            Raylib.DrawRectangleLines((int)x, (int)(y - 5), (int)segmentLength, 10, new Color((byte)255, (byte)255, (byte)255, (byte)255));
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
                            Raylib.DrawRectangle((int)(x - 5), (int)y, 10, (int)segmentLength, obstacle.BaseColor);
                            Raylib.DrawRectangleLines((int)(x - 5), (int)y, 10, (int)segmentLength, new Color((byte)255, (byte)255, (byte)255, (byte)255));
                        }
                    }
                }
                Console.WriteLine($"Drawing obstacle at {obstacle.Position} (Type: {obstacle.Type})");
            }
        }

        public void DrawPreviews(ObstacleManager obstacleManager)
        {
            foreach (var preview in obstacleManager.Previews)
            {
                float alpha = 255 * (0.5f + 0.5f * (float)System.Math.Sin(preview.Lifetime * 4 * System.Math.PI)); // Flash at 2Hz
                if (preview.Type == ObstacleType.PlusSign)
                {
                    Rectangle hRect = new Rectangle(preview.Position.X - preview.Width / 2, preview.Position.Y - 10, preview.Width, 20);
                    Rectangle vRect = new Rectangle(preview.Position.X - 10, preview.Position.Y - preview.Height / 2, 20, preview.Height);
                    Raylib.DrawRectangleLinesEx(hRect, 1f, new Color((byte)0, (byte)255, (byte)255, (byte)alpha));
                    Raylib.DrawRectangleLinesEx(vRect, 1f, new Color((byte)0, (byte)255, (byte)255, (byte)alpha));
                }
                else if (preview.Type == ObstacleType.Box)
                {
                    Rectangle rect = new Rectangle(preview.Position.X - preview.Width / 2, preview.Position.Y - preview.Height / 2, preview.Width, preview.Height);
                    Raylib.DrawRectangleLinesEx(rect, 1f, new Color((byte)0, (byte)255, (byte)255, (byte)alpha));
                }
                else if (preview.Type == ObstacleType.Triangle)
                {
                    Vector2[] points = GetTrianglePoints(preview.Position, preview.Width / 2);
                    for (int i = 0; i < 3; i++)
                    {
                        Raylib.DrawLineEx(points[i], points[(i + 1) % 3], 1f, new Color((byte)0, (byte)255, (byte)255, (byte)alpha));
                    }
                }
                else if (preview.Type == ObstacleType.DashedLine)
                {
                    if (preview.IsHorizontal)
                    {
                        float startX = 40;
                        float endX = 760;
                        float y = preview.Position.Y;
                        for (float x = startX; x < endX; x += 15)
                        {
                            float segmentLength = System.Math.Min(10, endX - x);
                            Raylib.DrawRectangleLines((int)x, (int)(y - 5), (int)segmentLength, 10, new Color((byte)0, (byte)255, (byte)255, (byte)alpha));
                        }
                    }
                    else
                    {
                        float startY = 90;
                        float endY = 580;
                        float x = preview.Position.X;
                        for (float y = startY; y < endY; y += 15)
                        {
                            float segmentLength = System.Math.Min(10, endY - y);
                            Raylib.DrawRectangleLines((int)(x - 5), (int)y, 10, (int)segmentLength, new Color((byte)0, (byte)255, (byte)255, (byte)alpha));
                        }
                    }
                }
                Console.WriteLine($"Drawing preview at {preview.Position} (Type: {preview.Type})");
            }
        }

        public void DrawFloatingTexts(List<FloatingText> floatingTexts)
        {
            foreach (var text in floatingTexts)
            {
                Raylib.DrawText(text.Text, (int)text.Position.X, (int)text.Position.Y, 20, text.Color);
            }
        }

        public void DrawScores(List<Snake> snakes, GameMode? gameMode)
        {
            if (gameMode == GameMode.Teams)
            {
                Dictionary<int, int> teamScores = new Dictionary<int, int>();
                foreach (var snake in snakes)
                {
                    if (!teamScores.ContainsKey(snake.TeamId))
                        teamScores[snake.TeamId] = 0;
                    teamScores[snake.TeamId] += snake.Score;
                }

                int xOffset = 40;
                foreach (var team in teamScores)
                {
                    Color teamColor = team.Key == 1 ? new Color((byte)0, (byte)255, (byte)0, (byte)255) : new Color((byte)255, (byte)0, (byte)0, (byte)255);
                    string text = $"Team {team.Key}: {team.Value}";
                    Raylib.DrawText(text, xOffset, 20, 20, teamColor);
                    xOffset += Raylib.MeasureText(text, 20) + 50;
                }
            }
            else
            {
                int xOffset = 40;
                for (int i = 0; i < snakes.Count; i++)
                {
                    var snake = snakes[i];
                    string text = $"P{i + 1}: {snake.Score}";
                    Raylib.DrawText(text, xOffset, 20, 20, snake.BaseColor);
                    xOffset += Raylib.MeasureText(text, 20) + 50;
                }
            }
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
}