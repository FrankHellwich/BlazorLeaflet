function demoPointToLayer(geoJsonPoint, latlng, objectReference) {
        var myIcon = L.icon({
            iconUrl: 'img/restaurant.png',
            iconSize: [32, 37],
            iconAnchor: [22, 94],
            popupAnchor: [-3, -76],
            shadowUrl: '/img/shadow.png',
            shadowSize: [51, 37],
            shadowAnchor: [22, 94]
        });
        return L.marker([latlng.lat, latlng.lng],
            {
                icon: myIcon,
                title:'demoPointToLayer',
                riseOnHover:true
            }
        );
    }

function demoFilter(geoJsonFeature, objectReference) {
    return true;
}

async function demoOnEachPoiFeature(feature, layer, objectReference) {
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
    }
