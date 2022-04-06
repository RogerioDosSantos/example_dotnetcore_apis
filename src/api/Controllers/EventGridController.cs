using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.Management.EventGrid;
using Microsoft.Azure.Management.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure;

//Event Grid API documentation
// - https://docs.microsoft.com/en-us/dotnet/api/overview/azure/eventgrid?view=azure-dotnet

namespace DotNetCoreApis.Controllers
{
    public class CustomLoginCredentials : ServiceClientCredentials
    {
        private string AuthenticationToken { get; set; }
        private readonly string _tenantId = null;
        private readonly string _clientId = null;
        private readonly string _clientSecret = null;


        public CustomLoginCredentials(string tenantId, string clientId, string clientSecret)
        {
            _tenantId = tenantId;
            _clientId = clientId; //"xxxxx-xxxx-xx-xxxx-xxx"
            _clientSecret = clientSecret;
        }

        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            AuthenticationContext authenticationContext = new AuthenticationContext($"https://login.windows.net/{_tenantId}");
            ClientCredential credential = new ClientCredential(clientId: _clientId, clientSecret: _clientSecret);
            AuthenticationResult result = authenticationContext.AcquireTokenAsync(resource: "https://management.core.windows.net/", clientCredential: credential).GetAwaiter().GetResult();
            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");
            AuthenticationToken = result.AccessToken;
        }

        public async override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (AuthenticationToken == null)
                throw new InvalidOperationException("Token Provider Cannot Be Null");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //request.Version = new Version(apiVersion);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    public class EventPayload
    {
        public string stringExampleData { get; set; }
        public int intExampleData { get; set; }
        public double doubleExampleData { get; set; }
        public bool boolExampleData { get; set; }
    }

    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EventGridController : ControllerBase
    {
        //Pre-requirements
        //Create Azure AD application and Service Principal: https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal

        private readonly ILogger _logger = null;
        public enum AccessKeys
        {
            key1 = 0,
            key2 = 1
        }
        public EventGridController(ILogger<EventGridController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Register an Event Grid event in a resource (for posting)
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="topicLocation">Location of the resource that will create the topic. E.g.: westus2</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("RegisterTopic")]
        public async Task<Topic> RegisterTopic(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId, string credentialsClientSecret,
            string resourceGroupName, string topicLocation, string topicName)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
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
        /// Register an Event Grid domain in a resource
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="domainName">The name of the domain where you want to subscribe the event.</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="domainLocation">Location of the resource that will create the topic. E.g.: westus2</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("RegisterDomain")]
        public async Task<Domain> RegisterDomain(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string credentialsClientSecret, string domainName, string domainLocation, string resourceGroupName, string topicName)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                Domain domainInfo = new Domain(domainLocation);
                Domain createdDomain = await managementClient.Domains.CreateOrUpdateAsync(resourceGroupName: resourceGroupName, domainName: domainName,
                    domainInfo: domainInfo);
                return createdDomain;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Remove a domain. In case topics are registered on this domain, all topics will be removed.
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="domainName">The name of the domain where you want to subscribe the event.</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpDelete("RemoveDomain")]
        public async Task<bool> RemoveDomain(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string credentialsClientSecret, string domainName, string resourceGroupName)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(
                    credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                await managementClient.Domains.DeleteAsync(resourceGroupName: resourceGroupName, domainName: domainName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// List the Domains in a resource group
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="filter">Filter that will be executed on the search. E.g.: contains(name, 'mydomain')</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("Domains")]
        public async Task<List<Domain>> ListDomains(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string credentialsClientSecret, string resourceGroupName, string filter)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                IPage<Domain> domainsResult = await managementClient.Domains.ListByResourceGroupAsync(resourceGroupName: resourceGroupName, filter: filter);
                List<Domain> domains = new List<Domain>();
                foreach (Domain domain in domainsResult)
                {
                    domains.Add(domain);
                }
                return domains;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Get the both keys of an Event Grid domain
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="domainName">The name of the domain where you want to subscribe the event.</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("DomainKeys")]
        public async Task<DomainSharedAccessKeys> GetDomainKeys(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string credentialsClientSecret, string domainName, string resourceGroupName)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                DomainSharedAccessKeys domainKeys = await managementClient.Domains.ListSharedAccessKeysAsync(resourceGroupName: resourceGroupName, domainName: domainName);
                return domainKeys;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Renew a specific key of an Event Grid Domain
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="domainName">The name of the domain where you want to subscribe the event.</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="key">Name of the key to regenerate.</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("RenewDomainKey")]
        public async Task<DomainSharedAccessKeys> RenewDomainKey(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string credentialsClientSecret, string domainName, string resourceGroupName, AccessKeys key)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                DomainSharedAccessKeys domainKeys = await managementClient.Domains.RegenerateKeyAsync(
                    resourceGroupName: resourceGroupName,
                    domainName: domainName,
                    keyName: key == AccessKeys.key1 ? "key1" : "key2");
                return domainKeys;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Register topic in an Event Grid domain (for posting)
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="domainName">The name of the domain where you want to subscribe the event.</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("RegisterDomainTopic")]
        public async Task<DomainTopic> RegisterDomainTopic(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string domainName, string credentialsClientSecret, string resourceGroupName, string topicName)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                DomainTopic createdTopic = await managementClient.DomainTopics.CreateOrUpdateAsync(resourceGroupName: resourceGroupName,
                    domainName: domainName, domainTopicName: topicName);
                return createdTopic;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// List topics available from an specific Event Grid Domain
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="domainName">The name of the domain where you want to subscribe the event.</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="filter">Filter that will be executed on the search. E.g.: contains(name, 'mytopic')</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("DomainTopics")]
        public async Task<List<DomainTopic>> GetDomainTopics(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string credentialsClientSecret, string resourceGroupName, string domainName, string filter)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                IPage<DomainTopic> topicsResult = await managementClient.DomainTopics.ListByDomainAsync(
                    resourceGroupName: resourceGroupName,
                    domainName: domainName,
                    filter: filter);
                List<DomainTopic> topics = new List<DomainTopic>();
                foreach (DomainTopic topic in topicsResult)
                {
                    topics.Add(topic);
                }
                return topics;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Remove topic of an specific Event Grid Domain
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="domainName">The name of the domain where you want to subscribe the event.</param>
        /// <param name="credentialsClientSecret">Secret of the client requesting the token.</param>
        /// <param name="resourceGroupName">The name of the resource group within the user subscription.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpDelete("RemoveDomainTopic")]
        public async Task<bool> RemoveDomainTopics(string credentialsAzureSubscriptionId, string credentialsTenantId, string credentialsClientId,
            string credentialsClientSecret, string resourceGroupName, string domainName, string topicName)
        {
            try
            {
                //Management SDKs (Microsoft.Azure.Management.EventGrid)
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                await managementClient.DomainTopics.DeleteAsync(resourceGroupName: resourceGroupName, domainName: domainName, domainTopicName: topicName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Publish an event on the Event Grid
        /// </summary>
        /// <param name="topicHostName"></param>
        /// <param name="dataVersion"></param>
        /// <param name="topicKey"></param>
        /// <param name="topic"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        /// <param name="subject"></param>
        /// <param name="data"></param>
        /// <param name="metadataVersion"></param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("PushEvent")]
        public async Task<bool> PushEvent(string topicHostName, string id, string subject, object data, string eventType, string dataVersion, string topicKey,
            string topic = null, string metadataVersion = null)
        {
            await Task.Delay(0);
            try
            {
                // Data plane SDKs (Microsoft.Azure.EventGrid)
                ServiceClientCredentials credentials = new TopicCredentials(topicKey);
                EventGridClient eventGridClientKey = new EventGridClient(credentials);
                // Create a new event and add it to a list
                List<EventGridEvent> events = new List<EventGridEvent>();
                events.Add(new EventGridEvent(id, subject, data, eventType, DateTime.UtcNow, dataVersion, topic, metadataVersion));
                // Publish the events
                await eventGridClientKey.PublishEventsAsync(topicHostName, events);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Subscribe to an event
        /// </summary>
        /// <param name="credentialsAzureSubscriptionId">The Azure Subscription Id. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsTenantId">Tenant id that will be used on the URL to reach the login for having the azure credentials (https://login.windows.net/{tenantId})</param>
        /// <param name="credentialsClientId">Identifier of the client requesting the token. ("xxxxx-xxxx-xx-xxxx-xxx")</param>
        /// <param name="credentialsClientSecret"></param>
        /// <param name="scope">The identifier of the resource to which the event subscription needs to be created. 
        /// The scope can be a subscription, or a resource group, or a top level resource belonging to a resource provider namespace, or an EventGrid topic.
        /// E.g.:
        /// - Subscription: /subscriptions/{subscriptionId}/
        /// - Resource Group: subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}
        /// - Resource: /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/{resourceProviderNamespace}/{resourceType}/{resourceName}
        /// - Event Grid topic: /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.EventGrid/topics/{topicName}</param>
        /// <param name="eventSubscriptionName">The name of the event you want to subscribe.</param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="subscriptionType"></param>
        /// <param name="topic"></param>
        /// <param name="provisioningState"></param>
        /// <param name="labels"></param>
        /// <param name="eventDeliverySchema"></param>
        /// <param name="maxDeliveryAttempts"></param>
        /// <param name="eventTimeToLiveInMinutes"></param>
        /// <param name="expirationTimeInHours"></param>
        /// <param name="endpointUrl"></param>
        /// <param name="maxEventsPerBatch"></param>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpPost("SubscribeEvent")]
        public async Task<EventSubscription> SubscribeEvent(
            string credentialsAzureSubscriptionId,
            string credentialsTenantId,
            string credentialsClientId,
            string credentialsClientSecret,
            string scope,
            string eventSubscriptionName,
            string id,
            string name,
            string subscriptionType,
            string topic,
            string provisioningState,
            List<string> labels,
            string eventDeliverySchema,
            int maxDeliveryAttempts,
            int eventTimeToLiveInMinutes,
            double expirationTimeInHours,
            string endpointUrl,
            int? maxEventsPerBatch)
        {
            await Task.Delay(0);
            try
            {
                EventGridManagementClient managementClient = new EventGridManagementClient(credentials: new CustomLoginCredentials(credentialsTenantId, credentialsClientId, credentialsClientSecret));
                managementClient.SubscriptionId = credentialsAzureSubscriptionId;
                EventSubscriptionFilter filter = null;
                DateTime? expirationTime = DateTime.UtcNow.AddHours(expirationTimeInHours);
                RetryPolicy retryPolicy = new RetryPolicy(maxDeliveryAttempts: maxDeliveryAttempts,
                    eventTimeToLiveInMinutes: eventTimeToLiveInMinutes);
                DeadLetterDestination deadLetterDestination = null;
                WebHookEventSubscriptionDestination destination = new WebHookEventSubscriptionDestination(endpointUrl: endpointUrl,
                    maxEventsPerBatch: maxEventsPerBatch);
                EventSubscription eventSubscriptionInfo = new EventSubscription(
                    id: id,
                    name: name,
                    type: subscriptionType,
                    topic: topic,
                    provisioningState: provisioningState,
                    destination: destination,
                    filter: filter,
                    labels: labels,
                    expirationTimeUtc: expirationTime,
                    eventDeliverySchema: eventDeliverySchema,
                    retryPolicy: retryPolicy,
                    deadLetterDestination: deadLetterDestination);
                EventSubscription createdSubscription = await managementClient.EventSubscriptions.CreateOrUpdateAsync(
                    scope: scope,
                    eventSubscriptionName: eventSubscriptionName,
                    eventSubscriptionInfo: eventSubscriptionInfo);
                return createdSubscription;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }
    }
}
