using AdvantageTool.Lti;
using AdvantageTool.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace AdvantageTool.Controllers
{
    [Authorize]
    public class ToolController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ToolController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);
            var response = new ToolResponseModel
            {
                LtiRequest = new LtiResourceLinkRequest(token.Payload),
                IdToken = idToken,
                JwtHeader = token.Header
            };

            return View(response);
        }
    }
}
