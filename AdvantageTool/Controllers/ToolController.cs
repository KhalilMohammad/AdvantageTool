using AdvantageTool.Lti;
using AdvantageTool.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var claims = await _userManager.GetClaimsAsync(user);

            //var options = new JsonSerializerOptions()
            //{
            //    MaxDepth = 0,
            //    IgnoreNullValues = true,
            //    IgnoreReadOnlyProperties = true
            //};

            //return Json(claims, options);

            var response = new ToolResponseModel
            {
                LtiRequest = new LtiResourceLinkRequest(claims),
            };

            return View(response);
        }
    }
}
