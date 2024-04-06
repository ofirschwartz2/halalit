namespace Assets.Enums
{
    public enum EventName
    {
        #region Items
        PLAYER_UPGRADE_PICKUP,
        PLAYER_ATTACK_ITEM_PICKUP,
        NEW_ITEM_DROP,
        NO_STOCK,
        #endregion

        #region Valuables
        NEW_VALUABLE_DROP,
        PLAYER_VALUABLE_PICKUP,
        #endregion

        #region Attacks
        NEW_ATTACK_SWITCH,
        #endregion

        #region Instantiations
        ASTEROID_INTERNAL_INSTANTIATION,
        #endregion

        #region Deaths
        ASTEROID_DEATH,
        HALALIT_DEATH,
        ENEMY_DEATH,
        #endregion

        #region Explosions
        ENEMY_EXPLOSION,
        #endregion
    }
}
