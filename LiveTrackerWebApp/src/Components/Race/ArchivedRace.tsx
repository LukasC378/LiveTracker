import {
    Checkbox,
    FormControlLabel,
    Paper,
    Slider,
    Table, TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow
} from "@mui/material";
import React, {useEffect, useRef, useState} from "react";
import LoadingComponent from "../Loading";
import {Button, Col, Row} from "react-bootstrap";
import {Eye, Pause, Play, SkipBackward, SkipForward, XSquare} from "react-bootstrap-icons";
import {useParams} from "react-router-dom";
import SessionService from "../../Services/SessionService";
import {SessionDriverVM} from "../../Models/Session";
import {DriverData, RaceData} from "../../Models/Race";
import {GeoJsonObject} from "geojson";
import {Constants} from "../../Constants";
import ArchiveService from "../../Services/ArchiveService";
import {GeoJSON, MapContainer, TileLayer} from "react-leaflet";
import MarkerClusterGroup from "react-leaflet-cluster";
import {Utils} from "../../Utils/Utils";
import GeoJsonImage from "./GeoJsonFit";
import DriverMarker from "./DriverMarker";
import LoadingComponentSmall from "../LoadingSmall";
import MapFocus from "./MapFocus";

const ArchivedRaceComponent = () => {

    const part: number = 5000

    const {sessionId} = useParams();

    const totalCount: React.MutableRefObject<number> = useRef(0)
    const totalRaceTime: React.MutableRefObject<number> = useRef(0)

    const timer: React.MutableRefObject<NodeJS.Timer|null> = useRef(null)
    const currentArray: React.MutableRefObject<RaceData[]> = useRef([])
    const nextArray: React.MutableRefObject<RaceData[]> = useRef([])
    const currentArrayIndex: React.MutableRefObject<number> = useRef(0);
    const currentFrameIndex: React.MutableRefObject<number> = useRef(0);
    const currentSliderIndex: React.MutableRefObject<number> = useRef(0);
    const currentStart: React.MutableRefObject<number> = useRef(0);
    const isSliderMoving: React.MutableRefObject<boolean> = useRef(false)

    const currentLap: React.MutableRefObject<number> = useRef(0);
    const laps: React.MutableRefObject<number> = useRef(0);
    const driversDict: React.MutableRefObject<{[key: string]: SessionDriverVM}> = useRef({});
    const needCenterMap: React.MutableRefObject<boolean> = useRef(true);

    const [isLoading, setIsLoading]: [boolean, any] = useState(true)
    const [isLoadingData, setIsLoadingData]: [boolean, any] = useState(false)
    const [, forceUpdate]: [any, any] = useState();

    const [gpsData, setGpsData]: [DriverData[], any] = useState([]);
    const [geoJson, setGeoJson]: [GeoJsonObject, any] = useState(Constants.defaultGeoJson);
    const [useCluster, setUseCluster]: [boolean, any] = useState(false);
    const [showTooltips, setShowTooltips]: [boolean, any] = useState(false);
    const [watchedDriver, setWatchedDriver]: [string, any] = useState('');


    const delay = () => {
        return Math.floor(totalRaceTime.current/totalCount.current * 1000)
    }
    const framesPerSecond = (): number => {
        return Math.floor(totalCount.current / totalRaceTime.current)
    }

    useEffect(() => {
        async function getSession(){
            let res = await SessionService.getSession(+sessionId!);
            console.log(res.data)
            res.data.drivers.forEach(x => {
                driversDict.current[x.carId] = x
            })
            laps.current = res.data.laps;
            const parsedGeoJsonObject = JSON.parse(res.data.geoJson);
            setGeoJson(parsedGeoJsonObject);
        }

        async function loadFirstData(){
            await loadDataAsync()
            loadNextDataAsync().then(() => {})
        }

        async function loadTotalCount(){
            let res = await ArchiveService.getTotalCount(+sessionId!)
            totalCount.current = res.data.count;
            totalRaceTime.current = res.data.time;
        }

        Promise.all([getSession(), loadFirstData(), loadTotalCount()]).then(() => {
            setIsLoading(() => false)
            console.log(':)')
            startTimer()
        })

    }, []);

    function startTimer(){
        if(currentFrameIndex.current >= totalCount.current - 1){
            return
        }

        timer.current = setInterval(() => {
            //console.log("tick")
            if(isLoadingData){
                return;
            }

            if(currentArrayIndex.current < 0){
                currentArrayIndex.current = 0
            }
            else if(currentArrayIndex.current >= currentArray.current.length){
                if(nextArray.current.length === 0){
                    setIsLoadingData(() => true)
                    return;
                }
                currentArray.current = [...nextArray.current]
                currentStart.current = currentFrameIndex.current
                //console.log('exchange ', currentStart.current)
                currentArrayIndex.current = currentArrayIndex.current % part
                //console.log(currentArrayIndex.current)
                loadNextDataAsync().then(() => {})
            }
            show()
            incrementCurrentFrame()
        }, delay())
    }

    function stopTimer(){
        if(timer.current != null){
            clearInterval(timer.current)
            timer.current = null
        }
    }

    function handleSliderChange() {
        const val = currentSliderIndex.current
        isSliderMoving.current = false
        console.log('val ', val)

        if(isInMainArray(val)){
            currentArrayIndex.current = val - currentStart.current
            currentFrameIndex.current = val
        }
        else if(isInNextArray(val)){
            const waitForNextArray = () => {
                if (nextArray.current.length > 0) {
                    currentArray.current = [...nextArray.current];
                    currentStart.current += part;
                    currentArrayIndex.current = val - currentStart.current;
                    nextArray.current = [];
                    loadNextDataAsync().then(() => {});
                    if(timer.current == null){
                        startTimer()
                    }
                } else {
                    stopTimer()
                    setIsLoadingData(() => true)
                    setTimeout(waitForNextArray, 100);
                }
            }
            waitForNextArray()
            currentFrameIndex.current = val
        }
        else{
            stopTimer()
            currentFrameIndex.current = val
            setIsLoadingData(() => true)
            loadDataAsync().then(() => {
                startTimer()
                loadNextDataAsync().then(() => {})
            })
        }
        console.log('frame changed to ' + currentFrameIndex.current)
    }

    function isInMainArray(newFrameIndex: number): boolean{
        let start = currentStart.current
        let end = start + part
        return newFrameIndex >= start && newFrameIndex < end
    }

    function isInNextArray(newFrameIndex: number): boolean{
        let start = currentStart.current + part
        let end = start + part
        return newFrameIndex >= start && newFrameIndex < end
    }

    async function loadDataAsync() {
        setIsLoadingData(() => true)
        currentArray.current = []
        currentArrayIndex.current = 0
        currentStart.current = currentFrameIndex.current

        ArchiveService.getChunk(+sessionId!, currentStart.current, part).then(res => {
            currentArray.current = res.data
            console.log('Data loaded');
            console.log(currentArray.current);
            setIsLoadingData(() => false)
        })
    }

    async function loadNextDataAsync(){
        nextArray.current = []

        ArchiveService.getChunk(+sessionId!, currentStart.current + part, part).then(res => {
            nextArray.current = res.data
            console.log('Next data loaded')
            console.log(nextArray.current)
            setIsLoadingData(() => false)
        })
    }

    function show(){
        let item = currentArray.current[currentArrayIndex.current]
        laps.current = item.lapCount
        setGpsData(() => item.driversData)
    }

    const calculateCurrentTime = () => {
        const percentComplete = currentFrameIndex.current / (totalCount.current - 1);
        const currentTimeInSeconds = percentComplete * totalRaceTime.current;
        return calculateTime(currentTimeInSeconds);
    };

    const calculateTotalTime = () => {
        return calculateTime(totalRaceTime.current);
    };

    function calculateTime(timeInSeconds: number) {
        const hours = Math.floor(timeInSeconds / 3600);
        const minutes = Math.floor((timeInSeconds % 3600) / 60);
        const seconds = Math.floor(timeInSeconds % 60);

        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

    function incrementCurrentFrame(inc: number = 1){
        currentArrayIndex.current += inc;
        let val = currentFrameIndex.current + inc

        if(val >= totalCount.current - 1){
            currentArrayIndex.current = currentArray.current.length - 1
            val = totalCount.current - 1
            stopTimer()
        }

        currentFrameIndex.current = val

        if(!isSliderMoving.current){
            currentSliderIndex.current = val
            forceUpdate({})
            //setCurrentSliderIndex(val)
        }
    }

    function handleSkip(inc: number){
        let val = currentFrameIndex.current + inc
        let stop = false

        if(val > totalCount.current - 1){
            currentArrayIndex.current = currentArray.current.length - 1
            val = totalCount.current - 1
            stop = true
            stopTimer()
        }
        else if(val < 0){
            currentArrayIndex.current = 0
            val = 0
        }

        if(!stop && timer.current == null){
            startTimer()
        }

        currentSliderIndex.current = val
        handleSliderChange()
    }

    function handleSliderMove(e: any) {
        currentSliderIndex.current = Math.round((e.target.value / 100) * (totalCount.current - 1))
        forceUpdate({})
        //setCurrentSliderIndex(val)
    }

    function secondsToFrames(seconds: number): number{
        return seconds * framesPerSecond()
    }

    function centerMap(){
        needCenterMap.current = false;
        return (<GeoJsonImage geoJsonData={geoJson}/>)
    }


    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return(
        <>
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
                        {
                            gpsData.length > 0 && !isLoadingData &&
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
                        }
                        {
                            (gpsData.length === 0 || isLoadingData) &&
                            <LoadingComponentSmall />
                        }
                        <div>
                            <Slider
                                value={(currentSliderIndex.current / totalCount.current) * 100}
                                onChange={handleSliderMove}
                                onMouseDown={() => isSliderMoving.current = true}
                                onChangeCommitted={handleSliderChange}
                            />
                            <div className={'d-flex'}>
                                {calculateCurrentTime()}/{calculateTotalTime()}
                            </div>
                            {/*<div>*/}
                            {/*    {currentFrameIndex.current}*/}
                            {/*</div>*/}
                            <div className={'d-flex justify-content-center mb-3'}>
                                <Button
                                    disabled={isLoadingData}
                                    onClick={() => {
                                        handleSkip(-1 * secondsToFrames(30))
                                    }}
                                >
                                    <SkipBackward/>
                                </Button>
                                <Button
                                    disabled={isLoadingData}
                                    onClick={() => {
                                        if(timer.current != null)
                                            stopTimer()
                                        else
                                            startTimer()
                                        forceUpdate({})
                                    }}
                                >
                                    {
                                        timer.current != null &&
                                        <Pause />
                                    }
                                    {
                                        timer.current == null &&
                                        <Play />
                                    }
                                </Button>
                                <Button
                                    disabled={isLoadingData}
                                    onClick={() => {
                                        handleSkip(secondsToFrames(30))
                                    }}
                                >
                                    <SkipForward/>
                                </Button>
                            </div>
                        </div>
                    </Col>
                    <Col>
                        {
                            laps.current > 0 &&
                            <div>
                                <strong>Lap {currentLap.current} / {laps.current}</strong>
                            </div>
                        }
                        <div style={{maxHeight: '900px', overflowY: 'auto'}}>
                            {
                                gpsData.length > 0 && !isLoadingData &&
                                <TableContainer component={Paper}>
                                    <Table sx={{ minWidth: 650 }} aria-label="drivers table">
                                        <TableHead>
                                            <TableRow>
                                                <TableCell className={"white-font"}>Pos</TableCell>
                                                <TableCell className={"white-font"}>Name</TableCell>
                                                <TableCell></TableCell>
                                                <TableCell></TableCell>
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
                                                        #{driver.number}
                                                    </TableCell>
                                                    <TableCell>
                                                        {driver.name}
                                                    </TableCell>
                                                    <TableCell>
                                                        {
                                                            !Utils.isNullOrEmpty(driver.teamName) &&
                                                            <div style={{color: "gray"}}>{driver.teamName}</div>
                                                        }
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
                            }
                            {
                                (gpsData.length === 0 || isLoadingData) &&
                                <LoadingComponentSmall />
                            }
                        </div>
                    </Col>
                </Row>
            </div>
        </>
    )
}

export default ArchivedRaceComponent;