using AdvantageTool.Lti;
using System.IdentityModel.Tokens.Jwt;

namespace AdvantageTool.Models
{
    public class ToolResponseModel
    {
        /// <summary>
        /// The error discovered while parsing the request.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// The parsed JWT header from id_token. Null if invalid token.
        /// </summary>
        public JwtHeader JwtHeader { get; set; }

        /// <summary>
        /// Wrapper around the request payload.
        /// </summary>
        public LtiResourceLinkRequest LtiRequest { get; set; }
    }
}
