using GearShop.Contracts;
using GearShop.Models.Dto.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers
{
    public class LoginController : Controller
    {
        private readonly IIdentityService _identity;
        private readonly IGearShopRepository _gearShopRepository;

        public LoginController(IIdentityService identity, IGearShopRepository gearShopRepository)
        {
            _identity = identity;
            _gearShopRepository = gearShopRepository;
        }

        public IActionResult Authentication()
        {
            return View();
        }

        /// <summary>
        /// Получение логина и пароля от пользователя.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Authentication(AuthData request)
        {
            var token = _identity.Login(request.Username, request.Password);
            if (token == null)
            {
                ModelState.AddModelError("", _identity.LastError);
                return View(request);
            }

            HttpContext.Session.SetString("JWToken", token);
            return RedirectToAction("index", "Admin");
        }
    }
}