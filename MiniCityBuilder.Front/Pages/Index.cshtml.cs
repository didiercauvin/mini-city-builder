using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniCityBuilder.Orleans.Contracts;
using MiniCityBuilder.Orleans.Grains;
using Newtonsoft.Json.Linq;

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

    public IActionResult OnGet()
    {
        if (User.Identity.IsAuthenticated)
        {
            return Redirect($"/Map?player={User.Identity.Name}");
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!string.IsNullOrEmpty(Login))
        {
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(Login);
            
            var playerDto = await playerGrain.Login(Login, Password);

            if (playerDto != null)
            {
                Response.Cookies.Append("jwt", playerDto.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                return Redirect($"/Map?player={playerDto.UserName}");
            }
        }
        return Page();
    }
}