using AdvantageTool.Lti;
using AdvantageTool.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace AdvantageTool.Controllers
{
    [Authorize]
    public class ToolController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var idToken = await HttpContext.GetTokenAsync(OpenIdConnectResponseType.IdToken);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(idToken);

            var response = new ToolResponseModel
            {
                IdToken = idToken,
                LtiRequest = new LtiResourceLinkRequest(jwt.Payload),
                JwtHeader = jwt.Header,
            };

            return View(response);
        }
    }
}
