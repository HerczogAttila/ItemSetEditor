using Newtonsoft.Json;
using System.IO;

namespace ItemSetEditor
{
    public class DDImage
    {
        public static string BaseLink { get; set; } = "";

        public string Sprite { get; set; }
        [JsonProperty("x")]
        public int Left { get; set; }
        [JsonProperty("y")]
        public int Top { get; set; }
        [JsonProperty("w")]
        public int Width { get; set; }
        [JsonProperty("h")]
        public int Height { get; set; }

        public string Link => BaseLink + Sprite;
        public string Path => "ItemSetEditor\\" + Sprite;
        public string SourceRect => Left + " " + Top + " " + Width + " " + Height;
        public string AbsolutePath => Directory.GetCurrentDirectory() + "\\" + Path;

        public DDImage()
        {
            Sprite = "";
            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
        }
    }
}
