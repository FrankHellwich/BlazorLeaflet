window.leafletBlazorDemo = {
    demoPointToLayer: function (geoJsonPoint, latlng, objectReference) {
        var myIcon = L.icon({
            iconUrl: 'img/restaurant.png',
            iconSize: [32, 37],
            shadowUrl: '/img/shadow.png',
            shadowSize: [51, 37],
        });
        return L.marker([latlng.lat, latlng.lng],
            {
                icon: myIcon,
                title:'demoPointToLayer',
                riseOnHover:true
            }
        );
    },

    demoFilter: function (geoJsonFeature, objectReference) {
        return true;
    },

    demoOnEachPoiFeature: async function (feature, layer, objectReference) {
        layer.on('click', async (e) => {
            //destroy any old popups that might be attached
            if (layer.isPopupOpen()) {
                layer.unbindPopup();
            }
            try {
                const response = await fetch('sample-data/popupcontent.html');
                let popupContent = await response.text() + "<br>";

                let featureData = await objectReference.invokeMethodAsync('GetFeatureData', JSON.stringify(feature));
                popupContent += featureData;   
                layer.closePopup();
                layer.bindPopup(popupContent);
                layer.openPopup();
            } catch (e) {
                console.log(e);
            }
        });
    },

    demoOnEachGeoFeature: async function (feature, layer, objectReference) {
        let props = feature.properties;
        let attrs = Object.keys(props);
        let popupContent = "";
        let attribute = "";
        let value = "";
        for (var i = 0; i < attrs.length; i += 1) {
            attribute = attrs[i];
            value = props[attribute];
            popupContent += attribute + ": " + value + "<br>"
        }
        layer.bindPopup(popupContent);
    }
}
