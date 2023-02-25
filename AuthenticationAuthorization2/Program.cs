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
            #region ConfigureServices 영역
            //builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            //builder.Services.AddAuthentication("Cookies").AddCookie(); 
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(); // Cookies의 상수를 갖고 있음

            #endregion
            var app = builder.Build();

            app.UseRouting();
            app.UseAuthentication(); // NET 7 버전에는 없어도 실행 되었지만 이전 버전이 안 될 경우 필요.
            app.UseAuthorization();

            #region Menu
            app.MapGet("/", async context =>
            {
                string content = "<h1>ASP .NET Core 인증과 권한 초간단 코드</h1>";

                content += "<a href='/Login'>로그인</a><br />";
                content += "<a href='/Login/User'>로그인(User)</a><br />";
                content += "<a href='/Login/Admin'>로그인(Admin)</a><br />";
                content += "<a href='/Info'>정보</a><br />";
                content += "<a href='/InfoDetails'>정보(Details)</a><br />";
                content += "<a href='/InfoJson'>정보(Json)</a><br />";
                content += "<a href='/Logout'>로그아웃</a><br />";
                content += "<a href='/Landing/Index'>랜딩페이지</a><br />";
                content += "<a href='/Landing/Greeting'>환영페이지</a><br />";
                content += "<a href='/Dashboard'>관리자페이지</a><br />";
                content += "<a href='/api/AuthService'>로그인 정보(JSON)</a><br />";

                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                // 한글 깨짐 해결
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
                            new Claim("원하는 이름", "원하는 값")
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
                    // AuthenticationProperties { IsPersistent = true } - 웹 브라우저를 닫고 다시 열었을 때 쿠키를 소멸시키지 않고 영구 저장하는 기능

                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("<h3>로그인 완료</h3>");
                }); 
            #endregion

            #region Login
            app.MapGet("/Login", async context =>
                {

                    var claims = new List<Claim>
                    {
                    //new Claim(ClaimTypes.Name, "User Name") //
                    new Claim(ClaimTypes.Name, "아이디")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("<h3>로그인 완료</h3>");
                }); 
            #endregion

            #region Info
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
            #endregion

            #region InfoDetails
            app.MapGet("/InfoDetails", async context =>
            {
                string result = "";

                if (context.User.Identity.IsAuthenticated)
                {   // User Name을 가져옴 (30 Line)
                    result += $"<h3>로그인 이름: {context.User.Identity.Name}</h3>";
                    foreach (var claim in context.User.Claims)
                    {
                        result += $"{claim.Type} = {claim.Value}<br />";
                    }
                    if (context.User.IsInRole("Admin") && context.User.IsInRole("Users"))
                    {
                        result += "<br />Admin + Users 권한이 있습니다.<br />";
                    }
                }
                else
                {
                    result += "<h3>로그인하지 않았습니다.<h3>";
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
                    {   // 직접 입력해서 했을 경우
                        //Json += "{\"type\": \"Name\", \"value\": \"User Name\"}";

                        // Json 변환으로 했을 경우
                        // claims 배열에 ClaimDto의 필드를 저장
                        var claims = context.User.Claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value});
                        //Json 으로 변환하기 + 인코딩x 한글 호환되게 설정
                        json += JsonSerializer.Serialize<IEnumerable<ClaimDto>>(
                            claims, new JsonSerializerOptions { 
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                            });

                    }
                    else
                    {
                        json += "<h3>로그인하지 않았습니다.<h3>";
                    }

                    // MIME 타입
                    context.Response.Headers.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(json);
                });
            #endregion

            #region 로그아웃
            app.MapGet("/Logout", async context =>
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("<h3>로그아웃 완료</h3>");
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