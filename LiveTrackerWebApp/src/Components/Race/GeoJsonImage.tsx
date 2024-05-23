import {MapContainer, TileLayer} from "react-leaflet";
import GeoJsonFit from "./GeoJsonFit";
import React from "react";

const GeoJsonImageComponent = (props: any) => {
    return(
        <MapContainer
            center={[0, 0]}
            zoom={0}
            style={{ height: '300px', width: '100%' }}
            scrollWheelZoom={false}
        >
            <TileLayer
                url="http://{s}.tile.osm.org/{z}/{x}/{y}.png"
                attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
            />
            <GeoJsonFit geoJsonData={props.geoJsonData} />
        </MapContainer>
    )
}

export default GeoJsonImageComponent;