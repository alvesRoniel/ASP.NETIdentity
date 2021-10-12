using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ByteBank.Forum.ViewModels
{
    public class ContaConfirmacaoAlteracaoSenhaViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public string UsuarioId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Nova Senha")]
        public string NovaSenha { get; set; }
    }
}