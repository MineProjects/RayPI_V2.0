﻿using Microsoft.IdentityModel.Tokens;
using RayPI.Token.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RayPI.Token
{
    public class RayPIToken
    {

        public RayPIToken()
        {
        }

        /// <summary>
        /// 获取JWT字符串并存入缓存
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="expireSliding"></param>
        /// <param name="expireAbsoulte"></param>
        /// <returns></returns>
        public static string IssueJWT(TokenModel tokenModel, TimeSpan expiresSliding, TimeSpan expiresAbsoulte)
        {
            DateTime UTC = DateTime.UtcNow;
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,tokenModel.Sub),//Subject,
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),//JWT ID,JWT的唯一标识
                new Claim(JwtRegisteredClaimNames.Iat, UTC.ToString(), ClaimValueTypes.Integer64),//Issued At，JWT颁发的时间，采用标准unix时间，用于验证过期
            };

            JwtSecurityToken jwt = new JwtSecurityToken(
            issuer: "RayPI",//jwt签发者,非必须
            audience: tokenModel.Uname,//jwt的接收该方，非必须
            claims: claims,//声明集合
            expires: UTC.AddHours(12),//指定token的生命周期，unix时间戳格式,非必须
            signingCredentials: new Microsoft.IdentityModel.Tokens
                .SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("RayPI's Secret Key")), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            RayPIMemoryCache.AddMemoryCache(encodedJwt, tokenModel, expiresSliding, expiresAbsoulte);//将JWT字符串，令牌实体，存入缓存
            return encodedJwt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Microsoft.Extensions.Primitives.StringValues ReadJWT(Microsoft.AspNetCore.Http.HttpContext context)
        {
            Microsoft.Extensions.Primitives.StringValues Bearer;
            context.Request.Headers.TryGetValue("Authorization", out Bearer);
            return Bearer;
        }
    }
}
