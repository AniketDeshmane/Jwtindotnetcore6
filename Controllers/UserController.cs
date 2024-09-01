using APIwithSQLLite.Data;
using APIwithSQLLite.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIwithSQLLite.Controllers
{
 

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        public UserController(DataContext dataContext,IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration = configuration;
        }


        [HttpPost]
        [Route("Signup")]
        public async Task<IActionResult> Signup([FromBody] UserDetails user)
        {
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            return Ok("User created successfully");

        }

        [HttpGet]
        [Route("Signin")]
        public async Task<IActionResult> Signin([FromBody] UserDetails user)
        {
            var userinfo = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName && x.Password == user.Password);
            if (userinfo == null)
            {
                return NotFound("Incorrect Username or Password"); // Return 404 if the user is not found
            }

            //Add logic to generate the jwt token 
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,_configuration["JWT:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),

                new Claim("UserID", userinfo.UserID.ToString()),
                new Claim("UserName", userinfo.UserName.ToString()),

            };

            var SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]));

            var signInAlgoAndKey = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Audience"],
                claims,
                signingCredentials: signInAlgoAndKey,
                expires: DateTime.UtcNow.AddMinutes(10)
            );

            var JwtTokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new {JwtTokenValue, userinfo}); // Return 200 OK status
        }
    }
}
