using System;

namespace AlejoF.Thumbnailer.Settings
{
    public class FunctionSettings
    {   
        public string StorageConnectionString { get; set; }
        public int MaxMediaWidth { get; set; }
        public int ThumbnailSize { get; set; }
        
        public CognitiveServicesSettings CognitiveServices { get; set; }
    }
    
    public class CognitiveServicesSettings
    {
        public string Host { get; set; }
        public string Key { get; set; }
    }

    public class Factory
    {
        public static FunctionSettings Build()
        {
            var getCognitiveSetting = GetPrefixedSettingFunc<CognitiveServicesSettings>();
            
            return new FunctionSettings
            {
                StorageConnectionString = GetSetting(nameof(FunctionSettings.StorageConnectionString)),
                MaxMediaWidth = GetIntSetting(nameof(FunctionSettings.MaxMediaWidth)) ?? 1200,
                ThumbnailSize = GetIntSetting(nameof(FunctionSettings.ThumbnailSize)) ?? 150,
                
                CognitiveServices = new CognitiveServicesSettings
                {
                    Host = getCognitiveSetting(nameof(CognitiveServicesSettings.Host)),
                    Key = getCognitiveSetting(nameof(CognitiveServicesSettings.Key)),
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
