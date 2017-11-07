using Apprenda.Services.Logging;
using RestSharp;
using System;

using System.Net;
using DBCloning.Properties;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DBCloning.Models;

namespace DBCloning.Clients
{
    internal class ACP
    {
        private readonly ILogger log = LogManager.Instance().GetLogger(typeof(ACP));
        private readonly RestClient client;
        private string token;

        internal ACP(string token)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            this.token = token;
            log.Info($"ACP auth {this.token}, {Settings.Default.PathBasedHost}.{Settings.Default.CloudHost}");
            this.client = new RestClient($"https://{Settings.Default.PathBasedHost}.{Settings.Default.CloudHost}");
        }

        internal async Task<ClientResponse<Component[]>> GetComponentsAsync(string applicationAlias, string versionAlias)
        {
            try
            {
                return await this.SendRequestAsync<Component[]>(Method.GET, "developer", $"/components/{applicationAlias}/{versionAlias}");
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting components: {ex}");
                throw;
            }
        }

        internal async Task<ClientResponse<PageResourceBase>> GetAllCustomPropertiesAsync()
        {
            try
            {
                return await this.SendRequestAsync<PageResourceBase>(Method.GET, "soc", $"/customproperties?pageSize=200");
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting component custom properties: {ex}");
                throw;
            }
        }

        internal async Task<ClientResponse<CustomProperty[]>> GetComponentCustomPropertiesAsync(string applicationAlias, string versionAlias, string componentAlias)
        {
            try
            {
                string path = $"/customproperties/{applicationAlias}/{versionAlias}";
                if (!string.IsNullOrEmpty(componentAlias))
                {
                    path += $"/{componentAlias}";
                }
                ClientResponse < CustomProperty[] > r = await this.SendRequestAsync<CustomProperty[]>(Method.GET, "developer", path);
                this.log.Error($"Custom properties Payload: {r.Payload}");
                return r;
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting component custom properties: {ex}");
                throw ex;
            }
        }

        internal ClientResponse<CustomProperty[]> GetComponentCustomProperties(string applicationAlias, string versionAlias, string componentAlias)
        {
            try
            {
                string path = $"/customproperties/{applicationAlias}/{versionAlias}";
                if (!string.IsNullOrEmpty(componentAlias))
                {
                    path += $"/{componentAlias}";
                }
                ClientResponse<CustomProperty[]> r = this.SendRequest<CustomProperty[]>(Method.GET, "developer", path);
                this.log.Error($"Custom properties Payload: {r.Payload}");
                return r;
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting component custom properties: {ex}");
                throw ex;
            }
        }

        internal async Task<ClientResponse<ApplicationVersion>> GetApplicationVersionAsync(string applicationAlias, string versionAlias)
        {
            try
            {
                return await this.SendRequestAsync<ApplicationVersion>(Method.GET, "developer", $"/versions/{applicationAlias}/{versionAlias}");
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting application version: {ex}");
                throw;
            }
        }

        private async Task<ClientResponse<TPayload>> SendRequestAsync<TPayload>(Method method, string api, string restUrl, object body = null)
        {
            var request = new RestRequest($"{api}/api/v1{restUrl}", method);
            log.Info($"Request url: {this.client.BaseUrl}{api}/api/v1{restUrl}");

            if (!string.IsNullOrWhiteSpace(this.token))
            {
                request.AddHeader("ApprendaSessionToken", this.token);
            }

            if (body != null)
            {
                request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
            }

            var response = await this.client.ExecuteTaskAsync<TPayload>(request);
            this.log.Info($"Response Status Code: {response.StatusCode}");
            this.log.Info($"Response Error Message: {response.ErrorMessage}");
            if (response.ErrorException != null)
            {
                this.log.Info($"Response Error Message: {response.ErrorException.ToString()}");
            }
            this.log.Info($"Response Response Status: {response.ResponseStatus}");
            this.log.Info($"Response Status Description: {response.StatusDescription}");
            this.log.Info($"Response Content: {response.Content}");

            if ((int)response.StatusCode < 400 ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {

                ClientResponse < TPayload > r = new ClientResponse<TPayload>
                {
                    Response = response,
                    Payload = JsonConvert.DeserializeObject<TPayload>(response.Content)
                };
                log.Info($"Payload Backup: {r.Payload}");
                return r;
            }

            throw new Exception($"Request failed with status code {response.StatusCode}: {response.Content}");
        }

        private ClientResponse<TPayload> SendRequest<TPayload>(Method method, string api, string restUrl, object body = null)
        {
            var request = new RestRequest($"{api}/api/v1{restUrl}", method);
            log.Info($"Request url: {this.client.BaseUrl}{api}/api/v1{restUrl}");

            if (!string.IsNullOrWhiteSpace(this.token))
            {
                request.AddHeader("ApprendaSessionToken", this.token);
            }

            if (body != null)
            {
                request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
            }

            var response = this.client.Execute(request);
            this.log.Info($"Response Status Code: {response.StatusCode}");
            this.log.Info($"Response Error Message: {response.ErrorMessage}");
            if (response.ErrorException != null)
            {
                this.log.Info($"Response Error Message: {response.ErrorException.ToString()}");
            }
            this.log.Info($"Response Response Status: {response.ResponseStatus}");
            this.log.Info($"Response Status Description: {response.StatusDescription}");
            this.log.Info($"Response Content: {response.Content}");

            if ((int)response.StatusCode < 400 ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {

                ClientResponse<TPayload> r = new ClientResponse<TPayload>
                {
                    Response = response,
                    Payload = JsonConvert.DeserializeObject<TPayload>(response.Content)
                };
                log.Info($"Payload Backup: {r.Payload}");
                return r;
            }

            throw new Exception($"Request failed with status code {response.StatusCode}: {response.Content}");
        }
    }
}
