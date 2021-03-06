using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
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

        private SignInManager<UsuarioAplicacao, string> _signInManager;
        public SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var contextoOwin = HttpContext.GetOwinContext();
                    _signInManager = contextoOwin.GetUserManager<SignInManager<UsuarioAplicacao, string>>();
                }

                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                var contextoOwin = Request.GetOwinContext();
                return contextoOwin.Authentication;
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

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(ContaLoiginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(model.Email);

                if (usuario == null) return SenhaOuUsuarioInvalidos();

                var signInResult =
                    await SignInManager.PasswordSignInAsync(
                        usuario.UserName,
                        model.Senha,
                        isPersistent: model.ContinuarLogado,
                        shouldLockout: true
                    );

                switch (signInResult)
                {
                    case SignInStatus.Success:

                        if (!usuario.EmailConfirmed)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("AguardandoConfirmacao");
                        }

                        return RedirectToAction("Index", "Home");
                    case SignInStatus.LockedOut:
                        var senhaCorreta =
                            await UserManager.CheckPasswordAsync(
                                usuario,
                                model.Senha);

                        if (senhaCorreta)
                            ModelState.AddModelError("", "A conta está bloqueada");
                        else
                            return SenhaOuUsuarioInvalidos();

                        break;
                    default:
                        return SenhaOuUsuarioInvalidos();
                }
            }

            return View(model);
        }

        public ActionResult EsqueciSenha()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EsqueciSenha(ContaEsqueciSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(model.Email);
                if (usuario != null)
                {
                    //Gera o token de resete da senha
                    var token = await UserManager.GeneratePasswordResetTokenAsync(usuario.Id);

                    var linkDeCallBack =
                           Url.Action(
                               "ConfirmacaoAlteracaoSenha",
                               "Conta",
                               new { usuarioId = usuario.Id, token = token },
                               Request.Url.Scheme);

                    await UserManager.SendEmailAsync(
                        usuario.Id,
                        "Fórum ByteBank - Alteração de senha",
                        $"Bem vindo ao fórum ByteBank, clique aqui {linkDeCallBack} para alterar a sua senha!");
                }

                return View("EmailAlteracaoSenhaEnviado");
            }

            return View();
        }

        public ActionResult ConfirmacaoAlteracaoSenha(string usuarioId, string token)
        {
            var modelo = new ContaConfirmacaoAlteracaoSenhaViewModel
            {
                UsuarioId = usuarioId,
                Token = token
            };

            return View(modelo);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmacaoAlteracaoSenha(ContaConfirmacaoAlteracaoSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resultadoAlteracao =
                    await UserManager.ResetPasswordAsync(
                            model.UsuarioId,
                            model.Token,
                            model.NovaSenha
                        );

                if (resultadoAlteracao.Succeeded)
                    return RedirectToAction("Index", "Home");

                AdicionarErros(resultadoAlteracao);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Logoff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private ActionResult SenhaOuUsuarioInvalidos()
        {
            ModelState.AddModelError("", "Usuário ou senha inválidos!");
            return View("Login");
        }

        private void AdicionarErros(IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
        }
    }
}