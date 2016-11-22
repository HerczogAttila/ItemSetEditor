using System.IO;

namespace ItemSetEditor
{
    public class DDImage
    {
        public string sprite { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }

        public string Link => "http://ddragon.leagueoflegends.com/cdn/" + MainWindow.Config.Version + "/img/sprite/" + sprite;
        public string Path => "ItemSetEditor\\" + sprite;
        public string SourceRect => x + " " + y + " " + w + " " + h;
        public string AbsolutePath => Directory.GetCurrentDirectory() + "\\" + Path;

        public DDImage()
        {
            sprite = "";
            x = 0;
            y = 0;
            w = 0;
            h = 0;
        }
    }
}
