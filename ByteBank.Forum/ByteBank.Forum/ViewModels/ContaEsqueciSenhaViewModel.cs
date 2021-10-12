using System.ComponentModel.DataAnnotations;

namespace ByteBank.Forum.ViewModels
{
    public class ContaEsqueciSenhaViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}