using Raylib_cs;
using System.Numerics;

namespace MultiplayerSnake
{
    public class SnakeController
    {
        protected Snake snake;
        private readonly KeyboardKey upKey;
        private readonly KeyboardKey downKey;
        private readonly KeyboardKey leftKey;
        private readonly KeyboardKey rightKey;

        public SnakeController(Snake snake, KeyboardKey upKey, KeyboardKey downKey, KeyboardKey leftKey, KeyboardKey rightKey)
        {
            this.snake = snake;
            this.upKey = upKey;
            this.downKey = downKey;
            this.leftKey = leftKey;
            this.rightKey = rightKey;
        }

        public virtual void HandleInput()
        {
            Vector2 newDirection = snake.Direction;

            if (Raylib.IsKeyDown(upKey) && snake.Direction.Y != 1)
                newDirection = new Vector2(0, -1);
            else if (Raylib.IsKeyDown(downKey) && snake.Direction.Y != -1)
                newDirection = new Vector2(0, 1);
            else if (Raylib.IsKeyDown(leftKey) && snake.Direction.X != 1)
                newDirection = new Vector2(-1, 0);
            else if (Raylib.IsKeyDown(rightKey) && snake.Direction.X != -1)
                newDirection = new Vector2(1, 0);

            snake.Direction = newDirection;
        }

        public virtual void Update(int gridSize)
        {
            snake.Move(gridSize);
        }
    }
}