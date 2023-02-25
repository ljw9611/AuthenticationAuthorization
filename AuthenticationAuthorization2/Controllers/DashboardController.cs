using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAuthorization2.Controllers
{
    [Authorize(Roles = "Admin")] // Admin 인증된 사용자만 접근 가능
    public class DashboardController : Controller
    {
        public IActionResult Index() => Content("관리자 님, 반갑습니다.");
    }
}
