using System;
using System.Collections.Generic;

namespace MultiplayerSnake
{
    public enum SnakeEffectType
    {
        GhostMode,
        SpeedBoost,
        ShrinkEnemies
    }

    public class SnakeEffectManager
    {
        private Snake snake;
        private Dictionary<SnakeEffectType, float> activeEffects;
        private float defaultMoveCooldown = 0.1f;
        private float speedBoostMoveCooldown = 0.05f;
        public float MoveCooldown { get; private set; }
        public float Scale { get; private set; }

        public SnakeEffectManager(Snake snake)
        {
            this.snake = snake;
            activeEffects = new Dictionary<SnakeEffectType, float>();
            MoveCooldown = defaultMoveCooldown;
            Scale = 1.0f;
        }

        public void ApplyEffect(SnakeEffectType effectType, float duration)
        {
            activeEffects[effectType] = duration;

            switch (effectType)
            {
                case SnakeEffectType.GhostMode:
                    snake.IsGhostMode = true;
                    break;
                case SnakeEffectType.SpeedBoost:
                    snake.IsSpeedBoosted = true;
                    MoveCooldown = speedBoostMoveCooldown;
                    Scale = 1.1f;
                    Console.WriteLine($"Applied SpeedBoost: MoveCooldown={MoveCooldown}, Scale={Scale}");
                    break;
                case SnakeEffectType.ShrinkEnemies:
                    break;
            }
        }

        public bool HasEffect(SnakeEffectType effectType)
        {
            return activeEffects.ContainsKey(effectType) && activeEffects[effectType] > 0;
        }

        public void Update(float deltaTime)
        {
            List<SnakeEffectType> effectsToRemove = new List<SnakeEffectType>();

            foreach (var effect in activeEffects)
            {
                activeEffects[effect.Key] -= deltaTime;
                if (activeEffects[effect.Key] <= 0)
                {
                    effectsToRemove.Add(effect.Key);
                }
            }

            foreach (var effect in effectsToRemove)
            {
                activeEffects.Remove(effect);
                switch (effect)
                {
                    case SnakeEffectType.GhostMode:
                        snake.IsGhostMode = false;
                        break;
                    case SnakeEffectType.SpeedBoost:
                        snake.IsSpeedBoosted = false;
                        MoveCooldown = defaultMoveCooldown;
                        Scale = 1.0f;
                        Console.WriteLine($"Removed SpeedBoost: MoveCooldown={MoveCooldown}, Scale={Scale}");
                        break;
                }
            }
        }
    }
}