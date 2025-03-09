using Microsoft.AspNetCore.SignalR;
using MiniCityBuilder.Orleans.Contracts;
using MiniCityBuilder.Orleans.Grains.Helpers;
using Orleans.Streams;

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
    private readonly IClusterClient _client;
    private readonly IHubContext<NotificationHub> _hubContext;

    public PlayerGrain([PersistentState("player", "playerStore")] IPersistentState<PlayerState>  playerState,
        IPasswordHashHelper passwordHashHelper,
        JwtTokenGenerator  jwtTokenGenerator,
        IClusterClient client,
        IHubContext<NotificationHub> hubContext)
    {
        _playerState = playerState;
        _passwordHashHelper = passwordHashHelper;
        _jwtTokenGenerator = jwtTokenGenerator;
        _client = client;
        _hubContext = hubContext;
    }
    
    private IAsyncStream<PlayerDto> _stream;
    
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("game");
        _stream = streamProvider.GetStream<PlayerDto>("players", new Guid("c3b9c03e-6d4c-4d61-a376-4f4c26d3bd76"));
        return base.OnActivateAsync(cancellationToken);
    }
    
    public async Task<PlayerDto> Login(string username, string password)
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

        var playerDto = new PlayerDto
        {
            UserName = username,
            Token = token
        };

        await _stream.OnNextAsync(playerDto);
        
        return playerDto;
    }
}