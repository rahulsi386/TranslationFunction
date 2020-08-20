using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Graph.Auth;

namespace Meetings
{
    public static class MeetingScheduler
    {
        [FunctionName("MeetingScheduler")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public static void CalendarCheck()
        {
            var clientId = "88478be8-1fea-46a9-9b55-678a1c7bc7b6";
            var clientSecret = "sB_.rFw-DtuRLHSFUo-04_QW94TuTMED28";
            var redirectUri = "http://localhost";
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithRedirectUri(redirectUri)
                .WithClientSecret(clientSecret)
                .Build();

            OnBehalfOfProvider authProvider = new OnBehalfOfProvider(confidentialClientApplication);
        }
    }
}
