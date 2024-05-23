import {Col, Row} from "react-bootstrap";
import {GeoJSON, MapContainer, TileLayer} from "react-leaflet";
import {Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow} from "@mui/material";
import React, {useEffect, useRef, useState} from "react";
import SessionService from "../../Services/SessionService";
import {useParams} from "react-router-dom";
import {GeoJsonObject} from "geojson";
import {Constants} from "../../Constants";
import {SessionDriverVM} from "../../Models/Session";
import GeoJsonFit from "./GeoJsonFit";
import LoadingComponent from "../Loading";

const RacePreviewComponent = () => {

    const [isLoading, setIsLoading]: [boolean, any] = useState(true);
    const [drivers, setDrivers]: [SessionDriverVM[], any] = useState([]);
    const [geoJson, setGeoJson]: [GeoJsonObject, any] = useState(Constants.defaultGeoJson);

    const laps: React.MutableRefObject<number> = useRef(0);
    const needCenterMap: React.MutableRefObject<boolean> = useRef(true);

    const {sessionId} = useParams();

    useEffect(() => {
        SessionService.getSession(+sessionId!).then(res => {
            setDrivers(() => res.data.drivers)
            laps.current = res.data.laps;
            const parsedGeoJsonObject = JSON.parse(res.data.geoJson);
            setGeoJson(parsedGeoJsonObject);
            setIsLoading(() => false)
        })
    }, []);

    function centerMap(){
        needCenterMap.current = false;
        return (<GeoJsonFit geoJsonData={geoJson}/>)
    }

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return(
        <Row>
            <Col>
                <MapContainer
                    id={"map-id"}
                    zoom={13}
                    maxZoom={18}
                    scrollWheelZoom={true}
                >
                    <>
                        <TileLayer
                            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                            attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                        />
                        {
                            needCenterMap.current && centerMap()
                        }
                        <GeoJSON key={"trackGeoJson"} data={geoJson} />
                    </>
                </MapContainer>
            </Col>
            <Col>

                {
                    laps.current > 0 &&
                    <div className={'mt-3'}>
                        <strong>Laps {laps.current}</strong>
                    </div>
                }

                <div style={{maxHeight: '900px', overflowY: 'auto'}}>
                    <TableContainer component={Paper}>
                        <Table sx={{ minWidth: 650 }} aria-label="drivers table">
                            <TableHead>
                                <TableRow>
                                    <TableCell className={"white-font"}>Num</TableCell>
                                    <TableCell className={"white-font"}>Name</TableCell>
                                    <TableCell></TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {drivers.map((driver, index) => (
                                    <TableRow
                                        key={index}
                                        sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                    >
                                        <TableCell>#{driver.number}</TableCell>
                                        <TableCell>{driver.name}</TableCell>
                                        <TableCell>
                                            <div style={{width: "20px", height: "20px", backgroundColor: driver.color}}></div>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                </div>
            </Col>
        </Row>
    )
}

export default RacePreviewComponent;