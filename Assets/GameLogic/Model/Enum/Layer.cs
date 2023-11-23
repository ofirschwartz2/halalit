using System.ComponentModel;

namespace Assets.Enums
{
    public enum Layer
    {
        [Description("Default")]
        DEFAULT,
        [Description("EnemyShots")]
        ENEMY_SHOTS,
        [Description("PlayerShots")]
        PLAYER_SHOTS,
        [Description("Enemies")]
        ENEMIES,
        [Description("Player")]
        PLAYER,
        [Description("Asteroids")]
        ASTEROIDS,
        [Description("ItemTriggers")]
        ITEM_TRIGGERS,
        [Description("ItemCollisions")]
        ITEM_COLLISIONS,
    }
}