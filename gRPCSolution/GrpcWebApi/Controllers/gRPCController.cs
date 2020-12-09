using Grpc.Core;
using GrpcServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GrpcWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class gRPCController : ControllerBase
    {
        private readonly Greeter.GreeterClient greeterClient;

        public gRPCController(Greeter.GreeterClient greeterClient)
        {
            this.greeterClient = greeterClient;
        }

        public async Task<IActionResult> Get(string token)
        {
            Console.WriteLine("发送异步请求");
            var headers = new Metadata { { "Authorization", $"Bearer {token}" } };

            var reply = await greeterClient.SayHelloAsync(new HelloRequest { Name = "发送异步请求gRPC" }, headers: headers);

            Console.WriteLine("发送同步请求");
            reply = greeterClient.SayHello(new HelloRequest { Name = "发送同步请求gRPC" }, headers: headers);
            return Ok(reply);
        }

        [HttpGet("login")]
        public IActionResult Login(string loginName, string loginPwd)
        {
            if (loginName == "admin" && loginPwd == "password")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, loginName),
                    new Claim(ClaimTypes.Email, "iserviceStudio@outlook.com")
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("www.iservice.com"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: "www.iservice.com",
                    audience: "www.iservice.com",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    //notBefore: DateTime.Now.AddMilliseconds(1),//一秒后生效
                    signingCredentials: creds);
                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            return NotFound("用户名或密码不正确");
        }
    }
}