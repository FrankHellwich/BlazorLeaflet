using GeoJSON.Text.Feature;
using System.Diagnostics;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorLeaflet.Demo.Models
{
    public record DynamicDataLayer : GeoJsonDataLayer
    {
        private Map _map;
        private HttpClient _httpClient;
        public DynamicDataLayer(Map map)
        {
            _map = map;
            _httpClient = new HttpClient();   
            OnAdd += OnAddLayer;
            OnRemove += OnRemoveLayer;
        }

        private void OnAddLayer(Layer sender, BlazorLeaflet.Models.Events.Event e)
        {
            _map.OnMoveEnd += MapOnMoveEnd;
        }

        private void OnRemoveLayer(Layer sender, BlazorLeaflet.Models.Events.Event e)
        {
            _map.OnMoveEnd -= MapOnMoveEnd;
        }

        private async void MapOnMoveEnd(object sender, BlazorLeaflet.Models.Events.Event e)
        {
            await RequeryAsync();
        }
        public float MinZoom { get; set; } = 12.0f;
        public float MaxZoom { get; set; }  

        public string OverpassQuery { get; set; }
        private string _overpassServer = @"https://lz4.overpass-api.de/api/";
        public async Task RequeryAsync()
        {
            float currentZoom = this._map.Zoom;
            if (currentZoom >= MinZoom)
            {
                await this.InternalRequeryAsync();
            }
            else
            {
                //this.clear();
            }
        }

        public async Task InternalRequeryAsync()
        {
            var bounds = await _map.GetBounds();
            var bbox = ToOverpassBBoxString(bounds);
            string queryWithMapCoordinates = this.OverpassQuery.Replace("{{bbox}}", bbox);
            string url = _overpassServer + "interpreter?data=[out:json];" + queryWithMapCoordinates;
            using (var req = await _httpClient.GetAsync(url))
            {
                req.EnsureSuccessStatusCode();
                using var s = await req.Content.ReadAsStreamAsync();
                using var sr = new StreamReader(s) ?? throw new InvalidOperationException("StreamReader is null");
                var json = sr.ReadToEnd();
                Debug.WriteLine(json);
                //Todo: extract Features and add to layer
            }
        }

        private string ToOverpassBBoxString(LatLngBounds box ) {
            var a = box.SouthWest;
            var b = box.NorthEast;
            var res = $"{a.Lat}#{a.Lng}#{b.Lat}#{b.Lng}";
            res = res.Replace(',', '.');
            res = res.Replace('#', ',');
            return res;
        }

    }
}
