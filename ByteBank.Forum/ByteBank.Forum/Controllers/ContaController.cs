using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {
        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var contextoOwin = HttpContext.GetOwinContext();
                    _userManager = contextoOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
                }

                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }



        // GET: Conta
        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (ModelState.IsValid)
            {
                var novoUsuario = new UsuarioAplicacao();
                novoUsuario.Email = model.Email;
                novoUsuario.UserName = model.UserName;
                novoUsuario.NomeCompleto = model.NomeCompleto;

                var usuario = await UserManager.FindByEmailAsync(model.Email);
                var usuarioJaExiste = usuario != null;

                if (usuarioJaExiste) return View("AguardandoConfirmacao");

                var resultado = await UserManager.CreateAsync(novoUsuario, model.Senha);

                if (resultado.Succeeded)
                {
                    await EviarEmailDeConfirmacao(novoUsuario);
                    return View("AguardandoConfirmacao");
                }
                else
                    AdicionarErros(resultado);
            }

            return View(model);
        }

        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {
            if (usuarioId == null || token == null) return View("Error");

            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
        }

        private async Task EviarEmailDeConfirmacao(UsuarioAplicacao usuario)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

            var linkDeCallBack =
                Url.Action(
                    "ConfirmacaoEmail",
                    "Conta",
                    new { usuarioId = usuario.Id, token = token },
                    Request.Url.Scheme);

            await UserManager.SendEmailAsync(
                usuario.Id,
                "Fórum ByteBank - Confirmação de E-mail",
                $"Bem vindo ao fórum ByteBank, clique aqui {linkDeCallBack} para confirmar seu e-mail!");
        }

        private void AdicionarErros(IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
        }
    }
}