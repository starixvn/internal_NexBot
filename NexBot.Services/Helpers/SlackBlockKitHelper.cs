namespace NexBot.Services.Helpers
{
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft.Bot.Schema;

    public static class SlackBlockKitHelper
    {
        public static object GenerateSlackBlockKit<T>(string templatePath, T model)
        {
            var templateJson = File.ReadAllText(templatePath);
            var modelJson = JObject.FromObject(model);
            var expandedJson = ReplaceTemplateVariables(templateJson, modelJson);
            return JsonConvert.DeserializeObject(expandedJson);
        }

        private static string ReplaceTemplateVariables(string templateJson, JObject modelJson)
        {
            foreach (var property in modelJson.Properties())
            {
                var placeholder = "${" + property.Name + "}";
                templateJson = templateJson.Replace(placeholder, property.Value.ToString());
            }
            return templateJson;
        }
    }
}