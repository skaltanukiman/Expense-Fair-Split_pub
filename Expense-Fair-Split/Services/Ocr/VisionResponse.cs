using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Ocr
{
    public class VisionResponseRoot
    {
        [JsonPropertyName("responses")]
        public List<VisionResponse>? Responses { get; set; }
        
        public class VisionResponse
        {
            [JsonPropertyName("fullTextAnnotation")]
            public FullTextAnnotation? FullTextAnnotation { get; set; }
        }

        public class FullTextAnnotation
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }

            [JsonPropertyName("pages")]
            public List<Page>? Pages { get; set; }
        }

        public class Page
        {
            [JsonPropertyName("blocks")]
            public List<Block>? Blocks { get; set; }
        }

        public class Block
        {
            [JsonPropertyName("paragraphs")]
            public List<Paragraph>? Paragraphs { get; set; }
        }

        public class Paragraph
        {
            [JsonPropertyName("words")]
            public List<Word>? Words { get; set; }
        }

        public class Word
        {
            [JsonPropertyName("symbols")]
            public List<Symbol>? Symbols { get; set; }
        }

        public class Symbol
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

    }
}
