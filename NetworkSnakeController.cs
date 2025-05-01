using Raylib_cs;
using System.Numerics;

namespace MultiplayerSnake
{
    public class NetworkSnakeController : SnakeController
    {
        private readonly string playerName;

        public NetworkSnakeController(Snake snake, string playerName)
            : base(snake, KeyboardKey.Null, KeyboardKey.Null, KeyboardKey.Null, KeyboardKey.Null)
        {
            this.playerName = playerName;
        }

        public override void HandleInput()
        {
            // Placeholder for network-based input (e.g., receive direction from server)
            // For now, do nothing to avoid affecting game behavior
            Console.WriteLine($"NetworkSnakeController for {playerName}: Awaiting network input");
        }

        public override void Update(int gridSize)
        {
            // Call base to maintain default movement behavior
            base.Update(gridSize);
        }
    }
}