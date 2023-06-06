using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace RenameMe.Api.Infrastructure.Jwt
{
    public class JwtService
    {
        private readonly JwtSetting jwtSetting;
        public const string TokenTypeConst = "tokenType";
        public const string AccessTokenConst = "accessToken";

        public JwtService(JwtSetting jwtSetting)
        {
            this.jwtSetting = jwtSetting;
        }
        /// <summary>
        /// 生成刷新Token
        /// </summary>
        /// <returns></returns>
        public string GenerateRefreshJwtToken(string accessToken)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SignKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(TokenTypeConst,TokenTypeEnum.RefreshToken.ToString()),
                new Claim(AccessTokenConst,accessToken),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtSetting.Issuer,
                Audience = jwtSetting.Audience,
                Expires = DateTime.UtcNow.AddMinutes(jwtSetting.LongExpiresInMinutes),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = credentials,
                Subject = new ClaimsIdentity(claims)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        /// <summary>
        /// 生成访问令牌
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public string GenerateAccessJwtToken(string sid)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SignKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sid,sid),
                new Claim(TokenTypeConst,TokenTypeEnum.AccessToken.ToString()),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtSetting.Issuer,
                Audience = jwtSetting.Audience,
                Expires = DateTime.UtcNow.AddMinutes(jwtSetting.ShortExpiresInMinutes),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = credentials,
                Subject = new ClaimsIdentity(claims)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 验证token
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="validateLifetime">是否验证过期时间</param>
        /// <returns></returns>
        public Task<TokenValidationResult> ValidateTokenAsync(string token, bool validateLifetime = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSetting!.Issuer,
                ValidAudience = jwtSetting.Audience,
                RequireExpirationTime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SignKey)),
            };
            return tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
        }

        /// <summary>
        /// 凭证是不是Bearer
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <returns></returns>
        public static bool AuthenticationIsBearerScheme(IHttpContextAccessor httpContextAccessor)
        {
            var authenticatioValue = GetAuthentication(httpContextAccessor);
            return authenticatioValue != null && "Bearer".Equals(authenticatioValue.Scheme, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取验证信息
        /// </summary>
        /// <returns></returns>
        public static AuthenticationHeaderValue? GetAuthentication(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor != null && httpContextAccessor.HttpContext != null)
            {
                var authorization = httpContextAccessor.HttpContext.Request.Headers.Authorization.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(authorization))
                {
                    return AuthenticationHeaderValue.Parse(authorization);
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Token的类型
    /// </summary>
    public enum TokenTypeEnum
    {
        AccessToken,
        RefreshToken
    }
}
