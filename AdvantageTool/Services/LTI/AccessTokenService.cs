using AdvantageTool.CustomClient;
using AdvantageTool.Models;
using AdvantageTool.Services.Rsa;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AdvantageTool.Services.LTI
{
    /// <summary>
    /// Service available via dependency injection to get an access token from the issuer.
    /// </summary>
    public class AccessTokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OidcModel _oidcModel;
        private readonly RsaKeyService _rsaKeyService;

        /// <summary>
        /// Create an instance of the AccessTokenService.
        /// </summary>
        /// <param name="context">The application database context to look up the issuer's token endpoint.</param>
        /// <param name="httpClientFactory">The HttpClient factory.</param>
        /// <param name-"rsaKeyService">RSA Key service</param>
        public AccessTokenService(IHttpClientFactory httpClientFactory, OidcModel oidcModel, RsaKeyService rsaKeyService)
        {
            _httpClientFactory = httpClientFactory;
            _oidcModel = oidcModel;
            _rsaKeyService = rsaKeyService;
        }

        /// <summary>
        /// Get an access token from the issuer.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        /// <param name="scope">The scope to request.</param>
        /// <returns>The token response.</returns>
        public async Task<TokenResponse> GetAccessTokenAsync(string issuer, string scope)
        {
            // Use a signed JWT as client credentials.
            var payload = new JwtPayload();
            payload.AddClaim(new Claim(JwtRegisteredClaimNames.Iss, _oidcModel.ClientId));
            payload.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, _oidcModel.ClientId));
            payload.AddClaim(new Claim(JwtRegisteredClaimNames.Aud, _oidcModel.Audience));
            payload.AddClaim(new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString()));
            payload.AddClaim(new Claim(JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(DateTime.UtcNow.AddSeconds(-5)).ToString()));
            payload.AddClaim(new Claim(JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(DateTime.UtcNow.AddMinutes(5)).ToString()));
            var bytes = CryptoRandom.CreateRandomKey(32);
            var jti = Base64Url.Encode(bytes);
            payload.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, jti));

            var handler = new JwtSecurityTokenHandler();
            var rsaKey = _rsaKeyService.GetKey();
            var jwt = handler.WriteToken(new JwtSecurityToken(new JwtHeader(new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha512)), payload));

            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.RequestClientCredentialsTokenWithJwtAsync(
                    new JwtClientCredentialsTokenRequest
                    {
                        Address = _oidcModel.AccessTokenUrl,
                        ClientId = _oidcModel.ClientId,
                        Jwt = jwt,
                        Scope = scope
                    });
        }
    }
}
