using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GearShop.Contracts;
using GearShop.Models.Dto.Authentication;
using Google.Apis.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;

namespace GearShop.Controllers
{
    public class LoginController : Controller
    {
	    private readonly IConfiguration _configuration;
	    private readonly IIdentityService _identity;
        private readonly IJwtAuth _jwtAuth;
        private readonly IGoogleAuth _googleAuth;
        private readonly IVkAuth _vkAuth;

        public LoginController(IConfiguration configuration, IIdentityService identity, IJwtAuth jwtAuth, IGoogleAuth googleAuth,
	        IVkAuth vkAuth)
        {
	        _configuration = configuration;
	        _identity = identity;
			_jwtAuth = jwtAuth;
            _googleAuth = googleAuth;
            _vkAuth = vkAuth;
        }

        public IActionResult Authentication()
        {
	        ViewData["googleAuthClientId"] = _configuration["GoogleAuth:ClientId"];
	        ViewData["vkAppId"] = _configuration["VkAuth:AppId"];

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
		
        public IActionResult Logout()
        {
	        HttpContext.Session.Clear();
	        return RedirectToAction("Index", "ProductList");
		}

		[AllowAnonymous]
		[HttpPost]
        public async Task<IActionResult> GoogleLogin(string token)
        {
	        string jwt = await _googleAuth.Authorization(token);
			if (token == null)
			{
				return BadRequest();
			}

			HttpContext.Session.SetString("JWToken", token);
	        return Ok();
		}
		
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> VkLogin(string token)
        {
	        string jwt = await _vkAuth.Authorization(token);
	        if (token == null)
	        {
		        return BadRequest();
	        }

	        HttpContext.Session.SetString("JWToken", token);
			return Ok();
		}

        /// <summary>
		/// Возвращает информацию о пользователе.
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]
        public IActionResult GetAccountInfo()
		{
			AccountInfoDto info = new AccountInfoDto()
			{
				Name = "Гость",
				Email = "",
				PictureUrl = ""
			};

			var jwt = HttpContext.Session.GetString("JWToken");

			if (jwt == null)
			{
				return Ok(info);
			}

			//Авторизация ВК.
			if (jwt.Contains("uid") && jwt.Contains("first_name"))
			{
				info = _vkAuth.GetUserInfo(jwt);
				return Ok(info);
			}

			info.IsAuth = true;
			JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);

			if (token.Audiences.Any()) //Пришел через гугл.
			{
				info = _googleAuth.GetUserInfoFromJwt(token);
				return Ok(info);
			}

			info = _jwtAuth.GetUserInfoFromJwt(token);

			return Ok(info);
		}
    }
}