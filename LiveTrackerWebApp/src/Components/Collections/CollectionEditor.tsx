import {Button, Col, Collapse, Form, Modal, Row} from "react-bootstrap";
import React, {useEffect, useRef, useState} from "react";
import {Driver} from "../../Models/Driver";
import "./Collections.css";
import {
    Checkbox,
    FormControlLabel, Paper, Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow
} from "@mui/material";
import {Team} from "../../Models/Team";
import {Constants} from "../../Constants";
import {Utils} from "../../Utils/Utils";
import {Pencil, Trash} from "react-bootstrap-icons";
import { validate as uuidValidate } from 'uuid';
import CollectionService from "../../Services/CollectionService";
import {useNavigate, useParams} from "react-router-dom";
import LoadingComponent from "../Loading";
import {Collection} from "../../Models/Collection";

const CollectionEditorComponent = () => {

    //region useParams

    const {id} = useParams();

    //endregion

    //region useRef
    const collectionId: React.MutableRefObject<number> = useRef(id == undefined ? 0 : +id);
    const oldCollection: React.MutableRefObject<Collection> = useRef(Constants.defaultCollection);

    const defaultDriverNumber: React.MutableRefObject<number> = useRef(1);
    const teamColorDict: React.MutableRefObject<{[key: string]: string}> = useRef({});

    const isValidNewDriverNumber: React.MutableRefObject<boolean> = useRef(true);
    const isValidNewDriverGpsDevice: React.MutableRefObject<boolean> = useRef(true);

    const driverToEditIndex: React.MutableRefObject<number> = useRef(0);
    const isValidDriverToEditGpsDevice: React.MutableRefObject<boolean> = useRef(true);

    const validateCollectionErrorText: React.MutableRefObject<string> = useRef("");
    const addDriverErrorText: React.MutableRefObject<string> = useRef("");
    const editDriverErrorText: React.MutableRefObject<string> = useRef("");
    const addTeamErrorText: React.MutableRefObject<string> = useRef("");
    const editTeamErrorText: React.MutableRefObject<string> = useRef("");

    const isValidNewTeamName: React.MutableRefObject<boolean> = useRef(true);

    const teamToEditIndex: React.MutableRefObject<number> = useRef(0);
    const isValidTeamToEditName: React.MutableRefObject<boolean> = useRef(true);
    //endregion

    //region constants
    const navigate = useNavigate();

    const defaultDriver = (): Driver => {
        return {
            id: 0,
            color: Constants.defaultColor,
            name: "",
            number: defaultDriverNumber.current,
            surname: "",
            gpsDevice: ""
        }
    }
    const defaultTeam = {
        id: 0,
        name: "",
        color: Constants.defaultColor
    }

    const isValidNewDriver = (): boolean => {
        return isValidNewDriverNumber.current && isValidNewDriverGpsDevice.current
    }

    const isValidDriverToEdit = (): boolean => {
        return isValidDriverToEditGpsDevice.current
    }

    const isValidNewTeam = (): boolean => {
        return isValidNewTeamName.current
    }

    const isValidTeamToEdit = (): boolean => {
        return isValidTeamToEditName.current
    }
    //endregion

    //region useState
    const [isLoading, setIsLoading]: [boolean, any] = useState(true);

    const [collectionName, setCollectionName]: [string, any] = useState("");
    const [useTeams, setUseTeams]: [boolean, any] = useState(false);
    const [driversList, setDriversList]: [Driver[], any] = useState([]);
    const [teamsList, setTeamsList]: [Team[], any] = useState([]);

    const [newDriver, setNewDriver]: [Driver, any] = useState(defaultDriver());
    const [visibleAddNewDriverForm, setVisibleAddNewDriverForm]: [boolean, any] = useState(false);

    const [newTeam, setNewTeam]: [Team, any] = useState(defaultTeam);
    const [visibleAddNewTeamForm, setVisibleAddNewTeamForm]: [boolean, any] = useState(false);

    const [showEditDriver, setShowEditDriver]: [boolean, any] = useState(false);
    const [driverToEdit, setDriverToEdit]: [Driver, any] = useState(defaultDriver());

    const [showEditTeam, setShowEditTeam]: [boolean, any] = useState(false);
    const [teamToEdit, setTeamToEdit]: [Team, any] = useState(defaultTeam);

    const [showRemoveTeams, setShowRemoveTeams]: [boolean, any] = useState(false);
    const [showErrorModal, setShowErrorModal]: [boolean, any] = useState(false);
    //endregion

    //region useEffect
    useEffect(() => {
        if(collectionId.current === 0){
            setIsLoading(() => false)
            return
        }

        CollectionService.getCollection(collectionId.current).then(res => {
            loadCollectionToEdit(res.data)
            setIsLoading(() => false)
        }).catch(() => {
            setIsLoading(() => false)
        })
    }, []);


    useEffect(() => {
        if(useTeams){
            validateDriversTeams()
        }
    }, [useTeams]);
    //endregion

    //region functions

    //region handlers
    function handleSaveCollection(){
        if(!validateCollection()){
            return
        }
        let collection = getCollectionObject()
        if(collection.id === 0){
            setIsLoading(() => true)
            CollectionService.createCollection(collection).then(() => {
                navigate("/collections")
            }).catch(e => {
                console.log(e)
                setIsLoading(() => false)
            })
        }
        else{
            if(isCollectionChanged()){
                setIsLoading(() => true)
                CollectionService.updateCollection(collection).then(() => {
                    navigate("/collections")
                }).catch(e => {
                    console.log(e)
                    setIsLoading(() => false)
                })
            }
            else if(collectionName !== oldCollection.current.name){
                setIsLoading(() => true)
                CollectionService.renameCollection(collectionId.current, collectionName).then(() => {
                    navigate("/collections")
                }).catch(e => {
                    console.log(e)
                    setIsLoading(() => false)
                })
            }
            else{
                alert("No changes found!");
            }
        }
    }

    function handleChangeUseTeams (event: React.ChangeEvent<HTMLInputElement>) {
        if(useTeams && driversList.filter(x => !Utils.isNullOrEmpty(x.teamName)).length > 0){
            setShowRemoveTeams(() => true);
            return;
        }
        setUseTeams(() => event.target.checked);
    }

    function handleAddDriver(e: any) {
        e.preventDefault();
        if(!isValidNewDriver())
            return;

        let newDriverTmp: Driver = {...newDriver}

        if(useTeams && Utils.isNullOrEmpty(newDriverTmp.teamName)){
            if(teamsList.length > 0){
                newDriverTmp.teamName = teamsList[0].name
                newDriverTmp.color = teamsList[0].color
                if(newDriverTmp.number == defaultDriverNumber.current){
                    recalculateDefaultDriverNumber([...driversList, newDriverTmp]);
                }
                setDriversList(() => [...driversList, newDriverTmp].sort((a, b) => {
                    return a.id - b.id;
                }));
                setNewDriver(() => defaultDriver());
                return;
            }
            return;
        }
        if(newDriverTmp.number == defaultDriverNumber.current){
            recalculateDefaultDriverNumber([...driversList, newDriverTmp]);
        }
        setDriversList(() => [...driversList, newDriverTmp].sort((a, b) => {
            return a.id - b.id;
        }));
        setNewDriver(() => defaultDriver());
    }

    function handleAddTeam(e: any){
        e.preventDefault();
        if(!isValidNewTeam())
            return

        teamColorDict.current[newTeam.name] = newTeam.color
        setTeamsList(() => [...teamsList, newTeam].sort((a, b) => {
            return a.name.localeCompare(b.name);
        }));
        setNewTeam(() => defaultTeam);
    }

    function handleCloseDriverEdit() {
        setDriverToEdit(() => defaultDriver())
        setShowEditDriver(() => false)
    }

    function handleSaveDriverEdit(e: any){
        e.preventDefault();
        if(!isValidDriverToEdit())
            return

        if(useTeams && Utils.isNullOrEmpty(driverToEdit.teamName)){
            if(teamsList.length > 0){
                let driverToEditTmp: Driver = {...driverToEdit}
                driverToEditTmp.teamName = teamsList[0].name
                driverToEditTmp.color = teamsList[0].color

                saveDriverToEditAndCheckList(driverToEditTmp);

                let driverTeamNameCell = document.getElementById(`driver${driverToEditIndex.current}team`);
                if(driverTeamNameCell != null){
                    driverTeamNameCell.classList.remove("error-cell");
                }
            }
        }
        else{
            saveDriverToEditAndCheckList({...driverToEdit});
        }
        setDriverToEdit(() => defaultDriver())
        setShowEditDriver(() => false)
    }

    function handleCloseRemoveTeams() {
        setShowRemoveTeams(() => false)
    }

    function handleRemoveTeams(){
        let tmpDriversList = [...driversList];
        tmpDriversList.forEach(x => {
            x.teamName = undefined
            x.color = getDriverColor(x)
        })
        setDriversList(() => tmpDriversList)
        teamColorDict.current = {}
        setTeamsList(() => [])
        setUseTeams(() => false)
        setShowRemoveTeams(() => false)
    }

    function handleRemoveDriver(index: number) {
        let driversTmp = [...driversList]
        driversTmp.splice(index, 1)
        recalculateDefaultDriverNumber(driversTmp)
        setDriversList(() => driversTmp)
    }

    function handleRemoveTeam(team: Team, index: number) {
        let driversTmp = [...driversList]
        driversTmp.forEach(x => {
            if(x.teamName == team.name)
                x.teamName = undefined
        })
        setDriversList(() => driversTmp)

        delete teamColorDict.current[team.name]

        let teamsTmp = [...teamsList]
        teamsTmp.splice(index, 1)
        setTeamsList(() => teamsTmp)

        validateDriversTeams()
    }

    function handleCloseErrorModal(){
        setShowErrorModal(() => false)
    }

    function handleCloseTeamEdit(){
        setTeamToEdit(() => defaultTeam)
        setShowEditTeam(() => false)
    }

    function handleSaveTeamEdit(e: any){
        e.preventDefault()
        if(!isValidTeamToEdit())
            return

        let oldTeam = teamsList[teamToEditIndex.current]
        if(oldTeam.name !== teamToEdit.name){
            let driversListTmp = [...driversList]
            driversListTmp.forEach(x => {
                if(x.teamName === oldTeam.name){
                    x.teamName = teamToEdit.name
                }
            })
            setDriversList([...driversListTmp])
        }

        delete teamColorDict.current[oldTeam.name]
        teamColorDict.current[teamToEdit.name] = teamToEdit.color

        console.log()

        let tmpTeamsList = [...teamsList];
        tmpTeamsList[teamToEditIndex.current] = {...teamToEdit}
        setTeamsList(() => tmpTeamsList)
        setShowEditTeam(() => false)
    }
    //endregion

    //region validators
    function validateCollection(): boolean {
        if(driversList.length <= 0){
            displayErrorModal("You have no drivers!");
            return false;
        }
        if(!validateDriversTeams())
            return false;
        if(!validateDriverNumbers(driversList))
            return false;
        return validateDriversGpsDevices(driversList);
    }

    function validateDriversTeams(): boolean {
        let isValid: boolean = true;

        driversList.forEach((x, index) => {
            if(useTeams && Utils.isNullOrEmpty(x.teamName)){
                let driverTeamNameCell = document.getElementById(`driver${index}team`);
                if(driverTeamNameCell != null){
                    isValid = false
                    driverTeamNameCell.classList.add("error-cell");
                }
            }
        });
        if(!isValid){
            displayErrorModal("Some drivers has no team!")
        }
        return isValid
    }

    function validateDriverNumbers(drivers: Driver[]): boolean{
        let duplicateNumbers: number[] = Utils.findDuplicateNumbers(drivers.map(x => x.number));
        let isValid = duplicateNumbers.length === 0;

        drivers.forEach((x, index) => {
            let driverRow = document.getElementById(`driver${index}number`)
            if (driverRow != null) {
                if (duplicateNumbers.includes(x.number)) {
                    driverRow.classList.add("error-cell");
                } else {
                    driverRow.classList.remove("error-cell");
                }
            }
        })
        if (!isValid) {
            displayErrorModal("Some drivers has the same number!")
        }
        return isValid
    }

    function validateDriversGpsDevices(drivers: Driver[]): boolean{
        let duplicateGpsDevices: string[] = Utils.findDuplicateStrings(drivers.map(x => x.gpsDevice));
        let isValid = duplicateGpsDevices.length === 0;

        drivers.forEach((x, index) => {
            let driverRow = document.getElementById(`driver${index}gpsDevice`)
            if (driverRow != null) {
                if (duplicateGpsDevices.includes(x.gpsDevice)) {
                    driverRow.classList.add("error-cell");
                } else {
                    driverRow.classList.remove("error-cell");
                }
            }
        })
        if (!isValid) {
            displayErrorModal("Some drivers has the same GPS device!")
        }
        return isValid
    }
    //endregion

    //region helpers
    function saveDriverToEditAndCheckList(driver: Driver) {
        let tmpDriversList = [...driversList];
        tmpDriversList[driverToEditIndex.current] = {...driver}
        setDriversList(() => tmpDriversList)

        if(!validateDriverNumbers(tmpDriversList))
            return;
        validateDriversGpsDevices(tmpDriversList)
    }

    function recalculateDefaultDriverNumber(drivers: Driver[]) {
        let number = 1;
        let numbers = drivers.map(x => x.number);
        while (true) {
            if (!numbers.includes(number))
                break;
            number++
        }
        defaultDriverNumber.current = number
    }

    function displayErrorModal(text: string){
        validateCollectionErrorText.current = text
        setShowErrorModal(() => true);
    }

    function getCollectionObject(): Collection{
        return {
            id: collectionId.current,
            name: collectionName,
            drivers: driversList,
            teams: teamsList,
            useTeams: useTeams
        }
    }

    function loadCollectionToEdit(collection: Collection){
        oldCollection.current = {...collection}

        recalculateDefaultDriverNumber(collection.drivers)
        if(collection.useTeams)
            createTeamColorDict(collection.teams)

        setCollectionName(() => collection.name)
        setDriversList(() => collection.drivers)
        setTeamsList(() => collection.teams)
        setUseTeams(() => collection.useTeams)
    }

    function createTeamColorDict(teams: Team[]){
        teams.forEach(x => {
            teamColorDict.current[x.name] = x.color
        })
    }

    function isCollectionChanged(): boolean {
        if(useTeams && !oldCollection.current.useTeams || !useTeams && oldCollection.current.useTeams){
            return true;
        }
        if(driversList.length !== oldCollection.current.drivers.length || teamsList.length !== oldCollection.current.teams.length){
            return true;
        }
        if(useTeams){
            const oldTeams = oldCollection.current.teams.sort((a, b) => {
                return a.name.localeCompare(b.name);
            });
            const currentTeams = teamsList.sort((a, b) => {
                return a.name.localeCompare(b.name);
            });

            for(let i = 0; i < oldTeams.length; i++){
                let team1 = oldTeams[i]
                let team2 = currentTeams[i]
                if(!areTeamsEqual(team1, team2)){
                    return true
                }
            }
        }
        const oldDrivers = oldCollection.current.drivers.slice().sort((a, b) => {
            return a.number - b.number
        })
        const currentDrivers = driversList.slice().sort((a, b) => {
            return a.number - b.number
        })

        for(let i = 0; i < oldDrivers.length; i++){
            let driver1 = oldDrivers[i]
            let driver2 = currentDrivers[i]
            if(!areDriversEqual(driver1, driver2)){
                return true
            }
        }

        return false
    }

    function areTeamsEqual(team1: Team, team2: Team): boolean {
        return team1.name === team2.name &&
            team1.color === team2.color
    }

    function areDriversEqual(driver1: Driver, driver2: Driver): boolean {
        return driver1.number === driver2.number &&
            driver1.name === driver2.name &&
            driver1.surname === driver2.surname &&
            driver1.teamName === driver2.teamName &&
            driver1.color === driver2.color &&
            driver1.gpsDevice === driver2.gpsDevice
    }

    function getDriverColor(driver: Driver): string{
        if(!useTeams)
            return driver.color
        return teamColorDict.current[driver.teamName!] || Constants.defaultColor
    }

    //endregion

    //endregion

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return (
        <div className={"container-fluid pt-5 pb-5"} style={{maxWidth: "1000px"}}>
            <Row>
                <Col>
                    <FormControlLabel
                        value="end"
                        control={
                            <Checkbox
                                checked={useTeams}
                                onChange={handleChangeUseTeams}
                            />}
                        label="Use Teams"
                        labelPlacement="end"
                    />
                </Col>
                <Col>
                    <Form.Label>Collection Name</Form.Label>
                    <Form.Control
                        type="text"
                        autoFocus
                        required
                        value={collectionName}
                        onChange={e => {
                            setCollectionName(() => e.target.value)
                        }}
                    />
                </Col>
                <Col>
                    <Button
                        className={"btn-success"}
                        onClick={handleSaveCollection}
                    >
                        Save Collection
                    </Button>
                </Col>
            </Row>
            <Row className={"m-3"}>
                {
                    driversList.length > 0 ?
                        <TableContainer component={Paper} style={{maxHeight: '400px'}}>
                            <Table sx={{ minWidth: 650 }} aria-label="drivers table">
                                <TableHead className={"table-head"}>
                                    <TableRow>
                                        <TableCell className={"white-font"}>Number</TableCell>
                                        <TableCell className={"white-font"}>Name</TableCell>
                                        <TableCell className={"white-font"}>Surname</TableCell>
                                        <TableCell className={"white-font"}>GPS Device</TableCell>
                                        {useTeams && <TableCell className={"white-font"}>Team</TableCell>}
                                        <TableCell className={"white-font"}>Color</TableCell>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {driversList.map((driver, index) => (
                                        <TableRow
                                            key={index}
                                            sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                        >
                                            <TableCell id={`driver${index}number`}>{driver.number}</TableCell>
                                            <TableCell>{driver.name}</TableCell>
                                            <TableCell>{driver.surname}</TableCell>
                                            <TableCell id={`driver${index}gpsDevice`}>{driver.gpsDevice}</TableCell>
                                            {useTeams && <TableCell id={`driver${index}team`}>{driver.teamName}</TableCell>}
                                            <TableCell>
                                                <div className={"color-div"} style={{backgroundColor: getDriverColor(driver)}}></div>
                                            </TableCell>
                                            <TableCell>
                                                <Button
                                                    className={"btn-secondary"}
                                                    onClick={() => {
                                                        setShowEditDriver(() => true)
                                                        driverToEditIndex.current = index
                                                        setDriverToEdit({...driver})
                                                    }}
                                                >
                                                    <Pencil></Pencil>
                                                </Button>
                                            </TableCell>
                                            <TableCell>
                                                <Button
                                                    className={"btn-danger"}
                                                    onClick={() => {
                                                        handleRemoveDriver(index)
                                                    }}
                                                >
                                                    <Trash></Trash>
                                                </Button>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                        :
                        <div>
                            No Drivers
                        </div>
                }
            </Row>
            <Row className={"m-2"}>
                <Col>
                    <Button
                        onClick={() => setVisibleAddNewDriverForm(!visibleAddNewDriverForm)}
                        aria-controls="addNewDriverForm"
                        aria-expanded={visibleAddNewDriverForm}
                    >
                        Add New Driver
                    </Button>
                </Col>
            </Row>
            <Row id="addNewDriverForm">
                <Col className={"d-flex justify-content-center"}>
                    <Collapse in={visibleAddNewDriverForm}>
                        <Form className={"add-form"} onSubmit={handleAddDriver}>
                            <Form.Group className="mb-3" controlId="newDriver">
                                <Form.Label>Name</Form.Label>
                                <Form.Control
                                    type="text"
                                    autoFocus
                                    required
                                    value={newDriver.name}
                                    onChange={e => {
                                        let newDriverTmp: Driver = {...newDriver}
                                        newDriverTmp.name = e.target.value
                                        setNewDriver({...newDriverTmp})
                                    }}
                                />

                                <Form.Label>Surname</Form.Label>
                                <Form.Control
                                    type="text"
                                    required
                                    value={newDriver.surname}
                                    onChange={e => {
                                        let newDriverTmp: Driver = {...newDriver}
                                        newDriverTmp.surname = e.target.value
                                        setNewDriver({...newDriverTmp})
                                    }}
                                />

                                <Form.Label>Number</Form.Label>
                                <Form.Control
                                    type="number"
                                    required
                                    min={1}
                                    value={newDriver.number}
                                    onChange={e => {
                                        let newDriverTmp: Driver = {...newDriver}
                                        newDriverTmp.number = +e.target.value
                                        for(let x of driversList) {
                                            if(x.number === newDriverTmp.number){
                                                e.target.classList.add("error-box")
                                                isValidNewDriverNumber.current = false
                                                addDriverErrorText.current = "Driver with this number already exists"
                                                setNewDriver({...newDriverTmp})
                                                return;
                                            }
                                        }
                                        e.target.classList.remove("error-box")
                                        if(isValidNewDriver())
                                            addDriverErrorText.current = ""
                                        isValidNewDriverNumber.current = true
                                        setNewDriver({...newDriverTmp})
                                    }}
                                />

                                <Form.Label>GPS Device</Form.Label>
                                <Form.Control
                                    type="text"
                                    required
                                    placeholder={"GUID"}
                                    value={newDriver.gpsDevice}
                                    onChange={e => {
                                        let newDriverTmp: Driver = {...newDriver}
                                        newDriverTmp.gpsDevice = e.target.value
                                        if(!uuidValidate(newDriverTmp.gpsDevice)) {
                                            e.target.classList.add("error-box")
                                            isValidNewDriverGpsDevice.current = false
                                            addDriverErrorText.current = "Value is not valid guid"
                                            setNewDriver({...newDriverTmp})
                                            return;
                                        }
                                        for(let x of driversList) {
                                            if(x.gpsDevice === newDriverTmp.gpsDevice){
                                                e.target.classList.add("error-box")
                                                isValidNewDriverGpsDevice.current = false
                                                addDriverErrorText.current = "Driver with this GPS device already exists"
                                                setNewDriver({...newDriverTmp})
                                                return;
                                            }
                                        }
                                        e.target.classList.remove("error-box")
                                        isValidNewDriverGpsDevice.current = true
                                        if(isValidNewDriver())
                                            addDriverErrorText.current = ""
                                        setNewDriver({...newDriverTmp})
                                    }}
                                />
                                {
                                    useTeams ?
                                        <>
                                            <Form.Label>Team</Form.Label>
                                            <Form.Select
                                                id={"team-select"}
                                                aria-label={"teams"}
                                                required={true}
                                                onChange={e => {
                                                    let newDriverTmp: Driver = {...newDriver};
                                                    newDriverTmp.teamName = e.target.value;
                                                    //newDriverTmp.color = teamColorDict.current[e.target.value]
                                                    setNewDriver({...newDriverTmp});
                                                }}
                                            >
                                                {
                                                    teamsList.map((team, index) => (
                                                        <option key={index} value={team.name}>{team.name}</option>
                                                    ))
                                                }
                                            </Form.Select>
                                        </>
                                        :
                                        <>
                                            <Form.Label>Color</Form.Label>
                                            <Form.Control
                                                type="color"
                                                required
                                                value={newDriver.color}
                                                onChange={e => {
                                                    let newDriverTmp: Driver = {...newDriver};
                                                    newDriverTmp.color = e.target.value;
                                                    setNewDriver({...newDriverTmp});
                                                }}
                                            />
                                        </>
                                }

                            </Form.Group>
                            <div>
                                <Button style={{width: 200}} type={"submit"} variant={"dark"}>Add Driver</Button>
                            </div>
                            {
                                addDriverErrorText.current &&
                                <div className={"mt-3"}>
                                    <p className={"text-danger"}>{addDriverErrorText.current}</p>
                                </div>
                            }
                        </Form>
                    </Collapse>
                </Col>
            </Row>
            {
                useTeams &&
                <>
                    <Row className={"m-3"}>
                        {
                            teamsList.length > 0 ?
                                <TableContainer component={Paper} style={{maxHeight: '400px'}}>
                                    <Table sx={{ minWidth: 650 }} aria-label="simple table">
                                        <TableHead className={"table-head"}>
                                            <TableRow>
                                                <TableCell className={"white-font"}>Name</TableCell>
                                                <TableCell className={"white-font"}>Color</TableCell>
                                                <TableCell></TableCell>
                                                <TableCell></TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {teamsList.map((team, index) => (
                                                <TableRow
                                                    key={index}
                                                    sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                                >
                                                    <TableCell>{team.name}</TableCell>
                                                    <TableCell>
                                                        <div className={"color-div"} style={{backgroundColor: team.color}}></div>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Button
                                                            className={"btn-secondary"}
                                                            onClick={() => {
                                                                setShowEditTeam(() => true)
                                                                teamToEditIndex.current = index
                                                                setTeamToEdit({...team})
                                                            }}
                                                        >
                                                            <Pencil></Pencil>
                                                        </Button>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Button
                                                            className={"btn-danger"}
                                                            onClick={() => {
                                                                handleRemoveTeam(team, index)
                                                            }}
                                                        >
                                                            <Trash></Trash>
                                                        </Button>
                                                    </TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                </TableContainer>
                                :
                                <div>
                                    No Teams
                                </div>
                        }
                    </Row>
                    <Row className={"m-2"}>
                        <Col>
                            <Button
                                onClick={() => setVisibleAddNewTeamForm(!visibleAddNewTeamForm)}
                                aria-controls="addNewTeamForm"
                                aria-expanded={visibleAddNewTeamForm}
                            >
                                Add New Team
                            </Button>
                        </Col>
                    </Row>
                    <Row id="addNewTeamForm">
                        <Col className={"d-flex justify-content-center"}>
                            <Collapse in={visibleAddNewTeamForm}>
                                <Form className={"add-form"} onSubmit={handleAddTeam}>
                                    <Form.Group className="mb-3" controlId="newTeam">
                                        <Form.Label>Name</Form.Label>
                                        <Form.Control
                                            type="text"
                                            autoFocus
                                            required
                                            value={newTeam.name}
                                            onChange={e => {
                                                let newTeamTmp: Team = {...newTeam};
                                                newTeamTmp.name = e.target.value;

                                                for(let x of teamsList) {
                                                    if(x.name === newTeamTmp.name){
                                                        e.target.classList.add("error-box")
                                                        isValidNewTeamName.current = false
                                                        addTeamErrorText.current = "Team with this name already exists"
                                                        setNewTeam({...newTeamTmp})
                                                        return;
                                                    }
                                                }
                                                e.target.classList.remove("error-box")
                                                isValidNewTeamName.current = true
                                                if(isValidNewTeam())
                                                    addTeamErrorText.current = ""
                                                setNewTeam({...newTeamTmp});
                                            }}/>

                                        <Form.Label>Color</Form.Label>
                                        <Form.Control
                                            type="color"
                                            required
                                            value={newTeam.color}
                                            onChange={e => {
                                                let newTeamTmp: Team = {...newTeam};
                                                newTeamTmp.color = e.target.value;
                                                teamColorDict.current[newTeamTmp.name] = newTeamTmp.color
                                                setNewTeam({...newTeamTmp});
                                            }}/>
                                    </Form.Group>
                                    <div>
                                        <Button style={{width: 200}} type={"submit"} variant={"dark"}>Add Team</Button>
                                    </div>
                                    {
                                        addTeamErrorText.current &&
                                        <div className={"mt-3"}>
                                            <p className={"text-danger"}>{addTeamErrorText.current}</p>
                                        </div>
                                    }
                                </Form>
                            </Collapse>
                        </Col>
                    </Row>
                </>
            }

            <Modal show={showEditDriver} onHide={handleCloseDriverEdit} animation={false}>
                <Modal.Header closeButton>
                    <Modal.Title>Driver Edit</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form id={"editDriverForm"} className={"add-form"} onSubmit={handleSaveDriverEdit}>
                        <Form.Group className="mb-3" controlId="editDriver">
                            <Form.Label>Name</Form.Label>
                            <Form.Control
                                type="text"
                                autoFocus
                                required
                                value={driverToEdit.name}
                                onChange={e => {
                                    let driverToEditTmp: Driver = {...driverToEdit}
                                    driverToEditTmp.name = e.target.value
                                    setDriverToEdit({...driverToEditTmp})
                                }}
                            />

                            <Form.Label>Surname</Form.Label>
                            <Form.Control
                                type="text"
                                required
                                value={driverToEdit.surname}
                                onChange={e => {
                                    let driverToEditTmp: Driver = {...driverToEdit}
                                    driverToEditTmp.surname = e.target.value
                                    setDriverToEdit({...driverToEditTmp})
                                }}
                            />

                            <Form.Label>Number</Form.Label>
                            <Form.Control
                                type="number"
                                required
                                min={1}
                                value={driverToEdit.number}
                                onChange={e => {
                                    let driverToEditTmp: Driver = {...driverToEdit}
                                    driverToEditTmp.number = +e.target.value
                                    setDriverToEdit({...driverToEditTmp})
                                }}
                            />

                            <Form.Label>GPS Device</Form.Label>
                            <Form.Control
                                type="text"
                                required
                                value={driverToEdit.gpsDevice}
                                onChange={e => {
                                    if(!uuidValidate(e.target.value)) {
                                        e.target.classList.add("error-box")
                                        editDriverErrorText.current = "Value is not valid guid"
                                        isValidDriverToEditGpsDevice.current = false
                                    }
                                    else {
                                        e.target.classList.remove("error-box")
                                        isValidDriverToEditGpsDevice.current = true
                                        if(isValidDriverToEdit())
                                            editDriverErrorText.current = ""
                                    }
                                    let driverToEditTmp: Driver = {...driverToEdit}
                                    driverToEditTmp.gpsDevice = e.target.value
                                    setDriverToEdit({...driverToEditTmp})
                                }}
                            />
                            {
                                editDriverErrorText.current &&
                                <div className={"mt-3"}>
                                    <p className={"text-danger"}>{editDriverErrorText.current}</p>
                                </div>
                            }

                            {
                                useTeams ?
                                    <>
                                        <Form.Label>Team</Form.Label>
                                        <Form.Select
                                            id={"team-select"}
                                            aria-label={"teams"}
                                            required={true}
                                            defaultValue={driverToEdit.teamName}
                                            onChange={e => {
                                                let driverToEditTmp: Driver = {...driverToEdit};
                                                driverToEditTmp.teamName = e.target.value;
                                                //driverToEditTmp.color = teamColorDict.current[e.target.value]
                                                setDriverToEdit({...driverToEditTmp});
                                            }}
                                        >
                                            {
                                                teamsList.map((team, index) => (
                                                    <option key={index} value={team.name}>{team.name}</option>
                                                ))
                                            }
                                        </Form.Select>
                                    </>
                                    :
                                    <>
                                        <Form.Label>Color</Form.Label>
                                        <Form.Control
                                            type="color"
                                            required
                                            value={driverToEdit.color}
                                            onChange={e => {
                                                let driverToEditTmp: Driver = {...driverToEdit};
                                                driverToEditTmp.color = e.target.value;
                                                setDriverToEdit({...driverToEditTmp});
                                            }}
                                        />
                                    </>
                            }
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseDriverEdit}>
                        Close
                    </Button>
                    <Button type={"submit"} form={"editDriverForm"} variant="primary">
                        Save Changes
                    </Button>
                </Modal.Footer>
            </Modal>

            <Modal show={showEditTeam} onHide={handleCloseTeamEdit} animation={false}>
                <Modal.Header closeButton>
                    <Modal.Title>Team Edit</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form id={"editTeamForm"} className={"add-form"} onSubmit={handleSaveTeamEdit}>
                        <Form.Group className="mb-3" controlId="editTeam">
                            <Form.Label>Name</Form.Label>
                            <Form.Control
                                type="text"
                                autoFocus
                                required
                                value={teamToEdit.name}
                                onChange={e => {
                                    let teamToEditTmp: Team = {...teamToEdit};
                                    teamToEditTmp.name = e.target.value;

                                    for(let x of teamsList) {
                                        if(x.name === teamToEditTmp.name){
                                            e.target.classList.add("error-box")
                                            isValidTeamToEditName.current = false
                                            editTeamErrorText.current = "Team with this name already exists"
                                            setTeamToEdit({...teamToEditTmp})
                                            return;
                                        }
                                    }
                                    e.target.classList.remove("error-box")
                                    isValidTeamToEditName.current = true
                                    if(isValidTeamToEdit())
                                        editTeamErrorText.current = ""
                                    setTeamToEdit({...teamToEditTmp});
                                }}/>
                            {
                                editTeamErrorText.current &&
                                <div className={"mt-3"}>
                                    <p className={"text-danger"}>{editTeamErrorText.current}</p>
                                </div>
                            }

                            <Form.Label>Color</Form.Label>
                            <Form.Control
                                type="color"
                                required
                                value={teamToEdit.color}
                                onChange={e => {
                                    let teamToEditTmp: Team = {...teamToEdit};
                                    teamToEditTmp.color = e.target.value;
                                    teamColorDict.current[teamToEditTmp.name] = teamToEditTmp.color
                                    setTeamToEdit({...teamToEditTmp});
                                }}/>
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseTeamEdit}>
                        Close
                    </Button>
                    <Button type={"submit"} form={"editTeamForm"} variant="primary">
                        Save Changes
                    </Button>
                </Modal.Footer>
            </Modal>

            <Modal show={showRemoveTeams} onHide={handleCloseRemoveTeams}>
                <Modal.Header closeButton></Modal.Header>
                <Modal.Body>Are you sure, you want to remove teams?</Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseRemoveTeams}>
                        Close
                    </Button>
                    <Button variant="danger" onClick={handleRemoveTeams}>
                        Remove
                    </Button>
                </Modal.Footer>
            </Modal>

            <Modal show={showErrorModal} onHide={handleCloseErrorModal}>
                <Modal.Header closeButton></Modal.Header>
                <Modal.Body>{validateCollectionErrorText.current}</Modal.Body>
                <Modal.Footer>
                    <Button variant="primary" onClick={handleCloseErrorModal}>
                        OK
                    </Button>
                </Modal.Footer>
            </Modal>

        </div>
    );
}

export default CollectionEditorComponent;