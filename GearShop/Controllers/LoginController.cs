using System.Security.Claims;
using GearShop.Contracts;
using GearShop.Models.Dto.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
	        string returnUrl = "/ProductList";

			return new ChallengeResult(
				GoogleDefaults.AuthenticationScheme,
				new AuthenticationProperties
				{
					RedirectUri = Url.Action(nameof(LoginCallback), new { returnUrl })
				});
        }

        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
	        var authenticateResult = await HttpContext.AuthenticateAsync("External");

	        if (!authenticateResult.Succeeded)
		        return BadRequest(); // TODO: Handle this better.

	        var claimsIdentity = new ClaimsIdentity("Application");

	        claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier));
	        claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.Email));
	        claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.Name));

			await HttpContext.SignInAsync(
		        "Application",
		        new ClaimsPrincipal(claimsIdentity));

	        return LocalRedirect("/");
        }


		[AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
			//ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
			//if (info == null)
			// return RedirectToAction(nameof(Login));

			//var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
			//string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };
			//if (result.Succeeded)
			// return View(userInfo);
			//else
			//{
			// AppUser user = new AppUser
			// {
			//  Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
			//  UserName = info.Principal.FindFirst(ClaimTypes.Email).Value
			// };

			// IdentityResult identResult = await userManager.CreateAsync(user);
			// if (identResult.Succeeded)
			// {
			//  identResult = await userManager.AddLoginAsync(user, info);
			//  if (identResult.Succeeded)
			//  {
			//   await signInManager.SignInAsync(user, false);
			//   return View(userInfo);
			//  }
			// }
			// return AccessDenied();
			//}
			return Ok();
        }
    }
}