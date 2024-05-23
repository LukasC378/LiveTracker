import {useMap} from "react-leaflet";
import React, {useEffect} from "react";

const MapFocusComponent = React.memo((props: any) => {
    const map = useMap();

    useEffect(() => {
        map.setView([props.latitude, props.longitude], map.getZoom());
        map.dragging.disable()

        return () => {
            map.dragging.enable()
        }
    }, [props]);
    return (<></>);
});

export default MapFocusComponent