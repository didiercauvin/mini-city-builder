namespace MiniCityBuilder.Orleans.Contracts;

[GenerateSerializer]
public class PlayerDto
{
    [Id(0)]
    public string UserName { get; set; }
    [Id(1)]
    public string Token { get; set; }  
}

public interface IPlayerGrain : IGrainWithStringKey
{
    Task<PlayerDto> Login(string username, string password);
}