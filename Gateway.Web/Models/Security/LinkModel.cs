namespace Gateway.Web.Models.Security
{
    public class LinkModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Glyph { get; set; }

        public string GlyphValue
        {
            get { return char.ConvertFromUtf32(Glyph); }
        }

        public string Type { get; set; }
        public string AdditionalData { get; set; }
    }
}