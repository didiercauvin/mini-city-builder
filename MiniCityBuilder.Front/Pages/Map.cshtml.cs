using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniCityBuilder.Orleans.Contracts;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MiniCityBuilder.Front.Pages;

public class Map : PageModel
{
    [BindProperty]
    public string Username { get; set; }
    
    public void OnGet()
    {
        var playerJson = HttpContext.Session.GetString("PLAYER");
        if (playerJson != null)
        {
            var playerDto = JsonSerializer.Deserialize<PlayerDto>(playerJson);
            Username = playerDto.UserName;
            // Utiliser playerDto
        }
    }
}