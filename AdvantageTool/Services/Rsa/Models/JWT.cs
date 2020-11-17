using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AdvantageTool.Services.Rsa.Models
{
    struct JWTHeader
    {
        [JsonProperty("alg")]
        public string Alg { get; set; }
        [JsonProperty("typ")]
        public string Typ { get; set; }
        [JsonProperty("kid")]
        public Guid Kid { get; set; }
    }

    struct JWTPayload
    {
        [JsonProperty("iss")]
        public string Iss { get; set; }
        [JsonProperty("sub")]
        public string Sub { get; set; }
        [JsonProperty("jti")]
        public Guid Jti { get; set; }
        [JsonProperty("aud")]
        public string Aud { get; set; }
        [JsonProperty("exp")]
        public long Exp { get; set; }
        [JsonProperty("nbf")]
        public long Nbf { get; set; }
        [JsonProperty("iat")]
        public long Iat { get; set; }
    }

    public class JWT
    {
        private JWTHeader Header { get; }

        private string HeaderJsonString =>
            JsonConvert.SerializeObject(Header);

        private string HeaderBase64String =>
            WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(HeaderJsonString));

        private JWTPayload Payload { get; }

        private string PayloadJsonString =>
            JsonConvert.SerializeObject(Payload);

        private string PayloadBase64String =>
            WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(PayloadJsonString));

        private byte[] Signature { get; set; }

        private string SignatureBase64String =>
            WebEncoders.Base64UrlEncode(Signature);

        public JWT(Guid keyId, string clientId, string audience)
        {
            Header = new JWTHeader
            {
                Alg = "RS256",
                Typ = "JWT",
                Kid = keyId
            };

            var now = new DateTimeOffset(DateTime.Now);
            Payload = new JWTPayload
            {
                Aud = audience,
                Iss = clientId,
                Sub = clientId,
                Jti = Guid.NewGuid(),
                Nbf = now.ToUnixTimeSeconds(),
                Exp = now
                        .AddMonths(1)
                        .ToUnixTimeSeconds(),
                Iat = now.ToUnixTimeSeconds()
            };
        }

        public string JWTBase64String() =>
            $"{HeaderBase64String}.{PayloadBase64String}.{SignatureBase64String}";

        public void Sign(ref RSACryptoServiceProvider rsaProvider)
        {
            Signature = rsaProvider.SignData(
                Encoding.UTF8.GetBytes($"{HeaderBase64String}.{PayloadBase64String}"),
                SHA256.Create()
            );
        }
    }
}
