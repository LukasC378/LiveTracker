import {Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow} from "@mui/material";
import React, {useEffect, useState} from "react";
import CollectionService from "../../Services/CollectionService";
import LoadingComponent from "../Loading";
import {Button, Col, Row} from "react-bootstrap";
import "./Collections.css"
import {Collection} from "../../Models/Collection";

const CollectionViewComponent = (props: any) => {

    const defaultCollection: Collection = {
        id: 0,
        name: '',
        drivers: [],
        teams: [],
        useTeams: false
    }

    const maxDisplayedCount: number = 5;

    const [isLoading, setIsLoading]: [boolean, any] = useState(true);
    const [collection, setCollection]: [Collection, any] = useState(defaultCollection);
    const [displayedDrivers, setDisplayedDriver]: [number, any] = useState(maxDisplayedCount);
    const [displayedTeams, setDisplayedTeams]: [number, any] = useState(maxDisplayedCount);

    useEffect(() => {
        CollectionService.getCollection(props.collectionId).then(res => {
            console.log(res.data)
            setIsLoading(() => false)
            setCollection(() => res.data)
        }).catch(() => {
            setIsLoading(() => false)
        })
    }, []);

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return(
        <div>
            {
                collection &&
                <div>
                    <h1>Drives</h1>
                    <TableContainer component={Paper} style={{maxHeight: '400px'}}>
                        <Table sx={{ minWidth: 650 }} aria-label="drivers table">
                            <TableHead className={"table-head"}>
                                <TableRow>
                                    <TableCell className={"white-font"}>Number</TableCell>
                                    <TableCell className={"white-font"}>Name</TableCell>
                                    <TableCell className={"white-font"}>Surname</TableCell>
                                    <TableCell className={"white-font"}>GPS Device</TableCell>
                                    {collection.useTeams && <TableCell className={"white-font"}>Team</TableCell>}
                                    <TableCell className={"white-font"}>Color</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {collection.drivers.map((driver, index) => (
                                    <TableRow
                                        id={`driverRow${index}`}
                                        key={index}
                                        sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                    >
                                        <TableCell>{driver.number}</TableCell>
                                        <TableCell>{driver.name}</TableCell>
                                        <TableCell>{driver.surname}</TableCell>
                                        <TableCell>{driver.gpsDevice}</TableCell>
                                        {collection.useTeams && <TableCell id={`driver${index}team`}>{driver.teamName}</TableCell>}
                                        <TableCell>
                                            <div className={"color-div"} style={{backgroundColor: driver.color}}></div>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>

                    {/*<Row className={'m-2'}>*/}
                    {/*    <Col>*/}

                    {/*{*/}
                    {/*    collection.drivers.length > maxDisplayedCount && displayedDrivers < collection.drivers.length &&*/}
                    {/*        <Button className={'btn-primary'} onClick={() => setDisplayedDriver(() => collection.drivers.length)}>*/}
                    {/*            Show All*/}
                    {/*        </Button>*/}
                    {/*}*/}
                    {/*{*/}
                    {/*    collection.drivers.length > maxDisplayedCount && displayedDrivers === collection.drivers.length &&*/}
                    {/*        <Button className={'btn-primary'} onClick={() => setDisplayedDriver(() => maxDisplayedCount)}>*/}
                    {/*            Show Less*/}
                    {/*        </Button>*/}
                    {/*}*/}

                    {/*    </Col>*/}
                    {/*</Row>*/}
                </div>
            }
            {
                collection.useTeams &&
                <div>
                    <h1>Teams</h1>
                    <TableContainer component={Paper} style={{maxHeight: '400px'}}>
                        <Table sx={{ minWidth: 650 }} aria-label="simple table">
                            <TableHead className={"table-head"}>
                                <TableRow>
                                    <TableCell className={"white-font"}>Name</TableCell>
                                    <TableCell className={"white-font"}>Color</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {collection.teams.map((team, index) => (
                                    <TableRow
                                        key={index}
                                        sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                    >
                                        <TableCell>{team.name}</TableCell>
                                        <TableCell>
                                            <div className={"color-div"} style={{backgroundColor: team.color}}></div>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                    {/*<Row className={'m-2'}>*/}
                    {/*    <Col>*/}
                    {/*        {*/}
                    {/*            collection.teams!.length > maxDisplayedCount && displayedTeams < collection.teams!.length &&*/}
                    {/*            <Button className={'btn-primary'} onClick={() => setDisplayedTeams(() => collection.teams!.length)}>*/}
                    {/*                Show All*/}
                    {/*            </Button>*/}
                    {/*        }*/}
                    {/*        {*/}
                    {/*            collection.teams!.length > maxDisplayedCount && displayedTeams === collection.teams!.length &&*/}
                    {/*            <Button className={'btn-primary'} onClick={() => setDisplayedTeams(() => maxDisplayedCount)}>*/}
                    {/*                Show Less*/}
                    {/*            </Button>*/}
                    {/*        }*/}
                    {/*    </Col>*/}
                    {/*</Row>*/}
                </div>
            }
        </div>
    )
}

export default CollectionViewComponent;