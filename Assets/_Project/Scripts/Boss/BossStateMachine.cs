public class BossStateMachine
{
    public IBossState CurrentState { get; private set; }
    public void Initialize(IBossState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }
    public void ChangeState(IBossState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}