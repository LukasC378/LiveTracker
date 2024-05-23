import {SessionToManageVM} from "../../Models/Session";
import React, {useEffect, useState} from "react";
import SessionService from "../../Services/SessionService";
import {Button, Col, Row} from "react-bootstrap";
import {Pencil, Trash} from "react-bootstrap-icons";
import {useNavigate} from "react-router-dom";
import LoadingComponent from "../Loading";

const SessionsManagerComponent = () => {

    const navigate = useNavigate()

    const [sessions, setSessions]: [SessionToManageVM[], any] = useState([]);
    const [isLoading, setIsLoading]: [boolean, any] = useState(false);

    useEffect(() => {
        SessionService.getSessionsToManage().then(res => {
            setSessions(() => res.data)
            setIsLoading(() => false)
        })
    }, []);

    function handleCancelSession(sessionId: number, index: number) {
        let sessionsTmp = [...sessions]
        sessionsTmp.splice(index, 1)
        setSessions(() => sessionsTmp)
        SessionService.cancelSession(sessionId).then(() => {})
    }

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return(
        <div>
            {
                sessions.length == 0 ?
                    <div>
                        <p>You have no session to manage</p>
                    </div>
                    :
                    <Row className={"m-2"}>
                        {
                            sessions.map((session, index) => (
                                <Col key={index} lg={4} md={6} sm={8} className={"mb-4"}>
                                    <div className={"p-4 session-card"}>
                                        <div>
                                            <div className={"pb-2"}>
                                                <h3>{session.name}</h3>
                                            </div>
                                            <div className={"text-start"}>
                                                <p>Scheduled from: {new Date(session.scheduledFrom).toLocaleString()}</p>
                                                <p>Scheduled to: {new Date(session.scheduledTo).toLocaleString()}</p>
                                            </div>
                                            <div className={"d-flex justify-content-evenly"}>
                                                <Button
                                                    className={"btn-secondary"}
                                                    onClick={() => {
                                                        navigate("/sessions/edit/" + session.id)
                                                    }}
                                                >
                                                    <Pencil></Pencil>
                                                </Button>
                                                <Button
                                                    className={"btn-danger"}
                                                    onClick={() => {
                                                        handleCancelSession(session.id, index)
                                                    }}
                                                >
                                                    <Trash></Trash>
                                                </Button>
                                            </div>
                                        </div>
                                    </div>
                                </Col>
                            ))
                        }
                    </Row>
            }
        </div>
    )
}

export default SessionsManagerComponent;