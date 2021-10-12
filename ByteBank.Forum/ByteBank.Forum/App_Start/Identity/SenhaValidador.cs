using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    public class SenhaValidador : IIdentityValidator<string>
    {
        public int TamanhoRequerido { get; set; }
        public bool ObrigatorioCaracteresEspeciais { get; set; }
        public bool ObrigatorioLowerCase { get; set; }
        public bool ObrigatorioUpeprCase { get; set; }
        public bool ObrigatorioDigistos { get; set; }

        public async Task<IdentityResult> ValidateAsync(string item)
        {
            var erros = new List<string>();

            if (ObrigatorioCaracteresEspeciais && !VerificaCaracteresEspeciais(item))
                erros.Add("A senha deve conter caracteres especiais");

            if (!VerificaTamanhoRequerido(item))
                erros.Add($"A senha deve conter no mínimo {TamanhoRequerido} caracteres");

            if(ObrigatorioLowerCase && !VerificarLowerCase(item))
                erros.Add($"A senha deve conter no mínimo uma letra minúscula");

            if (ObrigatorioUpeprCase && !VerificarUpperCase(item))
                erros.Add($"A senha deve conter no mínimo uma letra maiúscula");

            if (ObrigatorioDigistos && !VerificarDigitos(item))
                erros.Add($"A senha deve conter no mínimo um dígito");

            if (erros.Any())
                return IdentityResult.Failed(erros.ToArray());
            else
                return IdentityResult.Success;
        }

        private bool VerificaTamanhoRequerido(string senha) =>
           senha?.Length >= TamanhoRequerido;

        private bool VerificaCaracteresEspeciais(string senha) =>
            Regex.IsMatch(senha, @"[~`!@#$%^&*()+=|\\{}':;.,<>/?[\]""_-]");

        private bool VerificarUpperCase(string senha) =>
            senha.Any(char.IsUpper);

        private bool VerificarLowerCase(string senha) =>
            senha.Any(char.IsLower);

        private bool VerificarDigitos(string senha) =>
            senha.Any(char.IsDigit);
    }
}