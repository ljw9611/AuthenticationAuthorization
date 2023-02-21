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
            app.UseAuthentication(); // NET 7 버전에는 없어도 실행 되었지만 이전 버전이 안 될 경우 필요.

            app.MapGet("/", async context => {
                string content = "<h1>ASP .NET Core 인증과 권한 초간단 코드</h1>";

                content += "<a href='/Login'>로그인</a><br />";
                content += "<a href='/Info'>정보</a><br />";
                content += "<a href='/InfoJson'>정보(Json)</a>";

                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                // 한글 깨짐 해결
                await context.Response.WriteAsync(content);
            });

            app.MapGet("/Login", async context =>
            {
                //builder.Services.AddAuthentication("Cookies").AddCookie();

                var claims = new List<Claim>
                {
                    //new Claim(ClaimTypes.Name, "User Name") //
                    new Claim(ClaimTypes.Name, "아이디")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await context.SignInAsync("Cookies", claimsPrincipal);

                context.Response.Headers.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync("<h3>로그인 완료</h3>");
            });

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