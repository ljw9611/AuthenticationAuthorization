using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AuthenticationAuthorization2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthentication("Cookies").AddCookie();
            
            var app = builder.Build();
            app.UseAuthentication(); // NET 7 �������� ��� ���� �Ǿ����� ���� ������ �� �� ��� �ʿ�.

            app.MapGet("/", async context => {
                string content = "<h1>ASP .NET Core ������ ���� �ʰ��� �ڵ�</h1>";

                content += "<a href='/Login'>�α���</a><br />";
                content += "<a href='/Info'>����</a><br />";
                content += "<a href='/InfoJson'>����(Json)</a>";

                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                // �ѱ� ���� �ذ�
                await context.Response.WriteAsync(content);
            });

            app.MapGet("/Login", async context =>
            {
                //builder.Services.AddAuthentication("Cookies").AddCookie();

                var claims = new List<Claim>
                {
                    //new Claim(ClaimTypes.Name, "User Name") //
                    new Claim(ClaimTypes.Name, "���̵�")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await context.SignInAsync("Cookies", claimsPrincipal);

                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync("<h3>�α��� �Ϸ�</h3>");
            });

            #region Info
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
            #endregion

            #region InfoJson
            app.MapGet("/InfoJson", async context =>
                {
                    string json = "";

                    if (context.User.Identity.IsAuthenticated)
                    {   // ���� �Է��ؼ� ���� ���
                        //Json += "{\"type\": \"Name\", \"value\": \"User Name\"}";

                        // Json ��ȯ���� ���� ���
                        // claims �迭�� ClaimDto�� �ʵ带 ����
                        var claims = context.User.Claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value});
                        //Json ���� ��ȯ�ϱ� + ���ڵ�x �ѱ� ȣȯ�ǰ� ����
                        json += JsonSerializer.Serialize<IEnumerable<ClaimDto>>(
                            claims, new JsonSerializerOptions { 
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                            });

                    }
                    else
                    {
                        json += "<h3>�α������� �ʾҽ��ϴ�.<h3>";
                    }
                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(json);
                }); 
            #endregion

            app.Run();
        }
        public class ClaimDto // DTO : Data Transfer Object
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}