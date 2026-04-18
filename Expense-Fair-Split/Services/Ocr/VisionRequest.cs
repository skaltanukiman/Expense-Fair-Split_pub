using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Ocr
{
    public class VisionRequest
    {
        [JsonPropertyName("requests")]
        public required List<Request> Requests { get; set; }

        public class Request
        {
            [JsonPropertyName("image")]
            public required Image Image { get; set; }
            [JsonPropertyName("features")]
            public required List<Feature> Features { get; set; }
        }

        public class Image
        {
            [JsonPropertyName("content")]
            public required string Content { get; set; }
        }

        public class Feature
        {
            [JsonPropertyName("type")]
            public required string Type { get; set; }
        }
    }

    
}
