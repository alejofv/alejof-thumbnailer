using System;

namespace AlejoF.Media.Settings
{
    public class Factory
    {
        public static FunctionSettings Build()
        {
            var getCognitiveSetting = GetPrefixedSettingFunc<CognitiveServicesSettings>();
            
            return new FunctionSettings
            {
                StorageConnectionString = GetSetting("AzureWebJobsStorage"),
                MaxMediaWidth = GetIntSetting("MaxMediaWidth") ?? 1200,
                ThumbnailSize = GetIntSetting("ThumbnailWidth") ?? 150,
                
                CognitiveServices = new CognitiveServicesSettings
                {
                    Host = getCognitiveSetting("Host"),
                    Key = getCognitiveSetting("Key"),
                }
            };
        }

        private static string GetSetting(string name) =>
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

        private static Func<string, string> GetPrefixedSettingFunc<T>() =>
            name => Environment.GetEnvironmentVariable($"{typeof(T).Name}_{name}", EnvironmentVariableTarget.Process);

        private static int? GetIntSetting(string name)
        {
            var value = GetSetting(name);
            if (int.TryParse(value, out int intValue))
                return intValue;

            return null;
        }
    }
}
