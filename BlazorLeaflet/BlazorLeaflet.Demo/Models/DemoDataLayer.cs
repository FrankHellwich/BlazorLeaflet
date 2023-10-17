using GeoJSON.Text.Feature;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorLeaflet.Demo.Models
{
    public record DemoDataLayer: GeoJsonDataLayer
    {
        [JSInvokable]
        public Task<string> GetFeatureData(string featureData)
        {
            var feature = JsonSerializer.Deserialize<Feature>(featureData);
            string data = "";
            foreach (var prop in feature.Properties)
            {
                data += $"<br>{prop.Key}: {prop.Value}";
            }
            return Task<string>.FromResult(data);
        }
    }
}
