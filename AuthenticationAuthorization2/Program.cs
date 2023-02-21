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
                string content = "<h1>ASP .NET Core ������ ���� �ʰ��� �ڵ�</h1>";
                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                // �ѱ� ���� �ذ�
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
                await context.Response.WriteAsync("<h3>�α��� �Ϸ�</h3>");
            });
            app.MapGet("/Info", async context =>
            {
                string result = "";

                if (context.User.Identity.IsAuthenticated)
                {   // User Name�� ������ (30 Line)
                    result += $"<h3>�α��� �̸�: {context.User.Identity.Name}</h3>";
                }
                else
                {
                    result += "<h3>�α������� �ʾҽ��ϴ�.<h3>";
                }
                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(result, Encoding.Default);
            });
            app.Run();
        }
    }
}