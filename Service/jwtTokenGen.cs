
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using userPanelOMR.model;

namespace userPanelOMR.Service
{
    public class jwtTokenGen
    {
        public string GenerateJwtToken(SingUps emp)
        {
            // JWT token generate karne ke liye yeh method hai
            var tokenHandler = new JwtSecurityTokenHandler();

            // secrate key normal string nahi hoti, yeh ek byte array me convert hoti hai
            var key = Encoding.UTF8.GetBytes("aEj7A6mr5yVoDx0wq1jUj0A6xhb/8I+YJ0T+Y8h2sJk="); // Secret key must be us for sign token for fake token.

            //ab hum JWT Token banana chahte hain, to hume ek "description" deni padti hai ki: Info, Timeout, key, 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // so many extra point use kar sakte hai, Model Based
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, emp.sName),
                    new Claim(ClaimTypes.Email, emp.sEmail.ToString()),
                    new Claim(ClaimTypes.Hash, emp.sPassword),
                //  new Claim("City", emp.City),           agr koi extra point hai to use kar sakte ho
                    new Claim(ClaimTypes.MobilePhone, emp.sContact.ToString()),
                    new Claim(ClaimTypes.Role, emp.role)
                }),
                Expires = DateTime.UtcNow.AddHours(1), // token 1 ghante me expire hoga
                Issuer = "adityaInfotech",
                Audience = "GTG's IntoTech",
                // SigningCredentials( kon secret Key use karni hai, kon c algo use karni hai)
                // SymmetricSecurityKey = Tumhari secret key hai jo token ke saath judi hogi.
                // HMAC SHA-256 Algorithm use hoga token ko encrypt/sign karne ke liye.
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Token ko create karte hain
            var token = tokenHandler.CreateToken(tokenDescriptor);
            // Token ko string me convert karte hain
            return tokenHandler.WriteToken(token);
        }


    }
}
