using System.ComponentModel.DataAnnotations;
namespace Previa.ExtranetUserAuthentication.Web.Models
{
    public class CredentialsViewModel
    {
        [Required]
        [Display(Name = "Username", ResourceType = typeof(Resources.ModelResources))]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password", ResourceType = typeof(Resources.ModelResources))]
        public string Password { get; set; }


        public string AuthorizationResult { get; set; }
    }
}