namespace ItemSetEditor
{
    public class SortTag
    {
        public string Tag { get; set; }
        public bool? IsChecked { get; set; }

        public SortTag()
        {
            Tag = "";
            IsChecked = false;
        }
    }
}
