using Microsoft.AspNetCore.Mvc;
using SistemaMonitoreoRedes.Helpers;

namespace SistemaMonitoreoRedes.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            if (AuthHelper.EstaLogueado(HttpContext.Session))
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string password)
        {
            if (AuthHelper.ValidarCredenciales(usuario, password))
            {
                AuthHelper.IniciarSesion(HttpContext.Session);
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        public IActionResult Logout()
        {
            AuthHelper.CerrarSesion(HttpContext.Session);
            return RedirectToAction("Login");
        }
    }
}
