using Cedar.Configuration;
using Framework.Galaxy.Dtos;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Galaxy
{
    /// <summary>
    /// Client to create and hit web service requests
    /// </summary>
    public class WebServiceClient
    {
        string username = string.Empty;
        string password = string.Empty;
        string serviceBaseUrl = string.Empty;
        string soapAction = string.Empty;

        /// <summary>
        /// Creates header for Soap request Dynamically
        /// </summary>
        /// <param name="serviceName">Soap service Name to target</param>
        /// <returns>Returns the soap header with required tokens</returns>
        public string CreateSoapHeader(string serviceName)
        {
            //get config details
            var configJson = File.ReadAllText(Path.Combine(TestConfiguration.ResourcePath, "WebServiceConfig.json"));

            var webServiceConfigs = JsonConvert.DeserializeObject<WebServiceConfigDto>(configJson);

            var webServiceConfig = webServiceConfigs.WebServiceConfig.Find(w => w.Name.ToLower() == serviceName.ToLower());

            var config = webServiceConfig.Platform.FirstOrDefault(j => j.Env == TestConfiguration.BaseURL);

            username = config.Username;
            password = config.Password;
            serviceBaseUrl = config.Host + webServiceConfig.Uri;
            soapAction = webServiceConfig.SoapAction;

            string createdStr = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
            var createdByteArray = Encoding.ASCII.GetBytes(createdStr);

            var sha1 = SHA1.Create();
            var sha1nonce = sha1.ComputeHash(createdByteArray);

            string nonce = Convert.ToBase64String(sha1nonce);
            var nonceByteArray = Convert.FromBase64String(nonce);
            var passwordByteArray = Encoding.ASCII.GetBytes(password);

            //This part of documentation: Password_Digest = Base64 ( SHA-1 ( nonce + created + password ) )
            var passwordDigestByteArray = nonceByteArray.Concat(createdByteArray).Concat(passwordByteArray).ToArray();

            sha1 = SHA1.Create();

            var sha1PasswordDigest = sha1.ComputeHash(passwordDigestByteArray);
            var b64PasswordDigest = Convert.ToBase64String(sha1PasswordDigest);

            var passwordXml = $"<wsse:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">{b64PasswordDigest}</wsse:Password>";
            var nonceXml = $"<wsse:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">{nonce}</wsse:Nonce>";
            var createdXml = $"<wsu:Created>{createdStr}</wsu:Created>";
            var usernameXml = $"<wsse:Username>{username}</wsse:Username>";

            string soapHeader = @"<soapenv:Header>
            <wsse:Security soapenv:mustUnderstand=""1"" 
            xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" 
            xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
            <wsse:UsernameToken wsu:Id=""UsernameToken-164F6BBC54DF8A171C16194646068044"">"
            + usernameXml + passwordXml + nonceXml + createdXml +
            @"</wsse:UsernameToken>
            </wsse:Security>
            </soapenv:Header>";

            return soapHeader;
        }

        /// <summary>
        /// Creates Soap Envelop body
        /// </summary>
        /// <param name="header">Soap header</param>
        /// <param name="body">Xml body</param>
        /// <param name="envelopAttr">Envelop attribute, if empty uses a standard envelop attribute for service types</param>
        /// <returns>Returns the Soap envelop</returns>
        public string CreateSoapEnvelope(string header, string body, string envelopAttr = "")
        {
            string envelopAttribute = envelopAttr.Equals("") ? @"<soapenv:Envelope xmlns:ser=""http://www.acclaris.com/servicetypes"" 
            xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">" : envelopAttr;
            
            string soapEnvelop = envelopAttribute
            + header
            + @"<soapenv:Body>" + body
            + @"</soapenv:Body>
            </soapenv:Envelope>";

            return soapEnvelop;
        }

        public async Task<string> PostSoapRequest(string requestBody)
        {
            HttpResponseMessage response = await PostXmlRequest(serviceBaseUrl, requestBody, soapAction);
            string content = await response.Content.ReadAsStringAsync();

            return content;
        }

        private async Task<HttpResponseMessage> PostXmlRequest(string baseUrl, string requestBody, string soapAction)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(requestBody, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", soapAction);

                return await httpClient.PostAsync(baseUrl, httpContent);
            }
        }
    }
}
