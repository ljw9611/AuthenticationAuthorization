using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
            #region ConfigureServices ����
            //builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            //builder.Services.AddAuthentication("Cookies").AddCookie(); 
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(); // Cookies�� ����� ���� ����

            #endregion
            var app = builder.Build();

            app.UseRouting();
            app.UseAuthentication(); // NET 7 �������� ��� ���� �Ǿ����� ���� ������ �� �� ��� �ʿ�.
            app.UseAuthorization();

            #region Menu
            app.MapGet("/", async context =>
            {
                string content = "<h1>ASP .NET Core ������ ���� �ʰ��� �ڵ�</h1>";

                content += "<a href='/Login'>�α���</a><br />";
                content += "<a href='/Login/User'>�α���(User)</a><br />";
                content += "<a href='/Login/Admin'>�α���(Admin)</a><br />";
                content += "<a href='/Info'>����</a><br />";
                content += "<a href='/InfoDetails'>����(Details)</a><br />";
                content += "<a href='/InfoJson'>����(Json)</a><br />";
                content += "<a href='/Logout'>�α׾ƿ�</a><br />";
                content += "<a href='/Landing/Index'>����������</a><br />";
                content += "<a href='/Landing/Greeting'>ȯ��������</a><br />";
                content += "<a href='/Dashboard'>������������</a><br />";
                content += "<a href='/api/AuthService'>�α��� ����(JSON)</a><br />";

                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                // �ѱ� ���� �ذ�
                await context.Response.WriteAsync(content);
            }); 
            #endregion

            #region /Login/{Username}
            app.MapGet("/Login/{Username}", async context =>
                {
                    
                    var username = context.Request.RouteValues["Username"].ToString();
                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, username),
                            new Claim(ClaimTypes.Name, username),
                            new Claim(ClaimTypes.Email, username + "@a.com"),
                            new Claim(ClaimTypes.Role, "Users"),
                            new Claim("���ϴ� �̸�", "���ϴ� ��")
                        };

                    if (username == "Admin")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        claimsPrincipal, 
                        new AuthenticationProperties { IsPersistent = true });
                    // AuthenticationProperties { IsPersistent = true } - �� �������� �ݰ� �ٽ� ������ �� ��Ű�� �Ҹ��Ű�� �ʰ� ���� �����ϴ� ���

                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("<h3>�α��� �Ϸ�</h3>");
                }); 
            #endregion

            #region Login
            app.MapGet("/Login", async context =>
                {

                    var claims = new List<Claim>
                    {
                    //new Claim(ClaimTypes.Name, "User Name") //
                    new Claim(ClaimTypes.Name, "���̵�")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("<h3>�α��� �Ϸ�</h3>");
                }); 
            #endregion

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

            #region InfoDetails
            app.MapGet("/InfoDetails", async context =>
            {
                string result = "";

                if (context.User.Identity.IsAuthenticated)
                {   // User Name�� ������ (30 Line)
                    result += $"<h3>�α��� �̸�: {context.User.Identity.Name}</h3>";
                    foreach (var claim in context.User.Claims)
                    {
                        result += $"{claim.Type} = {claim.Value}<br />";
                    }
                    if (context.User.IsInRole("Admin") && context.User.IsInRole("Users"))
                    {
                        result += "<br />Admin + Users ������ �ֽ��ϴ�.<br />";
                    }
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

                    // MIME Ÿ��
                    context.Response.Headers.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(json);
                });
            #endregion

            #region �α׾ƿ�
            app.MapGet("/Logout", async context =>
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("<h3>�α׾ƿ� �Ϸ�</h3>");
                });
            #endregion

            //app.MapControllerRoute(name: "default",pattern: "{controller=Landing}/{action=Index}");
            app.MapDefaultControllerRoute();
            app.Run();

        }
        public class ClaimDto // DTO : Data Transfer Object
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        
    }
}