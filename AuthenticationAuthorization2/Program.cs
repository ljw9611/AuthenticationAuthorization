using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text;

namespace AuthenticationAuthorization2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthentication("Cookies").AddCookie();
            
            var app = builder.Build();
            app.UseAuthentication();

            app.MapGet("/", async context => {
                string content = "<h1>ASP .NET Core 인증과 권한 초간단 코드</h1>";
                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                // 한글 깨짐 해결
                await context.Response.WriteAsync(content);
            });

            app.MapGet("/Login", async context =>
            {
                //builder.Services.AddAuthentication("Cookies").AddCookie();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "User Name") //
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await context.SignInAsync("Cookies", claimsPrincipal);

                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync("<h3>로그인 완료</h3>");
            });
            app.MapGet("/Info", async context =>
            {
                string result = "";

                if (context.User.Identity.IsAuthenticated)
                {   // User Name을 가져옴 (30 Line)
                    result += $"<h3>로그인 이름: {context.User.Identity.Name}</h3>";
                }
                else
                {
                    result += "<h3>로그인하지 않았습니다.<h3>";
                }
                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(result, Encoding.Default);
            });
            app.Run();
        }
    }
}