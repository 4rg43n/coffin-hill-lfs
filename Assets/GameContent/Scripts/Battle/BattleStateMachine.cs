using System;

public class BattleStateMachine
{
    public BattleState CurrentState { get; private set; } = BattleState.Idle;
    public event Action<BattleState> OnStateChanged;

    public void TransitionToST(BattleState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
