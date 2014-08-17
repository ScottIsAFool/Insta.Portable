
using Newtonsoft.Json;

namespace Insta.Portable.Models
{
    public class Highlight
    {
        [JsonProperty("highlight_id")]
        public int Id { get; set; }
        public string Type { get; set; }
        [JsonProperty("bookmark_id")]
        public int BookmarkId { get; set; }
        public string Text { get; set; }
        public int Position { get; set; }
        public int Time { get; set; }
    }
}
