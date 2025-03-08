using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniCityBuilder.Orleans.Contracts;

namespace MiniCityBuilder.Front.Pages;

public class IndexModel : PageModel
{
    private readonly IClusterClient _grainFactory;

    [BindProperty]
    public string Login { get; set; }
    
    [BindProperty]
    public string Password { get; set; }

    public IndexModel(IClusterClient grainFactory)
    {
        _grainFactory = grainFactory;
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!string.IsNullOrEmpty(Login))
        {
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(Login);
            
            var playerDto = await playerGrain.Login(Login, Password);

            if (playerDto != null)
            {
                HttpContext.Session.SetString("PLAYER", JsonSerializer.Serialize(playerDto));
                return RedirectToPage("/Map");
            }
        }
        return Page();
    }
}