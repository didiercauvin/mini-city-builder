using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiniCityBuilder.Front.Pages;

public class Login : PageModel
{
    [BindProperty]
    public string Pseudo { get; set; }

    public IActionResult OnPost()
    {
        if (!string.IsNullOrEmpty(Pseudo))
        {
            // Stocker le pseudo dans la session ou l'envoyer au serveur
            return RedirectToPage("/Map");
        }
        return Page();
    }
}