using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using BlazorLeaflet.Exceptions;
using BlazorLeaflet.Models;
using BlazorLeaflet.Models.Events;
using BlazorLeaflet.Utils;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorLeaflet;

public class Map
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ObservableCollection<Layer> _layers = new();

    private bool _isInitialized;

    public Map(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        Id = StringHelper.GetRandomString(10);

        _layers.CollectionChanged += async (sender, args) => await OnLayersChanged(sender, args);
    }

    private LatLng _Center = new LatLng();
    /// <summary>
    ///     Geographic center of the map
    /// </summary>
    public LatLng Center
    {
        get => _Center;
        set
        {
            _Center = value;
            if (_isInitialized)
                RunTaskInBackground(async () => await LeafletInterops.PanTo(
                    _jsRuntime, Id, value.ToPointF(), false, 0, 0, false));
        }
    }

    private float _Zoom;
    /// <summary>
    ///     Map zoom level
    /// </summary>
    public float Zoom
    {
        get => _Zoom;
        set
        {
            _Zoom = value;
            if (_isInitialized)
                RunTaskInBackground(async () => await LeafletInterops.SetZoom(
                    _jsRuntime, Id, value));
        }
    }
    /// <summary>
    ///     Minimum zoom level of the map. If not specified and at least one
    ///     GridLayer or TileLayer is in the map, the lowest of their minZoom
    ///     options will be used instead.
    /// </summary>
    public float? MinZoom { get; set; }

    /// <summary>
    ///     Maximum zoom level of the map. If not specified and at least one
    ///     GridLayer or TileLayer is in the map, the highest of their maxZoom
    ///     options will be used instead.
    /// </summary>
    public float? MaxZoom { get; set; }

    /// <summary>
    ///     When this option is set, the map restricts the view to the given
    ///     geographical bounds, bouncing the user back if the user tries to pan
    ///     outside the view.
    /// </summary>
    public Tuple<LatLng, LatLng>? MaxBounds { get; set; }

    /// <summary>
    ///     Whether a zoom control is added to the map by default.
    ///     <para />
    ///     Defaults to true.
    /// </summary>
    public bool ZoomControl { get; set; } = true;

    public string Id { get; }

    /// <summary>
    ///     Event raised when the component has finished its first render.
    /// </summary>
    public event Action? OnInitialized;

    /// <summary>
    ///     This method MUST be called only once by the Blazor component upon rendering, and never by the user.
    /// </summary>
    public void RaiseOnInitialized()
    {
        _isInitialized = true;
        OnInitialized?.Invoke();
    }

    private async void RunTaskInBackground(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (Exception ex)
        {
            NotifyBackgroundExceptionOccurred(ex);
        }
    }
    
     /// <summary>
     ///     Add a layer to the map.
     /// </summary>
     /// <param name="layer">The layer to be added.</param>
     /// <exception cref="System.ArgumentNullException">Throws when the layer is null.</exception>
     /// <exception cref="UninitializedMapException">Throws when the map has not been yet initialized.</exception>
    public void AddLayer(Layer layer)
    {
        if (layer is null)
            throw new ArgumentNullException(nameof(layer));

        if (!_isInitialized)
            throw new UninitializedMapException();

        _layers.Add(layer);
    }

    /// <summary>
    ///     Remove a layer from the map.
    /// </summary>
    /// <param name="layer">The layer to be removed.</param>
    /// <exception cref="System.ArgumentNullException">Throws when the layer is null.</exception>
    /// <exception cref="UninitializedMapException">Throws when the map has not been yet initialized.</exception>
    public void RemoveLayer(Layer layer)
    {
        if (layer is null)
            throw new ArgumentNullException(nameof(layer));

        if (!_isInitialized)
            throw new UninitializedMapException();

        _layers.Remove(layer);
    }

    /// <summary>
    ///     Get a read only collection of the current layers.
    /// </summary>
    /// <returns>A read only collection of layers.</returns>
    public IEnumerable<Layer> GetLayers() => _layers.ToList().AsReadOnly();

    // ReSharper disable once UnusedParameter.Local
    private async Task OnLayersChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                foreach (var item in args.NewItems!)
                {
                    var layer = item as Layer;
                    await LeafletInterops.AddLayer(_jsRuntime, Id, layer);
                }

                break;
            }
            case NotifyCollectionChangedAction.Remove:
            {
                foreach (var item in args.OldItems!)
                    if (item is Layer layer)
                        await LeafletInterops.RemoveLayer(_jsRuntime, Id, layer.Id);
                break;
            }
            case NotifyCollectionChangedAction.Replace or NotifyCollectionChangedAction.Move:
            {
                foreach (var oldItem in args.OldItems!)
                    if (oldItem is Layer layer)
                        await LeafletInterops.RemoveLayer(_jsRuntime, Id, layer.Id);

                foreach (var newItem in args.NewItems!)
                    if (newItem is Layer layer)
                        await LeafletInterops.AddLayer(_jsRuntime, Id, layer);
                break;
            }
        }
    }

    public async Task FitBounds(PointF corner1, PointF corner2, PointF? padding = null, float? maxZoom = null)
        => await LeafletInterops.FitBounds(_jsRuntime, Id, corner1, corner2, padding, maxZoom);

    public async Task PanTo(PointF position, bool animate = false, float duration = 0.25f, float easeLinearity = 0.25f, bool noMoveStart = false)
        => await LeafletInterops.PanTo(_jsRuntime, Id, position, animate, duration, easeLinearity, noMoveStart);

    public async Task<LatLng> GetCenter()
        => await LeafletInterops.GetCenter(_jsRuntime, Id);

    public async Task<float> GetZoom()
        => await LeafletInterops.GetZoom(_jsRuntime, Id);

    /// <summary>
    ///     Increases the zoom level by one notch.
    ///     If <c>shift</c> is held down, increases it by three.
    /// </summary>
    public async Task ZoomIn(MouseEventArgs e)
        => await LeafletInterops.ZoomIn(_jsRuntime, Id, e);

    /// <summary>
    ///     Decreases the zoom level by one notch.
    ///     If <c>shift</c> is held down, decreases it by three.
    /// </summary>
    public async Task ZoomOut(MouseEventArgs e)
        => await LeafletInterops.ZoomOut(_jsRuntime, Id, e);
    private async Task UpdateZoom()
    {
        _Zoom = await GetZoom();
    }

    private async Task UpdateCenter()
    {

        _Center = await GetCenter();
    }
    #region events

    public delegate void MapEventHandler(object sender, Event e);

    public delegate void MapResizeEventHandler(object sender, ResizeEvent e);

    public event MapEventHandler? OnZoomLevelsChange;

    [JSInvokable]
    public void NotifyZoomLevelsChange(Event e)
        => OnZoomLevelsChange?.Invoke(this, e);

    public event MapResizeEventHandler? OnResize;

    [JSInvokable]
    public void NotifyResize(ResizeEvent e)
        => OnResize?.Invoke(this, e);

    public event MapEventHandler? OnUnload;

    [JSInvokable]
    public void NotifyUnload(Event e)
        => OnUnload?.Invoke(this, e);

    public event MapEventHandler? OnViewReset;

    [JSInvokable]
    public void NotifyViewReset(Event e)
        => OnViewReset?.Invoke(this, e);

    public event MapEventHandler? OnLoad;

    [JSInvokable]
    public void NotifyLoad(Event e)
        => OnLoad?.Invoke(this, e);

    public event MapEventHandler? OnZoomStart;

    [JSInvokable]
    public void NotifyZoomStart(Event e)
        => OnZoomStart?.Invoke(this, e);

    public event MapEventHandler? OnMoveStart;

    [JSInvokable]
    public void NotifyMoveStart(Event e)
        => OnMoveStart?.Invoke(this, e);

    public event MapEventHandler? OnZoom;

    [JSInvokable]
    public void NotifyZoom(Event e)
        => OnZoom?.Invoke(this, e);

    public event MapEventHandler? OnMove;

    [JSInvokable]
    public void NotifyMove(Event e)
        => OnMove?.Invoke(this, e);

    public event MapEventHandler? OnZoomEnd;

    [JSInvokable]
    public async void NotifyZoomEnd(Event e)
    {
        try
        {
            await UpdateZoom();
        }
        finally
        {
            OnZoomEnd?.Invoke(this, e);
        }
    }

    public event MapEventHandler? OnMoveEnd;

    [JSInvokable]
    public async void NotifyMoveEnd(Event e)
    {
        try
        {
            await UpdateCenter();
        }
        finally
        {
            OnMoveEnd?.Invoke(this, e);
        }
    }
    public event MouseEventHandler? OnMouseMove;

    [JSInvokable]
    public void NotifyMouseMove(MouseEvent eventArgs)
        => OnMouseMove?.Invoke(this, eventArgs);

    public event MapEventHandler? OnKeyPress;

    [JSInvokable]
    public void NotifyKeyPress(Event eventArgs)
        => OnKeyPress?.Invoke(this, eventArgs);

    public event MapEventHandler? OnKeyDown;

    [JSInvokable]
    public void NotifyKeyDown(Event eventArgs)
        => OnKeyDown?.Invoke(this, eventArgs);

    public event MapEventHandler? OnKeyUp;

    [JSInvokable]
    public void NotifyKeyUp(Event eventArgs)
        => OnKeyUp?.Invoke(this, eventArgs);

    public event MouseEventHandler? OnPreClick;

    [JSInvokable]
    public void NotifyPreClick(MouseEvent eventArgs)
        => OnPreClick?.Invoke(this, eventArgs);
    public event EventHandler<Exception> BackgroundExceptionOccurred;
    
    private void NotifyBackgroundExceptionOccurred(Exception exception) =>
        BackgroundExceptionOccurred?.Invoke(this, exception);

    #endregion events

    #region InteractiveLayerEvents

    // Has the same events as InteractiveLayer, but it is not a layer. 
    // Could place this code in its own class and make Layer inherit from that, but not every layer is interactive...
    // Is there a way to not duplicate this code?

    public delegate void MouseEventHandler(Map sender, MouseEvent e);

    public event MouseEventHandler? OnClick;

    [JSInvokable]
    public void NotifyClick(MouseEvent eventArgs)
        => OnClick?.Invoke(this, eventArgs);

    public event MouseEventHandler? OnDblClick;

    [JSInvokable]
    public void NotifyDblClick(MouseEvent eventArgs)
        => OnDblClick?.Invoke(this, eventArgs);

    public event MouseEventHandler? OnMouseDown;

    [JSInvokable]
    public void NotifyMouseDown(MouseEvent eventArgs)
        => OnMouseDown?.Invoke(this, eventArgs);

    public event MouseEventHandler? OnMouseUp;

    [JSInvokable]
    public void NotifyMouseUp(MouseEvent eventArgs)
        => OnMouseUp?.Invoke(this, eventArgs);

    public event MouseEventHandler? OnMouseOver;

    [JSInvokable]
    public void NotifyMouseOver(MouseEvent eventArgs)
        => OnMouseOver?.Invoke(this, eventArgs);

    public event MouseEventHandler? OnMouseOut;

    [JSInvokable]
    public void NotifyMouseOut(MouseEvent eventArgs)
        => OnMouseOut?.Invoke(this, eventArgs);

    public event MouseEventHandler? OnContextMenu;

    [JSInvokable]
    public void NotifyContextMenu(MouseEvent eventArgs)
        => OnContextMenu?.Invoke(this, eventArgs);

    #endregion InteractiveLayerEvents
}