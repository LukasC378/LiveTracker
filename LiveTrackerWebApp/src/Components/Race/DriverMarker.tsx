import {CircleMarker, Tooltip} from "react-leaflet";
import React from "react";

const DriverMarkerComponent = (props: any) => {
    return (
        <div key={props.data.driverId}>
            <>
                <CircleMarker
                    key={props.data.driverId}
                    center={[props.data.latitude, props.data.longitude]}
                    radius={8}
                    fillColor={props.driver.color}
                    fillOpacity={1}
                >
                    {
                        (props.showTooltips || props.watchedDriver === props.data.driverId) &&
                        <Tooltip permanent>
                            <div>
                                <strong>{props.driver.name}</strong>
                            </div>
                        </Tooltip>
                    }
                    {
                        !props.showTooltips && props.watchedDriver !== props.data.driverId &&
                        <Tooltip>
                            <div>
                                <strong>{props.driver.name}</strong>
                            </div>
                        </Tooltip>
                    }
                </CircleMarker>
            </>
        </div>
    )
}

export default DriverMarkerComponent;