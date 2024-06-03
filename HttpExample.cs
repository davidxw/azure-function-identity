using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using System.IdentityModel.Tokens.Jwt;
using Azure.Identity;

namespace func_identity_aca
{
    public class HttpExample
    {
        private readonly ILogger _logger;

        public HttpExample(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpExample>();
        }

        [Function("HttpExample")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var token = await GetAzureDefaultIdentity();

            if (token != null)
            {
                _logger.LogInformation(token.ToString());

                string? displayName = null;

                if (token.Claims.First(c => c.Type == "upn") != null)
                {
                    displayName = token.Claims.First(c => c.Type == "upn").Value;
                }
                else if (token.Claims.First(c => c.Type == "app_displayname") != null)
                {
                    displayName = token.Claims.First(c => c.Type == "app_displayname").Value;
                }
                else if (token.Claims.First(c => c.Type == "sub") != null)
                {
                    displayName = token.Claims.First(c => c.Type == "sub").Value;
                }
                else
                {
                    displayName = "unknown but authenticated user";
                }

                response.WriteString($"Welcome {displayName}:{Environment.NewLine}{Environment.NewLine}{token}");
            }
            else
            {
                response.WriteString("Welcome stranger!");
            }

            return response;
        }

        private async Task<JwtSecurityToken> GetAzureDefaultIdentity()
        {
            try
            {
                var credential = new DefaultAzureCredential();
                string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
                var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes));

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token.Token) as JwtSecurityToken;
                //var upn = jsonToken.Claims.First(c => c.Type == "upn").Value;

                return jsonToken;
            }
            catch
            {
                return null;
            }
        }
    }
}


