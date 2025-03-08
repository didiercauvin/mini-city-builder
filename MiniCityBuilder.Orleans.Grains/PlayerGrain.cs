using MiniCityBuilder.Orleans.Contracts;
using MiniCityBuilder.Orleans.Grains.Helpers;

namespace MiniCityBuilder.Orleans.Grains;

public class PlayerState
{
    public bool Exists { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
}

public class PlayerGrain : Grain, IPlayerGrain 
{
    private readonly IPersistentState<PlayerState> _playerState;
    private readonly IPasswordHashHelper _passwordHashHelper;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public PlayerGrain([PersistentState("player", "playerStore")] IPersistentState<PlayerState>  playerState,
        IPasswordHashHelper passwordHashHelper,
        JwtTokenGenerator  jwtTokenGenerator)
    {
        _playerState = playerState;
        _passwordHashHelper = passwordHashHelper;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    
    public async Task<PlayerDto?> Login(string username, string password)
    {
        if (!_playerState.State.Exists)
        {
            _playerState.State.UserName = username;
            _playerState.State.Exists = true;
            
            var (salt, hashed) = _passwordHashHelper.CalculateHash(password);
            _playerState.State.PasswordSalt = salt;
            _playerState.State.PasswordHash = hashed;
        }
        else
        {
            var (_, hashed) = _passwordHashHelper.CalculateHash(password);
            if (_playerState.State.PasswordHash != hashed)
            {
                throw new Exception("Can't login");
            }
        }

        await _playerState.WriteStateAsync();

        var token  = _jwtTokenGenerator.GenerateToken(username);
        
        return new PlayerDto
        {
            UserName = username,
            Token = ""
        };
    }
}