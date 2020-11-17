using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdvantageTool.Models
{
    public class OidcModel
    {
        /// <summary>
        /// Platform Issuer URL
        /// </summary>
        [JsonProperty("iss")]
        [BindProperty(Name = "iss", SupportsGet = true)]
        public string Issuer { get; set; } = "https://testconestoga.desire2learn.com";

        /// <summary>
        /// Opaque value that helps the platform identify the user
        /// </summary>
        [JsonProperty("login_hint")]
        [BindProperty(Name = "login_hint", SupportsGet = true)]
        public string LoginHint { get; set; }

        /// <summary>
        /// Opaque value that helps the platform identity the resource link
        /// </summary>
        [JsonProperty("lti_message_hint")]
        [BindProperty(Name = "lti_message_hint", SupportsGet = true)]
        public string LtiMessageHint { get; set; }

        /// <summary>
        /// Tool's Deployment ID
        /// </summary>
        [JsonProperty("lti_deployment_id")]
        [BindProperty(Name = "lti_deployment_id", SupportsGet = true)]
        public string LtiDeploymentId { get; set; }

        /// <summary>
        /// Tool's launch URL
        /// </summary>
        [JsonProperty("target_link_uri")]
        [BindProperty(Name = "target_link_uri", SupportsGet = true)]
        public string TargetLinkUri { get; set; }

        /// <summary>
        /// Platform's client id
        /// </summary>
        [JsonProperty("client_id")]
        [BindProperty(Name = "client_id", SupportsGet = true)]
        public string ClientId { get; set; }
        /// <summary>
        /// Brightspace OAuth2 Audience
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// Access Token Url
        /// </summary>
        public string AccessTokenUrl { get; set; }
    }
}
