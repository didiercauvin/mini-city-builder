using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniCityBuilder.Orleans.Contracts;

namespace MiniCityBuilder.Front.Pages;

public class IndexModel : PageModel
{
    private readonly IClusterClient _grainFactory;

    [BindProperty]
    public string Pseudo { get; set; }

    public IndexModel(IClusterClient grainFactory)
    {
        _grainFactory = grainFactory;
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!string.IsNullOrEmpty(Pseudo))
        {
            var playerGrain = _grainFactory.GetGrain<IPlayerGrain>(Pseudo);
            
            var playerDto = await playerGrain.Login(Pseudo, "");

            if (playerDto != null)
            {
                //HttpContext.Session.SetString("PLAYER", JsonSerializer.Serialize(playerDto));
                return RedirectToPage("/Map");
            }
        }
        return Page();
    }
}