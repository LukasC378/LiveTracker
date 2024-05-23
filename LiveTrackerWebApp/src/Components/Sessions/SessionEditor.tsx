import {Button, Col, Form, Modal, Row} from "react-bootstrap";
import React, {useEffect, useRef, useState} from "react";
import "./Sessions.css"
import SessionService from "../../Services/SessionService";
import LoadingComponent from "../Loading";
import {Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow} from "@mui/material";
import {Eye, Trash} from "react-bootstrap-icons";
import CollectionService from "../../Services/CollectionService";
import CollectionView from "../Collections/CollectionView";
import {Session} from "../../Models/Session";
import {CollectionBasicVM} from "../../Models/Collection";
import {Utils} from "../../Utils/Utils";
import {Constants} from "../../Constants";
import LayoutService from "../../Services/LayoutService";
import {Layout} from "../../Models/Layout";
import {useParams} from "react-router-dom";
import GeoJsonImage from "../Race/GeoJsonImage";
import TextMessage from "../TextMessage";

const SessionEditorComponent = () => {

    //region useParams

    const {id} = useParams();

    //endregion

    //region useRef

    const sessionId: React.MutableRefObject<number> = useRef(id == undefined ? 0 : +id);
    const oldSession: React.MutableRefObject<Session> = useRef(Constants.defaultSession);

    const collectionsLoaded: React.MutableRefObject<boolean> = useRef(false);
    const layoutsLoaded: React.MutableRefObject<boolean> = useRef(false);

    const collectionToShow: React.MutableRefObject<number> = useRef(0);
    const selectedCollectionName: React.MutableRefObject<string> = useRef("");
    const selectedLayoutName: React.MutableRefObject<string> = useRef("");
    const geoJsonValidationMessage: React.MutableRefObject<string> = useRef("");
    const isCircuit: React.MutableRefObject<boolean> = useRef(false);

    //endregion

    //region useState

    const [session, setSession]: [Session, any] = useState(Constants.defaultSession);
    const [isCreated, setIsCreated]: [boolean, any] = useState(false);
    const [isUpdate, setIsUpdated]: [boolean, any] = useState(false);
    const [isLoading, setIsLoading]: [boolean, any] = useState(false);
    const [collections, setCollections]: [CollectionBasicVM[], any] = useState([]);
    const [layouts, setLayouts]: [Layout[], any] = useState([])
    const [showCollectionsModal, setShowCollectionsModal]: [boolean, any] = useState(false);
    const [showCollectionModal, setShowCollectionModal]: [boolean, any] = useState(false);
    const [showLayoutModal, setShowLayoutModal]: [boolean, any] = useState(false);
    const [collectionValidationMessage, setCollectionValidationMessage]: [string, any] = useState('');
    const [cannotBeModified, setCannotBeModified]: [boolean, any] = useState(false);

    //endregion

    //region useEffect

    useEffect(() => {
        if(sessionId.current === 0){
            setIsLoading(() => false)
            return
        }

        SessionService.getSessionToEdit(sessionId.current).then(res => {
            oldSession.current = {...res.data}
            selectedCollectionName.current = res.data.collectionName
            selectedLayoutName.current = res.data.layoutName

            let sessionTmp: Session = {
                id: res.data.id,
                name: res.data.name,
                collectionId: res.data.collectionId,
                geoJson: res.data.geoJson,
                layoutId: res.data.layoutId,
                laps: res.data.laps,
                scheduledFrom: res.data.scheduledFrom,
                scheduledTo: res.data.scheduledTo
            }

            let scheduledFromDate = new Date(sessionTmp.scheduledFrom)
            if(scheduledFromDate < getMinDate()){
                setCannotBeModified(() => true)
                setIsLoading(() => false)
                return
            }

            setSession(() => sessionTmp)
            setIsLoading(() => false)
        }).catch(() => {
            setIsLoading(() => false)
        })
    }, []);

    useEffect(() => {
        CollectionService.getCollections().then(res => {
            collectionsLoaded.current = true
            setCollections(() => res.data)
        })

        LayoutService.getLayouts().then(res => {
            layoutsLoaded.current = true
            setLayouts(() => res.data)
        })
    }, []);

    useEffect(() => {
        if(collectionsLoaded.current && layoutsLoaded.current)
            setIsLoading(() => false)
    }, [collections, layouts]);

    useEffect(() => {
        if(session.geoJson)
            validateGeoJson(session.geoJson)
    }, [session.geoJson]);

    //endregion

    function getMinDate(): Date{
        const minDate = new Date();
        minDate.setMinutes(minDate.getMinutes() - minDate.getTimezoneOffset() + 60);
        return minDate;
    }

    function handleCloseCollectionsModal() {
        setShowCollectionsModal(() => false)
    }

    function handleCloseCollectionModal() {
        setShowCollectionModal(() => false)
    }

    function handleCloseLayoutModal() {
        setShowLayoutModal(() => false)
    }
    function handleRemoveLayout() {
        selectedLayoutName.current = ''
        let sessionTmp = {...session}
        sessionTmp.geoJson = ''
        sessionTmp.layoutId = undefined
        setSession(() => sessionTmp)
    }

    function handleRemoveCollection(){
        selectedCollectionName.current = ''
        let sessionTmp = {...session}
        sessionTmp.collectionId = 0
        setSession(() => sessionTmp)
    }

    function handleSubmit(e: any){
        e.preventDefault();

        if(!validateSession()){
            return
        }

        if(session.id === 0){
            createSession()
            return;
        }

        if(isSessionChanged()){
            if(new Date(oldSession.current.scheduledFrom) < getMinDate()){
                setCannotBeModified(() => true)
                return
            }
            updateSession()
        }
        else{
            alert("No changes found!");
            return;
        }
    }

    function createSession(){
        setIsLoading(() => true)
        SessionService.createSession(session).then(() => {
            setIsLoading(() => false)
            setIsCreated(() => true)
        })
    }

    function updateSession(){
        setIsLoading(() => true)
        SessionService.updateSession(session).then(() => {
            setIsLoading(() => false)
            setIsUpdated(() => true)
        })
    }

    function validateSession(): boolean {
        return (!session.layoutId ? validateGeoJson(session.geoJson) : true)
            && validateSelectedCollection()
    }

    function validateGeoJson(geoJsonString: string): boolean {

        let [result, geoJson, message] = Utils.validateGeoJson(geoJsonString)

        if(result){
            isCircuit.current = isGeoJsonCircuit(geoJson);
            if(isCircuit.current){
                let sessionTmp = {...session}
                sessionTmp.laps = 1
                setSession(() => sessionTmp)
            }
            else if (!isCircuit.current && session.laps > 0){
                let sessionTmp = {...session}
                sessionTmp.laps = 0
                setSession(() => sessionTmp)
            }
            geoJsonValidationMessage.current = '';
            return true
        }

        isCircuit.current = false;
        if (session.laps > 0){
            let sessionTmp = {...session}
            sessionTmp.laps = 0
            setSession(() => sessionTmp)
        }
        geoJsonValidationMessage.current = message;
        return false;
    }

    function isGeoJsonCircuit(geoJson: any): boolean{
        let geometry = geoJson.features[0].geometry;
        return geometry.type === 'LineString' &&
            geometry.coordinates[0][0] === geometry.coordinates[geometry.coordinates.length - 1][0] &&
            geometry.coordinates[0][1] === geometry.coordinates[geometry.coordinates.length - 1][1];
    }

    function validateSelectedCollection(): boolean {
        if (session.collectionId === 0) {
            setCollectionValidationMessage(() => 'Please select collection');
            return false
        }
        setCollectionValidationMessage(() => '');
        return true
    }

    function isSessionChanged(): boolean {
        let layoutChanged;
        if(session.layoutId && oldSession.current.layoutId)
            layoutChanged = session.layoutId !== oldSession.current.layoutId
        else
            layoutChanged = session.geoJson !== oldSession.current.geoJson

        return layoutChanged ||
            session.name !== oldSession.current.name ||
            session.collectionId !== oldSession.current.collectionId ||
            session.laps !== oldSession.current.laps ||
            session.scheduledFrom !== oldSession.current.scheduledFrom ||
            session.scheduledTo !== oldSession.current.scheduledTo
    }

    if(cannotBeModified){
        return (
            <div className={"container-fluid d-flex justify-content-center pt-5"} style={{maxWidth: "500px"}}>
                <div className={"p-4"}>
                    <p>Session can no longer be edited.</p>
                </div>
            </div>
        )
    }
    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    if(isCreated) {
        return (
            <TextMessage message={"Your session was successfully created."} />
        )
    }
    if(isUpdate) {
        return (
            <TextMessage message={"Your session was successfully updated."} />
        )
    }
    return (
        <div>
            <Row>
                <Col className={"d-flex justify-content-center"}>
                    <Form className={"form"} onSubmit={handleSubmit}>
                        <Form.Group controlId={"create-session"} className={"m-3"}>
                            <Form.Label>Session name</Form.Label>
                            <Form.Control
                                type={"text"}
                                required
                                value={session.name}
                                onChange={e => {
                                    let sessionTmp = {...session}
                                    sessionTmp.name = e.target.value
                                    setSession(() => sessionTmp)
                                }}
                            />
                        </Form.Group>
                        <Form.Group controlId={"collection"} className={"m-3"}>
                            {
                                selectedCollectionName.current !== '' &&
                                <>
                                    <div className={'m-2'}>
                                        Selected Collection
                                    </div>
                                    <div className={'d-flex'}>
                                        <div className={'form-div p-2 w-100'}>
                                            {selectedCollectionName.current}
                                        </div>
                                        <Button
                                            className={"btn-danger"}
                                            onClick={() => {
                                                handleRemoveCollection()
                                            }}
                                        >
                                            <Trash></Trash>
                                        </Button>
                                    </div>
                                </>
                            }
                            {
                                session.collectionId === 0 &&
                                <div className={'m-2'}>
                                    <Button
                                        style={{width: 200}}
                                        className={'btn-primary'}
                                        onClick={() => setShowCollectionsModal(() => true)}
                                    >
                                        Select Collection
                                    </Button>
                                </div>
                            }
                            {
                                collectionValidationMessage &&
                                <div>
                                    <p className={"text-warning"}>{collectionValidationMessage}</p>
                                </div>
                            }
                        </Form.Group>
                        <Form.Group controlId={"layout"} className={"m-3"}>
                            {
                                selectedLayoutName.current !== '' &&
                                <>
                                    <div className={'m-2'}>
                                        Selected Layout
                                    </div>
                                    <div className={'d-flex'}>
                                        <div className={'form-div p-2 w-100'}>
                                            {selectedLayoutName.current}
                                        </div>
                                        <Button
                                            className={"btn-danger"}
                                            onClick={() => {
                                                handleRemoveLayout()
                                            }}
                                        >
                                            <Trash></Trash>
                                        </Button>
                                    </div>
                                </>
                            }
                            {
                                !session.layoutId &&
                                <div className={'m-2'}>
                                    <Button
                                        style={{width: 200}}
                                        className={'btn-primary'}
                                        onClick={() => setShowLayoutModal(() => true)}
                                    >
                                        Select Layout
                                    </Button>
                                </div>
                            }
                            {
                                !session.layoutId &&
                                <>
                                    <Form.Label>GeoJson</Form.Label>
                                    <Form.Control
                                        as={"textarea"}
                                        style={{minHeight: '200px'}}
                                        required={session.layoutId === undefined || session.layoutId === 0}
                                        disabled={session.layoutId !== undefined && session.layoutId > 0}
                                        value={session.geoJson}
                                        onChange={e => {
                                            const geoJsonString = e.target.value
                                            let sessionTmp = {...session}
                                            sessionTmp.geoJson = geoJsonString
                                            setSession(() => sessionTmp)
                                        }}
                                    />
                                    {
                                        geoJsonValidationMessage.current &&
                                        <div className={"mb-3"}>
                                            <p className={"text-warning"}>{geoJsonValidationMessage.current}</p>
                                        </div>
                                    }
                                </>
                            }
                            {
                                geoJsonValidationMessage.current === '' &&
                                <Row
                                    className={'mt-2 mb-2'}
                                >
                                    {
                                        session.geoJson &&
                                        <GeoJsonImage geoJsonData={JSON.parse(session.geoJson)} />
                                    }
                                </Row>
                            }
                            {
                                isCircuit.current &&
                                <>
                                    <Form.Label>Laps</Form.Label>
                                    <Form.Control
                                        type={"number"}
                                        required={isCircuit.current}
                                        value={session.laps}
                                        min={1}
                                        onChange={e => {
                                            let laps: number = +e.target.value;
                                            let sessionTmp = {...session}
                                            sessionTmp.laps = laps
                                            setSession(() => sessionTmp)
                                        }}
                                    />
                                </>
                            }
                        </Form.Group>
                        <Form.Group controlId={"schedule"} className={"m-3"}>
                            <Form.Label>Scheduled From</Form.Label>
                            <Form.Control
                                type={"datetime-local"}
                                required
                                value={session.scheduledFrom.slice(0, 16)}
                                min={getMinDate().toISOString().slice(0, 16)}
                                onChange={e => {
                                    let scheduledFrom = e.target.value;
                                    let sessionTmp = {...session}
                                    sessionTmp.scheduledFrom = scheduledFrom
                                    setSession(() => sessionTmp)
                                }}
                            />

                            <Form.Label>Scheduled To</Form.Label>
                            <Form.Control
                                type={"datetime-local"}
                                required
                                value={session.scheduledTo.slice(0, 16)}
                                min={getMinDate().toISOString().slice(0, 16)}
                                onChange={e => {
                                    let sessionTmp = {...session}
                                    sessionTmp.scheduledTo = e.target.value
                                    setSession(() => sessionTmp)
                                }}
                            />
                        </Form.Group>
                        <div className={'m-3'}>
                            <Button style={{width: 200}} type={"submit"} variant={"dark"}>Submit</Button>
                        </div>
                    </Form>
                </Col>
            </Row>

            <Modal
                show={showCollectionsModal}
                onHide={handleCloseCollectionsModal}
                animation={false}
                dialogClassName={'wider-modal'}
            >
                <Modal.Header closeButton></Modal.Header>
                <Modal.Body>
                    {
                        collections.length == 0 ?
                            <div>
                                <p>You have no collections</p>
                            </div>
                            :
                            <TableContainer component={Paper}>
                                <Table aria-label="collections table">
                                    <TableHead className={"table-head"}>
                                        <TableRow>
                                            <TableCell className={"white-font"}>Name</TableCell>
                                            <TableCell className={"white-font"}>View</TableCell>
                                            <TableCell className={"white-font"}>Select</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {collections.map((collection, index) => (
                                            <TableRow
                                                key={index}
                                                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                            >
                                                <TableCell>{collection.name}</TableCell>
                                                <TableCell>
                                                    <Button
                                                        className={"btn-secondary"}
                                                        onClick={() => {
                                                            collectionToShow.current = collection.id
                                                            setShowCollectionModal(() => true)
                                                        }}
                                                    >
                                                        <Eye></Eye>
                                                    </Button>
                                                </TableCell>
                                                <TableCell>
                                                    <Button
                                                        className={"btn-success"}
                                                        onClick={() => {
                                                            selectedCollectionName.current = collection.name
                                                            let sessionTmp = {...session}
                                                            sessionTmp.collectionId = collection.id
                                                            setSession(() => sessionTmp)
                                                            if(collection.id !== 0) {
                                                                setCollectionValidationMessage(() => '')
                                                            }
                                                            handleCloseCollectionsModal()
                                                        }}
                                                    >
                                                        Select
                                                    </Button>
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                    }
                </Modal.Body>
            </Modal>

            <Modal
                show={showCollectionModal}
                onHide={handleCloseCollectionModal}
                animation={false}
                dialogClassName={'wider-modal'}
            >
                <Modal.Header closeButton></Modal.Header>
                <Modal.Body style={{overflowY: "auto"}}>
                    {collectionToShow.current !== 0 && <CollectionView collectionId={collectionToShow.current} />}
                </Modal.Body>
            </Modal>

            <Modal
                show={showLayoutModal}
                onHide={handleCloseLayoutModal}
                animation={false}
                dialogClassName={'wider-modal'}
            >
                <Modal.Header closeButton></Modal.Header>
                <Modal.Body style={{overflowY: "auto"}}>
                    {
                        <Row className={"m-2"}>
                            {
                                layouts.map((layout, index) => (
                                    <Col key={index} lg={4} md={6} sm={8} className={"mb-4"}>
                                        <div className={"p-4 session-card"}>
                                            <div>
                                                <div className={"pb-2"}>
                                                    <h3>{layout.name}</h3>
                                                </div>
                                                <div className={"mt-2 mb-2"}>
                                                    {
                                                        <GeoJsonImage geoJsonData={JSON.parse(layout.geoJson)} />
                                                    }
                                                </div>
                                                <div className={"d-flex justify-content-center"}>
                                                    <Button
                                                        className={"btn-success"}
                                                        onClick={() => {
                                                            selectedLayoutName.current = layout.name
                                                            let sessionTmp = {...session}
                                                            sessionTmp.layoutId = layout.id
                                                            sessionTmp.geoJson = layout.geoJson
                                                            setSession(() => sessionTmp)
                                                            //validateGeoJson(layout.geoJson)
                                                            handleCloseLayoutModal()
                                                        }}
                                                    >
                                                        Select
                                                    </Button>
                                                </div>
                                            </div>
                                        </div>
                                    </Col>
                                ))
                            }
                        </Row>
                    }
                </Modal.Body>
            </Modal>
        </div>
    )
}

export default SessionEditorComponent;