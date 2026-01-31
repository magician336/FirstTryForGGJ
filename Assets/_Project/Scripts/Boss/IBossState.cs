public interface IBossState
{
    void Enter();
    void LogicUpdate();
    void PhysicsUpdate();
    void Exit();
}