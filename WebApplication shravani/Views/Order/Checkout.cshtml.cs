using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication_shravani.Pages.Order
{
    public class CheckoutModel : PageModel
    {
        public int ProductId { get; set; }

        public void OnGet(int productId)
        {
            ProductId = productId;
        }
    }
}