import {SessionResultVM} from "../../Models/Session";
import React, {useEffect, useState} from "react";
import LoadingComponent from "../Loading";
import SessionService from "../../Services/SessionService";
import {useNavigate, useParams} from "react-router-dom";
import {Row} from "react-bootstrap";
import {Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow} from "@mui/material";
import {Utils} from "../../Utils/Utils";
import {Constants} from "../../Constants";

const RaceResultComponent = () => {

    const {sessionId} = useParams();
    const navigate = useNavigate()

    const [isLoading, setIsLoading]: [boolean, any] = useState(true)
    const [sessionResult, setSessionResult]: [SessionResultVM, any] = useState(Constants.defaultSessionResult)

    useEffect(() => {
        if(sessionId === undefined || isNaN(+sessionId!)){
            navigate("/404")
            return
        }
        SessionService.getSessionResult(+sessionId!).then(res => {
            setSessionResult(() => res.data)
            setIsLoading(() => false)
        })
    }, []);

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return(
        <Row>
            <h1>
                {sessionResult.name}
            </h1>
            <div style={{maxHeight: '900px', overflowY: 'auto'}}>
                <TableContainer component={Paper}>
                    <Table sx={{ minWidth: 650 }} aria-label="drivers table">
                        <TableHead>
                            <TableRow>
                                <TableCell className={"white-font"}>Pos</TableCell>
                                <TableCell className={"white-font"}>Name</TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {sessionResult.drivers.map((driver, index) => (
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
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            </div>
        </Row>
    )
}

export default RaceResultComponent;