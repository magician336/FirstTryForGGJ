using System.Collections.Generic;

public abstract class PlayerFormStateFactory
{
    public abstract PlayerFormType FormType { get; }

    public abstract PlayerFormStateBundle CreateStateBundle(PlayerController controller);

    public virtual void ApplyFormSettings(PlayerController controller)
    {
        controller?.ApplyFormSettings(FormType);
    }

    protected static Dictionary<PlayerStates, IPlayerState> BuildDefaultStateMap(PlayerController controller)
    {
        return new Dictionary<PlayerStates, IPlayerState>
        {
            { PlayerStates.Idle, new PlayerIdleState(controller) },
            { PlayerStates.Run, new PlayerRunState(controller) },
            { PlayerStates.Jump, new PlayerJumpState(controller) },
            { PlayerStates.Fall, new PlayerFallState(controller) },
            { PlayerStates.Interact, new PlayerInteractState(controller) }
        };
    }
}

public static class PlayerFormFactoryRegistry
{
    private static readonly Dictionary<PlayerFormType, PlayerFormStateFactory> Cache = new();

    public static PlayerFormStateFactory GetFactory(PlayerFormType formType)
    {
        if (!Cache.TryGetValue(formType, out var factory))
        {
            factory = formType switch
            {
                PlayerFormType.NormalHead => new NormalHeadFormStateFactory(),
                PlayerFormType.SuperJump => new SuperJumpFormStateFactory(),
                PlayerFormType.Fish => new FishFormStateFactory(),
                _ => null
            };

            if (factory != null)
            {
                Cache[formType] = factory;
            }
        }

        return factory;
    }
}
