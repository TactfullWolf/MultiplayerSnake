using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace MultiplayerSnake
{
    public class AIController : SnakeController
    {
        private new readonly Snake snake;
        private Food? regularFood;
        private SpecialFoodManager? specialFoodManager;
        private ObstacleManager obstacleManager;
        private List<Snake> allSnakes;
        private readonly Random rand;
        private readonly int gridSize = 10;
        private readonly int minX = 40, maxX = 760, minY = 90, maxY = 580;

        public AIController(Snake snake, Food regularFood, SpecialFoodManager specialFoodManager, ObstacleManager obstacleManager, List<Snake> allSnakes)
            : base(snake, KeyboardKey.Null, KeyboardKey.Null, KeyboardKey.Null, KeyboardKey.Null)
        {
            this.snake = snake;
            this.regularFood = regularFood;
            this.specialFoodManager = specialFoodManager;
            this.obstacleManager = obstacleManager;
            this.allSnakes = allSnakes;
            this.rand = new Random();
        }

        public void UpdateGameState(Food? regularFood, SpecialFoodManager? specialFoodManager, ObstacleManager obstacleManager, List<Snake> allSnakes)
        {
            this.regularFood = regularFood;
            this.specialFoodManager = specialFoodManager;
            this.obstacleManager = obstacleManager;
            this.allSnakes = allSnakes;
        }

        public override void HandleInput()
        {
            // AI doesn't use manual input
        }

        public override void Update(int gridSize)
        {
            Vector2 target = ChooseTarget();
            List<Vector2> path = FindPath(snake.Segments[0], target);

            if (path != null && path.Count > 1)
            {
                Vector2 nextPos = path[1];
                Vector2 direction = (nextPos - snake.Segments[0]) / gridSize;
                if (IsValidDirection(direction))
                {
                    snake.Direction = direction;
                    Console.WriteLine($"AI snake moving toward {nextPos} (Target: {target})");
                }
                else
                {
                    ChooseSafeDirection();
                }
            }
            else
            {
                ChooseSafeDirection();
            }

            snake.Move(gridSize);
        }

        private Vector2 ChooseTarget()
        {
            Vector2 regularFoodPos = regularFood?.Position ?? snake.Segments[0];
            float minDist = Vector2.Distance(snake.Segments[0], regularFoodPos);
            Vector2 target = regularFoodPos;

            if (specialFoodManager != null)
            {
                foreach (var specialFood in specialFoodManager.SpecialFoods)
                {
                    float dist = Vector2.Distance(snake.Segments[0], specialFood.Position);
                    if (dist < minDist && rand.NextDouble() > 0.3)
                    {
                        minDist = dist;
                        target = specialFood.Position;
                    }
                }
            }

            return target;
        }

        private List<Vector2> FindPath(Vector2 start, Vector2 goal)
        {
            var openSet = new PriorityQueue<Node>();
            var closedSet = new HashSet<Vector2>();
            var cameFrom = new Dictionary<Vector2, Vector2>();
            var gScore = new Dictionary<Vector2, float> { { start, 0 } };
            var fScore = new Dictionary<Vector2, float> { { start, ManhattanDistance(start, goal) } };

            openSet.Enqueue(new Node(start, fScore[start]), fScore[start]);

            while (openSet.Count > 0)
            {
                Vector2 current = openSet.Dequeue().Position;

                if (Vector2.Distance(current, goal) < gridSize)
                {
                    return ReconstructPath(cameFrom, current);
                }

                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor) || !IsSafePosition(neighbor))
                        continue;

                    float tentativeGScore = gScore[current] + gridSize;

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + ManhattanDistance(neighbor, goal);
                        openSet.Enqueue(new Node(neighbor, fScore[neighbor]), fScore[neighbor]);
                    }
                }
            }

            return null;
        }

        private List<Vector2> GetNeighbors(Vector2 pos)
        {
            var neighbors = new List<Vector2>
            {
                pos + new Vector2(gridSize, 0),
                pos + new Vector2(-gridSize, 0),
                pos + new Vector2(0, gridSize),
                pos + new Vector2(0, -gridSize)
            };
            return neighbors;
        }

        private bool IsSafePosition(Vector2 pos)
        {
            if (pos.X < minX || pos.X > maxX || pos.Y < minY || pos.Y > maxY)
                return false;

            foreach (var s in allSnakes)
            {
                foreach (var segment in s.Segments)
                {
                    if (Vector2.Distance(pos, segment) < gridSize)
                        return false;
                }
            }

            foreach (var obstacle in obstacleManager.Obstacles)
            {
                if (obstacle.Type == ObstacleType.PlusSign)
                {
                    Rectangle hRect = new Rectangle(obstacle.Position.X - obstacle.Width / 2, obstacle.Position.Y - 10, obstacle.Width, 20);
                    Rectangle vRect = new Rectangle(obstacle.Position.X - 10, obstacle.Position.Y - obstacle.Height / 2, 20, obstacle.Height);
                    if (CheckRectangleCollision(pos, hRect) || CheckRectangleCollision(pos, vRect))
                        return false;
                }
                else if (obstacle.Type == ObstacleType.Box)
                {
                    Rectangle rect = new Rectangle(obstacle.Position.X - obstacle.Width / 2, obstacle.Position.Y - obstacle.Height / 2, obstacle.Width, obstacle.Height);
                    if (CheckBoxCollision(pos, rect))
                        return false;
                }
                else if (obstacle.Type == ObstacleType.Triangle)
                {
                    Vector2[] points = GetTrianglePoints(obstacle.Position, obstacle.Width / 2);
                    if (CheckTriangleCollision(pos, points))
                        return false;
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
                            float segmentLength = Math.Min(10, endX - x);
                            Rectangle rect = new Rectangle(x, y - 5, segmentLength, 10);
                            if (CheckRectangleCollision(pos, rect))
                                return false;
                        }
                    }
                    else
                    {
                        float startY = 90;
                        float endY = 580;
                        float x = obstacle.Position.X;
                        for (float y = startY; y < endY; y += 15)
                        {
                            float segmentLength = Math.Min(10, endY - y);
                            Rectangle rect = new Rectangle(x - 5, y, 10, segmentLength);
                            if (CheckRectangleCollision(pos, rect))
                                return false;
                        }
                    }
                }
            }

            foreach (var preview in obstacleManager.Previews)
            {
                if (preview.Type == ObstacleType.PlusSign)
                {
                    Rectangle hRect = new Rectangle(preview.Position.X - preview.Width / 2, preview.Position.Y - 10, preview.Width, 20);
                    Rectangle vRect = new Rectangle(preview.Position.X - 10, preview.Position.Y - preview.Height / 2, 20, preview.Height);
                    if (CheckRectangleCollision(pos, hRect) || CheckRectangleCollision(pos, vRect))
                        return false;
                }
                else if (preview.Type == ObstacleType.Box)
                {
                    Rectangle rect = new Rectangle(preview.Position.X - preview.Width / 2, preview.Position.Y - preview.Height / 2, preview.Width, preview.Height);
                    if (CheckBoxCollision(pos, rect))
                        return false;
                }
                else if (preview.Type == ObstacleType.Triangle)
                {
                    Vector2[] points = GetTrianglePoints(preview.Position, preview.Width / 2);
                    if (CheckTriangleCollision(pos, points))
                        return false;
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
                            float segmentLength = Math.Min(10, endX - x);
                            Rectangle rect = new Rectangle(x, y - 5, segmentLength, 10);
                            if (CheckRectangleCollision(pos, rect))
                                return false;
                        }
                    }
                    else
                    {
                        float startY = 90;
                        float endY = 580;
                        float x = preview.Position.X;
                        for (float y = startY; y < endY; y += 15)
                        {
                            float segmentLength = Math.Min(10, endY - y);
                            Rectangle rect = new Rectangle(x - 5, y, 10, segmentLength);
                            if (CheckRectangleCollision(pos, rect))
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        private void ChooseSafeDirection()
        {
            var directions = new List<Vector2>
            {
                new Vector2(1, 0),
                new Vector2(-1, 0),
                new Vector2(0, 1),
                new Vector2(0, -1)
            };

            var safeDirections = new List<Vector2>();
            foreach (var dir in directions)
            {
                if (IsValidDirection(dir))
                {
                    Vector2 nextPos = snake.Segments[0] + dir * gridSize;
                    if (IsSafePosition(nextPos))
                    {
                        safeDirections.Add(dir);
                    }
                }
            }

            if (safeDirections.Count > 0)
            {
                snake.Direction = safeDirections[rand.Next(safeDirections.Count)];
                Console.WriteLine($"AI chose safe direction: {snake.Direction}");
            }
            else
            {
                Console.WriteLine("AI has no safe direction, continuing current");
            }
        }

        private bool IsValidDirection(Vector2 dir)
        {
            return Vector2.Dot(dir, snake.Direction) >= 0 || Vector2.Distance(dir, -snake.Direction) > 1.5f;
        }

        private float ManhattanDistance(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
        {
            var path = new List<Vector2> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
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

            return (point.X >= left && point.X <= right && (Math.Abs(point.Y - top) <= thickness || Math.Abs(point.Y - bottom) <= thickness)) ||
                   (point.Y >= top && point.Y <= bottom && (Math.Abs(point.X - left) <= thickness || Math.Abs(point.X - right) <= thickness));
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
                float angle = i * 120 * (float)Math.PI / 180;
                points[i] = center + new Vector2(
                    radius * (float)Math.Cos(angle),
                    radius * (float)Math.Sin(angle)
                );
            }
            return points;
        }

        private class Node
        {
            public Vector2 Position { get; }
            public float FScore { get; }

            public Node(Vector2 position, float fScore)
            {
                Position = position;
                FScore = fScore;
            }
        }

        private class PriorityQueue<T>
        {
            private readonly List<(T Item, float Priority)> elements = new List<(T, float)>();

            public int Count => elements.Count;

            public void Enqueue(T item, float priority)
            {
                elements.Add((item, priority));
                elements.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            public T Dequeue()
            {
                if (elements.Count == 0)
                    throw new InvalidOperationException("Queue is empty");
                var item = elements[0].Item;
                elements.RemoveAt(0);
                return item;
            }
        }
    }
}