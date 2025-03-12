using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniCityBuilder.Orleans.Contracts;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace MiniCityBuilder.Front.Pages;

public class Map : PageModel
{
    [BindProperty]
    public string Username { get; set; }
    
    public IActionResult OnGet()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/Index");
        }

        var userNameFromToken = User.Identity.Name;
        var userNameFromQuery = Request.Query["player"];

        //if (userNameFromToken != userNameFromQuery)
        //{
        //    return Redirect("/Index");
        //}


        // Récupère le joueur depuis la session
        //var playerJson = HttpContext.Session.GetString("PLAYER");
        //if (string.IsNullOrEmpty(playerJson))
        //{
        //    return RedirectToPage("/Index"); // Redirige si pas de session
        //}

        //// Désérialise l'objet
        //var player = JsonSerializer.Deserialize<PlayerDto>(playerJson);

        //// Vérifie si le token est valide
        //if (!IsTokenValid(player.Token))
        //{
        //    HttpContext.Session.Remove("PLAYER"); // Supprime la session expirée
        //    return RedirectToPage("/Index"); // Redirige vers la page de login
        //}

        //Username = player.UserName;
        return Page();
    }

    private bool IsTokenValid(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo > DateTime.UtcNow; // Vérifie l'expiration
        }
        catch
        {
            return false; // Token invalide
        }
    }
}