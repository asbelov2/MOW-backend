using API.Services;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AccountController : Controller
  {
    private ApplicationContext context;

    public AccountController(ApplicationContext context)
    {
      Debug.WriteLine("[Account controller] Init start");
      this.context = context;
      Debug.WriteLine("[Account controller] Init end");
    }

    [HttpPost("/token")]
    public async Task<IActionResult> Token([FromForm] string email, [FromForm] string password)
    {
      Debug.WriteLine("[Account controller] Get token start");
      var identity = await GetIdentity(email, password);
      if (identity == null)
      {
        return BadRequest("Invalid username or password.");
      }

      var now = DateTime.UtcNow;
      // создаем JWT-токен
      var jwt = new JwtSecurityToken(
              issuer: AuthOptions.ISSUER,
              audience: AuthOptions.AUDIENCE,
              notBefore: now,
              claims: identity.Claims,
              expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
              signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
      var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

      var response = new
      {
        access_token = encodedJwt,
        username = identity.Name,
        id = context.Users.FirstOrDefaultAsync(x => x.Email == identity.Name).Result.Id
      };
      Debug.WriteLine("[Account controller] Get token start");
      return Json(response);
    }

    private async Task<ClaimsIdentity> GetIdentity(string email, string password)
    {
      User person = await context.Users.FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
      if (person != null)
      {
        var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, person.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Nickname)
                };
        ClaimsIdentity claimsIdentity =
        new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        return claimsIdentity;
      }
      // если пользователь не найден
      return null;
    }
  }
}