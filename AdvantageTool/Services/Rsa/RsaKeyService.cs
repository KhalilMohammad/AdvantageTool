using AdvantageTool.Services.Rsa.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;

namespace AdvantageTool.Services.Rsa
{
    public class RsaKeyService
    {
        /// <summary>
        /// This points to a JSON file in the format: 
        /// {
        ///  "Modulus": "",
        ///  "Exponent": "",
        ///  "P": "",
        ///  "Q": "",
        ///  "DP": "",
        ///  "DQ": "",
        ///  "InverseQ": "",
        ///  "D": ""
        /// }
        /// </summary>
        private string _file
        {
            get
            {
                return Path.Combine(_environment.ContentRootPath, "rsakey.json");
            }
        }
        private readonly IWebHostEnvironment _environment;
        private readonly TimeSpan _timeSpan;

        public RsaKeyService(IWebHostEnvironment environment, TimeSpan timeSpan)
        {
            _environment = environment;
            _timeSpan = timeSpan;
        }

        public bool NeedsUpdate()
        {
            if (File.Exists(_file))
            {
                var creationDate = File.GetCreationTime(_file);
                return DateTime.Now.Subtract(creationDate) > _timeSpan;
            }
            return true;
        }

        public RSAParameters GetRandomKey()
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            try
            {
                return rsa.ExportParameters(true);
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }

        public RsaKeyService GenerateKeyAndSave(bool forceUpdate = false)
        {
            if (forceUpdate || NeedsUpdate())
            {
                var p = GetRandomKey();
                var t = new RSAParametersWithPrivate();
                t.SetParameters(p);
                File.WriteAllText(_file, JsonConvert.SerializeObject(t, Formatting.Indented));
            }
            return this;
        }

        /// <summary>
        /// 
        /// Generate 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public RSAParameters GetKeyParameters()
        {
            if (!File.Exists(_file)) throw new FileNotFoundException("Check configuration - cannot find auth key file: " + _file);
            var keyParams = JsonConvert.DeserializeObject<RSAParametersWithPrivate>(File.ReadAllText(_file));
            return keyParams.ToRSAParameters();
        }

        public RsaSecurityKey GetKey()
        {
            if (NeedsUpdate()) GenerateKeyAndSave();
            var provider = new RSACryptoServiceProvider();
            provider.ImportParameters(GetKeyParameters());
            var key = Guid.NewGuid();
            return new RsaSecurityKey(provider)
            {
                KeyId = key.ToString()
            };
        }

        public JWT GenerateSignedJWT(string keyId, string clientId, string audience)
        {
            var jwt = new JWT(new Guid(keyId), clientId, audience);
            var provider = new RSACryptoServiceProvider();
            provider.ImportParameters(GetKeyParameters());
            jwt.Sign(ref provider);
            return jwt;
        }
    }
}
