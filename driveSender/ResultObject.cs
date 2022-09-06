using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace driveSender
{
    public class ResultObject
    {
        [JsonPropertyName("defect_coordinates")]
        public List<int[]> DefectCoordinates { get; set; }
        [JsonPropertyName("defect_count")]
        public int DefectCount { get; set; }
    }
}
