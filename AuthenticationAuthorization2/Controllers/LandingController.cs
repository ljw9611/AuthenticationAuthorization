using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAuthorization2.Controllers
{
    [AllowAnonymous]
    public class LandingController : Controller
    {
        public IActionResult Index() => Content("누구나 접근 가능");

        public IActionResult Greeting()
        {
            var roleName = HttpContext.User.IsInRole("Admin") ? "관리자" : "사용자";
            return Content($"{roleName} 님 반갑습니다");
        }
    }
}
