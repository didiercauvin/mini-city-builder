using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using MiniCityBuilder.Orleans.Contracts;
using MiniCityBuilder.Orleans.Grains.Helpers;
using Orleans.Concurrency;
using SignalR.Orleans.Core;

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
    private readonly IGrainFactory _grainFactory;
    private HubContext<NotificationHub> _hubContext;

    public PlayerGrain([PersistentState("player", "playerStore")] IPersistentState<PlayerState>  playerState,
        IPasswordHashHelper passwordHashHelper,
        JwtTokenGenerator  jwtTokenGenerator,
        IGrainFactory grainFactory)
    {
        _playerState = playerState;
        _passwordHashHelper = passwordHashHelper;
        _jwtTokenGenerator = jwtTokenGenerator;
        _grainFactory = grainFactory;
        //_hubContext = hubContext;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _hubContext = _grainFactory.GetHub<NotificationHub>();

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

        // Send a message to a single client
        await _hubContext.Group("players").Send("PlayerJoined", $"Joueur {username} connecté");

        var userManager = _grainFactory.GetGrain<IUserManagerGrain>(0);
        await userManager.RegisterUser(username);

        return playerDto;
    }
}