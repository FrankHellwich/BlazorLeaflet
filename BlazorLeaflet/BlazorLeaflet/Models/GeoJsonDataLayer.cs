using System;

namespace BlazorLeaflet.Models;

/// <summary>
/// a mix of geoJSON-data to add to the map and the options
/// </summary>
public record GeoJsonDataLayer : InteractiveLayer
{
    /// <summary>
    /// the GeoJSON feature/feature-collection
    /// </summary>
    public string? GeoJsonData { get; set; }

    /// <summary>
    /// a string reference to a js-function (interop may cause performance issues)
    /// A Function defining how GeoJSON points spawn Leaflet layers.
    /// It is internally called when data is added, passing the GeoJSON point
    /// feature and its LatLng.
    /// 
    /// The default is to spawn a default Marker:
    /// ...
    /// function(geoJsonPoint, latlng, layerObjectReference) {
    ///     return L.marker(latlng);
    /// }
    /// ...
    /// </summary>
    public string? PointToLayerFuncName { get; set; }

    /// <summary>
    /// a string reference to a js-function (interop may cause performance issues)
    /// A Function that will be called once for each created Feature, after it
    /// has been created and styled. Useful for attaching events and popups to features.
    /// 
    /// The default is to do nothing with the newly created layers:
    /// 
    /// ...
    /// function (feature, layer, layerObjectReference) {}
    /// ...
    /// </summary>
    public string? OnEachFeatureFuncName { get; set; }

    /// <summary>
    /// Path options for styling GeoJSON lines and polygons
    /// called internally when data is added.
    /// </summary>
    public Path? Style { get; set; }

    /// <summary>
    /// a string reference to a js-function (interop may cause performance issues)
    /// A Function that will be used to decide whether to show a feature or not.
    /// The default is to show all features:
    /// ...
    /// function (geoJsonFeature) {
    ///    return true;
    ///    }
    /// ...
    /// </summary>
    public string? FilterFuncName {get;set;}

    /// <summary>
    /// a string reference to a js-function (interop may cause performance issues)
    /// A Function that will be used for converting GeoJSON coordinates to LatLngs.
    ///  The default is the coordsToLatLng static method.
    ///  function(coords: [number, number] | [number, number, number]): LatLng
    /// </summary>
    public string? CoordsToLatLngFuncName { get; set; }

    /// <summary>
    /// Whether default Markers for "Point" type Features inherit from group options.
    /// </summary>
    public bool MarkersInheritOptions { get; set; } = true;
}