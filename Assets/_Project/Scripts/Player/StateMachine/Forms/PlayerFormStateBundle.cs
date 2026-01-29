using System.Collections.Generic;

public class PlayerFormStateBundle
{
    private readonly Dictionary<PlayerStates, IPlayerState> states;

    public PlayerFormStateBundle(IPlayerState defaultState, Dictionary<PlayerStates, IPlayerState> states)
    {
        DefaultState = defaultState;
        this.states = states ?? new Dictionary<PlayerStates, IPlayerState>();
    }

    public IPlayerState DefaultState { get; }

    public IPlayerState GetStateOrDefault(PlayerStates stateId)
    {
        if (states != null && states.TryGetValue(stateId, out var state))
        {
            return state;
        }

        return null;
    }
}
