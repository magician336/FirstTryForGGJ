using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PlayerStateMachineTests
{
    private GameObject playerObject;
    private PlayerStateMachine playerStateMachine;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject();
        playerStateMachine = playerObject.AddComponent<PlayerStateMachine>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void InitialState_ShouldBeIdleState()
    {
        Assert.IsInstanceOf<PlayerIdleState>(playerStateMachine.CurrentState);
    }

    [Test]
    public void ChangeState_ToRunState_ShouldTransitionCorrectly()
    {
        playerStateMachine.ChangeState<PlayerRunState>();
        Assert.IsInstanceOf<PlayerRunState>(playerStateMachine.CurrentState);
    }

    [Test]
    public void ChangeState_ToJumpState_ShouldTransitionCorrectly()
    {
        playerStateMachine.ChangeState<PlayerJumpState>();
        Assert.IsInstanceOf<PlayerJumpState>(playerStateMachine.CurrentState);
    }

    [Test]
    public void ChangeState_ToFallState_ShouldTransitionCorrectly()
    {
        playerStateMachine.ChangeState<PlayerFallState>();
        Assert.IsInstanceOf<PlayerFallState>(playerStateMachine.CurrentState);
    }

    [Test]
    public void ChangeState_ToInteractState_ShouldTransitionCorrectly()
    {
        playerStateMachine.ChangeState<PlayerInteractState>();
        Assert.IsInstanceOf<PlayerInteractState>(playerStateMachine.CurrentState);
    }

    [Test]
    public void Update_ShouldCallCurrentStateUpdate()
    {
        var mockState = new MockPlayerState();
        playerStateMachine.ChangeState(mockState);
        playerStateMachine.Update();

        Assert.IsTrue(mockState.UpdateCalled);
    }

    private class MockPlayerState : IPlayerState
    {
        public bool UpdateCalled { get; private set; }

        public void Enter() { }

        public void Update()
        {
            UpdateCalled = true;
        }

        public void Exit() { }
    }
}