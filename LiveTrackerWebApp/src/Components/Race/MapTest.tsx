import React from "react";
import {MapContainer, TileLayer, CircleMarker, Tooltip, Marker} from "react-leaflet";
import MarkerClusterGroup from "react-leaflet-cluster";
import L from "leaflet";

const MapTestComponent = () => {

    return(
        <MapContainer
            className="markercluster-map"
            center={[-6.906454, 107.6439]}
            zoom={10}
            maxZoom={18}
        >
            <TileLayer
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
            />
            {/*<MarkerClusterGroup*/}
            {/*    onMouseOver={(e) => {*/}
            {/*        let text = ''*/}
            {/*        const cluster = e.propagatedFrom as L.MarkerCluster;*/}
            {/*        cluster.getAllChildMarkers().sort((x, y) => {*/}
            {/*            const key1 = +(x.getTooltip()!.options as any).children.key*/}
            {/*            const key2 = +(y.getTooltip()!.options as any).children.key*/}
            {/*            return key1 - key2*/}
            {/*        }).forEach(x => {*/}
            {/*            const tooltip = (x.getTooltip()!.options as any).children.props.children.props.children as string*/}
            {/*            text += tooltip + '<br>'*/}
            {/*        })*/}
            {/*        e.propagatedFrom.bindTooltip(text).openTooltip();*/}
            {/*    }}*/}
            {/*    showCoverageOnHover={false}*/}
            {/*    spiderfyOnMaxZoom={true}*/}
            {/*>*/}
            {/*    <CircleMarker*/}
            {/*        key={1}*/}
            {/*        center={[-6.88077, 107.579187]}*/}
            {/*        radius={8}*/}
            {/*    >*/}
            {/*        <Tooltip permanent>*/}
            {/*            <div key={1}>*/}
            {/*                <strong>driver 1</strong>*/}
            {/*            </div>*/}
            {/*        </Tooltip>*/}
            {/*    </CircleMarker>*/}
            {/*    <CircleMarker key={2} center={[-6.882981, 107.573238]} radius={8}>*/}
            {/*        <Tooltip permanent>*/}
            {/*            <div key={2}>*/}
            {/*                <strong>driver 2</strong>*/}
            {/*            </div>*/}
            {/*        </Tooltip>*/}
            {/*    </CircleMarker>*/}
            {/*    <CircleMarker key={3} center={[-6.868206, 107.588659]} radius={8}>*/}
            {/*        <Tooltip permanent>*/}
            {/*            <div key={3}>*/}
            {/*                <strong>driver 3</strong>*/}
            {/*            </div>*/}
            {/*        </Tooltip>*/}
            {/*    </CircleMarker>*/}
            {/*</MarkerClusterGroup>*/}

            <Marker position={[50.445364681281745, 5.962675124949451]} />
            <Marker position={[50.446701318718254, 5.964162875050549]} />
            <Marker position={[50.444919318718256, 5.965763875050549 ]} />
            <Marker position={[50.44358268128175, 5.964276124949451]} />

            {/*<Marker position={[50.44262565759035, 5.966607543716366]} />*/}
            {/*<Marker position={[50.44431765759035, 5.965094543716366]} />*/}
            {/*<Marker position={[50.44249234240966, 5.966458456283634]} />*/}
            {/*<Marker position={[50.444184342409656, 5.964945456283634]} />*/}

            <CircleMarker
                key={1}
                center={[48.13577023396422, 17.14038481218521]}
                radius={8}
            />
        </MapContainer>
    )
}

export default MapTestComponent;