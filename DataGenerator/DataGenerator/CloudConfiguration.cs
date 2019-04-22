using System.Configuration;

namespace DataGenerator
{
    public static class CloudConfiguration
    {
        public static string NoiseSentiments => ConfigurationManager.AppSettings["NoiseSentimentsOccurence"];
        public static string ModifiableSentiments => ConfigurationManager.AppSettings["ModifiableSentimentsOccurence"];

        // Event Hub config
        public static string EventHubName => ConfigurationManager.AppSettings["EventHubName"];
        public static string EventHubConnString => ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

        //Timer Config
        public static string Timer => ConfigurationManager.AppSettings["Timer"];

    }
}
