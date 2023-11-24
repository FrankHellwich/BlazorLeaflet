using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace BlazorLeaflet.Models
{
    public class LatLngBounds
    {
        [JsonPropertyName("_northEast")]
        public LatLng NorthEast { get; set; }

        [JsonPropertyName("_southWest")]
        public LatLng SouthWest { get; set; }

        public LatLngBounds(LatLng southWest, LatLng northEast)
        {
            NorthEast = northEast;
            SouthWest = southWest;
        }

        public override string ToString() =>
            $"NE: {NorthEast.Lat} N, {NorthEast.Lng} E; SW: {SouthWest.Lat} N, {SouthWest.Lng} E";
    }
}
