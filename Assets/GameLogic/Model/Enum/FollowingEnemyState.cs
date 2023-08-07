using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum FollowingEnemyState
    {
        SET_WAITING,
        WAITING,
        SET_ATTACKING,
        ATTACKING
    }
}