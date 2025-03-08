using MiniCityBuilder.Orleans.Contracts;

namespace MiniCityBuilder.Orleans.Grains;

public class PlayerState
{
    public bool Exists { get; set; }
    public string UserName { get; set; }
}

public class PlayerGrain : Grain, IPlayerGrain 
{
    private readonly IPersistentState<PlayerState> _playerState;

    public PlayerGrain([PersistentState("player", "playerStore")] IPersistentState<PlayerState>  playerState)
    {
        _playerState = playerState;
    }
    
    public async Task<PlayerDto?> Login(string username, string password)
    {
        if (!_playerState.State.Exists)
        {
            _playerState.State.UserName = username;
            _playerState.State.Exists = true;
        }

        await _playerState.WriteStateAsync();

        return new PlayerDto
        {
            UserName = username
        };
    }
}