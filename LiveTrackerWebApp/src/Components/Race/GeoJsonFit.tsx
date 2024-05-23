import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import {useMap} from "react-leaflet";
import {useEffect} from "react";


const GeoJsonFitComponent = (props: any) => {

    const map = useMap();

    useEffect(() => {
        if(!props.geoJsonData)
            return
        const geoJsonLayer = L.geoJSON(props.geoJsonData).addTo(map);
        map.fitBounds(geoJsonLayer.getBounds());
    }, [props]);

    return (
        <></>
    );
}

export default GeoJsonFitComponent;