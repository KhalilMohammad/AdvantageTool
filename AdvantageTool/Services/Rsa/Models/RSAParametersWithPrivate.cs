using System.Security.Cryptography;

namespace AdvantageTool.Services.Rsa.Models
{
    /// <summary>
    /// Util class to allow restoring RSA parameters from JSON as the normal
    /// RSA parameters class won't restore private key info.
    /// </summary>
    public class RSAParametersWithPrivate
    {
        public byte[] D { get; set; }
        public byte[] DP { get; set; }
        public byte[] DQ { get; set; }
        public byte[] Exponent { get; set; }
        public byte[] InverseQ { get; set; }
        public byte[] Modulus { get; set; }
        public byte[] P { get; set; }
        public byte[] Q { get; set; }

        public void SetParameters(RSAParameters p)
        {
            D = p.D;
            DP = p.DP;
            DQ = p.DQ;
            Exponent = p.Exponent;
            InverseQ = p.InverseQ;
            Modulus = p.Modulus;
            P = p.P;
            Q = p.Q;
        }

        public RSAParameters ToRSAParameters()
            => new RSAParameters()
            {
                D = D,
                DP = DP,
                DQ = DQ,
                Exponent = Exponent,
                InverseQ = InverseQ,
                Modulus = Modulus,
                P = P,
                Q = Q
            };
    }
}
