import React, {useEffect, useRef, useState} from 'react';
import * as signalR from '@microsoft/signalr';
import {GeoJSON, MapContainer, TileLayer} from "react-leaflet";
import {DriverData, RaceData} from "../../Models/Race";
import 'leaflet/dist/leaflet.css';
import SessionService from "../../Services/SessionService";
import {Button, Col, Row} from "react-bootstrap";
import {
    Checkbox,
    FormControlLabel,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow
} from "@mui/material";
import {GeoJsonObject} from "geojson";
import LoadingComponent from "../Loading";
import {Constants} from "../../Constants";
import MarkerClusterGroup from "react-leaflet-cluster";
import {useParams} from "react-router-dom";
import GeoJsonImage from "./GeoJsonFit";
import MapFocus from "./MapFocus";
import {SessionDriverVM} from "../../Models/Session";
import {Eye, XSquare} from "react-bootstrap-icons";
import DriverMarker from "./DriverMarker";
import {Utils} from "../../Utils/Utils";

const LiveRaceComponent = () => {

    //region constants

    const TRACK_SERVER_URL: string = import.meta.env.VITE_TRACK_SERVER_URL;

    const [isLoading, setIsLoading]: [boolean, any] = useState(true);
    const [gpsData, setGpsData]: [DriverData[], any] = useState([]);
    const [geoJson, setGeoJson]: [GeoJsonObject, any] = useState(Constants.defaultGeoJson);
    const [useCluster, setUseCluster]: [boolean, any] = useState(false);
    const [showTooltips, setShowTooltips]: [boolean, any] = useState(false);
    const [watchedDriver, setWatchedDriver]: [string, any] = useState('');

    //endregion

    const currentLap: React.MutableRefObject<number> = useRef(0);
    const laps: React.MutableRefObject<number> = useRef(0);
    const driversDict: React.MutableRefObject<{[key: string]: SessionDriverVM}> = useRef({});
    const needCenterMap: React.MutableRefObject<boolean> = useRef(true);
    const {sessionId} = useParams();

    //region useEffect

    useEffect(() => {
        SessionService.getSession(+sessionId!).then(res => {
            res.data.drivers.forEach(x => {
                driversDict.current[x.carId] = x
            })
            laps.current = res.data.laps;
            const parsedGeoJsonObject = JSON.parse(res.data.geoJson);
            setGeoJson(parsedGeoJsonObject);
            setIsLoading(() => false)
        }).catch(e => {
            setIsLoading(() => false)
            console.log(e)
        })
    }, []);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(TRACK_SERVER_URL)
            .build();

        connection
            .start()
            .then(() => {
                console.log('WebSocket Connection Established');
                connection
                    .invoke('JoinRace', sessionId!.toString())
                    .then(() => console.log('Joined the race'))
                    .catch(error => console.error(error));
            })
            .catch(error => console.error(error));

        connection.on('ReceiveRaceData', data => {
            let jsonData: RaceData = JSON.parse(data);
            //console.log(jsonData)
            currentLap.current = jsonData.lapCount;
            setGpsData(() => jsonData.driversData);
        });

        return () => {
            connection.stop().then(() => {});
        };
    }, []);

    //endregion

    //region functions

    function centerMap(){
        needCenterMap.current = false;
        return (<GeoJsonImage geoJsonData={geoJson}/>)
    }

    //endregion

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return (
        <div>
            <Row>
                <Col lg={2} md={4} sm={6}>
                    <div className={'mt-2 mb-1 ms-2 me-1'}>
                        <div className={'d-flex'}>
                            <FormControlLabel
                                value="end"
                                control={
                                    <Checkbox
                                        checked={useCluster}
                                        onChange={() => setUseCluster(() => !useCluster)}
                                    />}
                                label="Use cluster"
                                labelPlacement="end"
                            />
                        </div>
                    </div>
                </Col>
                <Col lg={2} md={4} sm={6}>
                    <div className={'mt-2 mb-1 ms-2 me-1'}>
                        <div className={'d-flex'}>
                            <FormControlLabel
                                value="end"
                                control={
                                    <Checkbox
                                        checked={showTooltips}
                                        onChange={() => setShowTooltips(() => !showTooltips)}
                                    />}
                                label="Show tooltips"
                                labelPlacement="end"
                            />
                        </div>
                    </div>
                </Col>
            </Row>
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
                            {
                                useCluster &&
                                <MarkerClusterGroup
                                    onMouseOver={Utils.showClusterTooltip}
                                    showCoverageOnHover={false}
                                    spiderfyOnMaxZoom={true}
                                >
                                    {
                                        gpsData.map((data: DriverData, index: number) => (
                                            <React.Fragment key={index}>
                                                {data.driverId === watchedDriver &&
                                                    <MapFocus latitude={data.latitude} longitude={data.longitude}/>
                                                }
                                                <DriverMarker
                                                    data={data}
                                                    driver={driversDict.current[data.driverId]}
                                                    showTooltips={showTooltips}
                                                    watchedDriver={watchedDriver}
                                                />
                                            </React.Fragment>
                                        ))
                                    }
                                </MarkerClusterGroup>
                            }
                            {
                                !useCluster && gpsData.map((data, index) => (
                                    <React.Fragment key={index}>
                                        {data.driverId === watchedDriver &&
                                            <MapFocus latitude={data.latitude} longitude={data.longitude}/>
                                        }
                                        <DriverMarker
                                            key={index}
                                            data={data}
                                            driver={driversDict.current[data.driverId]}
                                            showTooltips={showTooltips}
                                            watchedDriver={watchedDriver}
                                        />
                                    </React.Fragment>
                                ))
                            }
                        </>
                    </MapContainer>
                </Col>
                <Col>
                    {
                        laps.current > 0 && currentLap.current <= laps.current &&
                        <div>
                            <strong>Lap {currentLap.current} / {laps.current}</strong>
                        </div>
                    }
                    {
                        laps.current > 0 && currentLap.current > laps.current &&
                        <div>
                            <strong>Finish</strong>
                        </div>
                    }
                    <div style={{maxHeight: '900px', overflowY: 'auto'}}>
                        <TableContainer component={Paper}>
                            <Table sx={{ minWidth: 650 }} aria-label="drivers table">
                                <TableHead>
                                    <TableRow>
                                        <TableCell className={"white-font"}>Pos</TableCell>
                                        <TableCell className={"white-font"}>Name</TableCell>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {gpsData.map((x, index) => {
                                        let driver = driversDict.current[x.driverId]
                                        return (
                                            <TableRow
                                                key={index}
                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                            >
                                                <TableCell>{index + 1}</TableCell>
                                                <TableCell>
                                                    <div>
                                                        #{driver.number} {driver.name}
                                                        {
                                                            !Utils.isNullOrEmpty(driver.teamName) &&
                                                            <div style={{color: "gray"}}>{driver.teamName}</div>
                                                        }
                                                    </div>

                                                </TableCell>
                                                <TableCell>
                                                    <div style={{width: "20px", height: "20px", backgroundColor: driver.color}}></div>
                                                </TableCell>
                                                <TableCell>
                                                    {
                                                        driver.carId !== watchedDriver  &&
                                                        <Button onClick={() => {
                                                            setWatchedDriver(() => driver.carId)
                                                        }}>
                                                            <Eye/>
                                                        </Button>
                                                    }
                                                    {
                                                        driver.carId === watchedDriver  &&
                                                        <Button
                                                            className={'btn-danger'}
                                                            onClick={() => {
                                                                setWatchedDriver(() => '')
                                                            }}
                                                        >
                                                            <XSquare/>
                                                        </Button>
                                                    }
                                                </TableCell>
                                            </TableRow>
                                        )})}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </div>
                </Col>
            </Row>
        </div>
    );
};

export default LiveRaceComponent;
