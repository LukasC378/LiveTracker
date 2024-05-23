import {useParams} from "react-router-dom";
import React, {useEffect, useRef, useState} from "react";
import {Constants} from "../../Constants";
import {Layout} from "../../Models/Layout";
import LayoutService from "../../Services/LayoutService";
import {Utils} from "../../Utils/Utils";
import {Button, Col, Form, Row} from "react-bootstrap";
import {MapContainer, TileLayer} from "react-leaflet";
import GeoJsonImage from "../Race/GeoJsonFit";
import layoutService from "../../Services/LayoutService";
import LoadingComponent from "../Loading";

const LayoutEditorComponent = () => {
    //region useParams

    const {id} = useParams();

    //endregion

    //region useRef

    const layoutId: React.MutableRefObject<number> = useRef(id == undefined ? 0 : +id);
    const oldLayout: React.MutableRefObject<Layout> = useRef(Constants.defaultLayout);
    const geoJsonValidationMessage: React.MutableRefObject<string> = useRef("");

    //endregion

    //region useState

    const [isLoading, setIsLoading]: [boolean, any] = useState(true);
    const [isCreated, setIsCreated]: [boolean, any] = useState(false);
    const [isUpdated, setIsUpdated]: [boolean, any] = useState(false);

    const [name, setName]: [string, any] = useState("")
    const [geoJson, setGeoJson]: [string, any] = useState("");

    //endregion

    //region useEffect

    useEffect(() => {
        if(layoutId.current === 0){
            setIsLoading(() => false)
            return
        }

        LayoutService.getLayout(layoutId.current).then(res => {
            oldLayout.current = {...res.data}
            setName(() => res.data.name)
            setGeoJson(() => res.data.geoJson)
            setIsLoading(() => false)
        }).catch(() => {
            setIsLoading(() => false)
        })
    }, []);

    //endregion

    //region functions

    function validateGeoJson(geoJsonString: string): boolean {

        let [result, _, message] = Utils.validateGeoJson(geoJsonString)
        if(result){
            geoJsonValidationMessage.current = '';
            return true
        }
        geoJsonValidationMessage.current = message;
        return false;
    }

    function handleSubmit(e: any){
        e.preventDefault();

        if(!validateGeoJson(geoJson)){
            return
        }

        if(layoutId.current !== 0){
            if(isLayoutChanged()){
                updateLayout()
                return;
            }
            else if(oldLayout.current.name !== name){
                renameLayout()
                return;
            }
            else{
                alert("No changes found!");
                return;
            }
        }

        createLayout()
    }

    function isLayoutChanged(): boolean{
        return oldLayout.current.name !== name && oldLayout.current.geoJson !== geoJson
    }

    function createLayout() {
        setIsLoading(() => true)
        const layout: Layout = {
            id: 0,
            name: name,
            geoJson: geoJson
        }
        layoutService.createLayout(layout).then(() => {
            setIsLoading(() => false)
            setIsCreated(() => true)
        }).catch(() => {
            setIsLoading(() => false)
        })
    }

    function updateLayout() {
        setIsLoading(() => true)
        const layout: Layout = {
            id: oldLayout.current.id,
            name: name,
            geoJson: geoJson
        }
        layoutService.updateLayout(layout).then(() => {
            setIsLoading(() => false)
            setIsUpdated(() => true)
        }).catch(() => {
            setIsLoading(() => false)
        })
    }

    function renameLayout(){
        setIsLoading(() => true)
        layoutService.renameLayout(layoutId.current, name).then(() => {
            setIsLoading(() => false)
            setIsUpdated(() => true)
        }).catch(() => {
            setIsLoading(() => false)
        })
    }

    //endregion

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    if(isCreated) {
        return (
            <div className={"container-fluid d-flex justify-content-center pt-5"} style={{maxWidth: "500px"}}>
                <div className={"p-4"}>
                    <p>Your layout was successfully created.</p>
                </div>
            </div>
        )
    }
    if(isUpdated) {
        return (
            <div className={"container-fluid d-flex justify-content-center pt-5"} style={{maxWidth: "500px"}}>
                <div className={"p-4"}>
                    <p>Your layout was successfully updated.</p>
                </div>
            </div>
        )
    }
    return(
        <div>
            <Row>
                <Col className={"d-flex justify-content-center"}>
                    <Form className={"form"} onSubmit={handleSubmit}>
                        <Form.Group controlId={"layout-editor-form"}>
                            <Form.Label>Layout name</Form.Label>
                            <Form.Control
                                type={"text"}
                                required
                                value={name}
                                onChange={e => {
                                    setName(() => e.target.value)
                                }}
                            />

                            <Form.Label>GeoJson</Form.Label>
                            <Form.Control
                                as={"textarea"}
                                style={{minHeight: '200px'}}
                                required
                                value={geoJson}
                                onChange={e => {
                                    const geoJsonString = e.target.value
                                    validateGeoJson(geoJsonString)
                                    setGeoJson(() => geoJsonString)
                                }}
                            />
                            {
                                geoJsonValidationMessage.current &&
                                <div className={"mb-3"}>
                                    <p className={"text-warning"}>{geoJsonValidationMessage.current}</p>
                                </div>
                            }
                            {
                                geoJsonValidationMessage.current === '' &&
                                <Row
                                    className={'mt-2 mb-2'}
                                >
                                    {
                                        geoJson &&
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
                                            <GeoJsonImage geoJsonData={JSON.parse(geoJson)} />
                                        </MapContainer>
                                    }
                                </Row>
                            }
                        </Form.Group>
                        <div className={'m-3'}>
                            <Button style={{width: 200}} type={"submit"} variant={"dark"}>Submit</Button>
                        </div>
                    </Form>
                </Col>
            </Row>
        </div>
    )
}

export default LayoutEditorComponent;