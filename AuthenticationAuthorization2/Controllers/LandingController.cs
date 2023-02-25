using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAuthorization2.Controllers
{
    [AllowAnonymous] // Authorize 있어도 누구나 접근 가능
    public class LandingController : Controller
    {
        public IActionResult Index() => Content("누구나 접근 가능");
        [Authorize] // 인증 정보가 없을 경우 또는 인증되지 않은 사용자는 접근 불가능
        [Route("/Greeting")] // 해당 경로로만 들어갔을 경우 누구나 접근 가능
        public IActionResult Greeting()
        {
            var roleName = HttpContext.User.IsInRole("Admin") ? "관리자" : "사용자";
            return Content($"{roleName} 님 반갑습니다");
        }
    }
    
}
