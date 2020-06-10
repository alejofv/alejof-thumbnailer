using System;
using Microsoft.Extensions.DependencyInjection;

namespace AlejoF.Thumbnailer.Settings
{
    public class FunctionSettings
    {   
        public int MaxMediaSize { get; set; }
        public int ThumbnailSize { get; set; }
        public string UploadContainer { get; set; } = string.Empty;
        
        public CognitiveServicesSettings CognitiveServices { get; private set; }

        public FunctionSettings(CognitiveServicesSettings cognitiveServicesSettings)
        {
            this.CognitiveServices = cognitiveServicesSettings;
        }
    }
    
    public class CognitiveServicesSettings
    {
        public string? Endpoint { get; set; }
        public string? Key { get; set; }
    }

    internal class Factory
    {
        private const int DefaultMaxMediaWidth = 1200;
        private const int DefaultThumbnailSize = 150;
        private const string DefaultUploadContainer = "uploads";

        internal static FunctionSettings Build()
        {
            var GetCognitiveServicesSetting = BuildPrefixedSettingGetter<CognitiveServicesSettings>();
            
            return new FunctionSettings(
                new CognitiveServicesSettings
                {
                    Endpoint = GetCognitiveServicesSetting(nameof(CognitiveServicesSettings.Endpoint)),
                    Key = GetCognitiveServicesSetting(nameof(CognitiveServicesSettings.Key)),
                })
            {
                MaxMediaSize = GetIntSetting(nameof(FunctionSettings.MaxMediaSize)) ?? DefaultMaxMediaWidth,
                ThumbnailSize = GetIntSetting(nameof(FunctionSettings.ThumbnailSize)) ?? DefaultThumbnailSize,
                UploadContainer = GetSetting(nameof(FunctionSettings.UploadContainer)) ?? DefaultUploadContainer,
            };
        }

        private static string? GetSetting(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

        private static int? GetIntSetting(string name)
        {
            var value = GetSetting(name);
            if (int.TryParse(value, out int intValue))
                return intValue;

            return null;
        }

        /// <summary>
        /// Get a setting getter func for a class name (removing the "Settings" posfix, if any)
        /// </summary>
        private static Func<string, string?> BuildPrefixedSettingGetter<T>() =>
            name => GetSetting($"{typeof(T).Name.Replace("Settings", string.Empty)}_{name}");

    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddFunctionSettings(this IServiceCollection services)
        {
            services.AddSingleton(svc => Factory.Build());
            return services;
        }
    }
}
