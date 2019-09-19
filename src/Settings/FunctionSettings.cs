namespace AlejoF.Media.Settings
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
}
