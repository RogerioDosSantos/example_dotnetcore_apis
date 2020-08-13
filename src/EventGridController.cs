using System;
using System.Threading.Tasks;
using DotNetCoreApis.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Rest;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using Microsoft.Azure.Management.EventGrid.Models;

namespace DotNetCoreApis.Controllers
{
    public class CustomLoginCredentials : ServiceClientCredentials
    {        
        private string AuthenticationToken { get; set; }
        private string _tenantId = null;
        private string _clientId = null;
        private string _clientSecret = null;


        public CustomLoginCredentials(string tenantId, string clientId, string clientSecret)
        {
            _tenantId = tenantId;
            _clientId = clientId; //"xxxxx-xxxx-xx-xxxx-xxx"
            _clientSecret = clientSecret;
        }

        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            var authenticationContext = new AuthenticationContext($"https://login.windows.net/{_tenantId}");
            var credential = new ClientCredential(clientId: _clientId, clientSecret: _clientSecret);
            var result = authenticationContext.AcquireTokenAsync(resource: "https://management.core.windows.net/", clientCredential: credential).GetAwaiter().GetResult();
            if (result == null) 
                throw new InvalidOperationException("Failed to obtain the JWT token");
            AuthenticationToken = result.AccessToken;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException("request");

            if (AuthenticationToken == null) throw new InvalidOperationException("Token Provider Cannot Be Null");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //request.Version = new Version(apiVersion);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }


    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EventGridController : ControllerBase
    {
        //Pre-requirements
        //Create Azure AD application and Service Principal: https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal

        private readonly ILogger _logger = null;
        public EventGridController(ILogger<EventGridController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Register a Event Grid event (for posting)
        /// </summary>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="topicLocation">Location of the resource that will create the topic.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("RegisterEvent")]
        public async Task<Topic> RegisterEvent(string credentialsTenantId, string credentialsClientId, string credentialsClientSecret, 
            string resourceGroupName, string topicLocation, string topicName)
        {
            await Task.Delay(0);
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                Topic topic = new Topic(location: topicLocation);
                Topic createdTopic = await managementClient.Topics.CreateOrUpdateAsync(resourceGroupName: resourceGroupName, topicName: topicName, topicInfo: topic);
                return createdTopic;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Publish an event on the Event Grid
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("PushEvent")]
        public async Task<bool> PushEvent()
        {
            await Task.Delay(0);
            try
            {
                //Data plane SDKs (Microsoft.Azure.EventGrid)
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Subscribe to an event
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("SubscribeEvent")]
        public async Task<bool> SubscribeEvent()
        {
            await Task.Delay(0);
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return false;
            }
        }
    }
}
