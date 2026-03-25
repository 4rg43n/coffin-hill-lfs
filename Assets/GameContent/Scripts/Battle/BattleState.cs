public enum BattleState
{
    Idle,
    Intro,
    PlayerChoose,
    PlayerAction,
    EnemyAction,
    StatusTick,
    CheckFainted,
    BattleEnd,
    CatchAttempt
}

public enum BattleResult
{
    Win,
    Lose,
    Fled,
    Caught
}

public enum CatchResult
{
    Caught,
    Shake3,
    Shake2,
    Shake1,
    Failed
}
