namespace InstaSharper.Classes.Models
{
    using Newtonsoft.Json;


    public class Image
    {
        public Image() { }

        public Image(string url)
        {
            Url = url;
        }

        public Image(string url, string width, string height)
        {
            Url = url;
            Width = width;
            Height = height;
        }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }
    }
}