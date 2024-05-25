import {Button, Col, Row} from "react-bootstrap";
import "./Sessions.css";
import React, {forwardRef} from "react";
import {useNavigate} from "react-router-dom";
import {SessionGroupVM} from "../../Models/Session";
import {SessionStateEnum} from "../../Models/Enums";

const SessionCardsComponent = forwardRef((props: any, _) => {

    const sessions: SessionGroupVM[] = props.sessions;
    const navigate = useNavigate();

    function getSessionButton(state: SessionStateEnum, sessionId: number){
        switch (state){
            case SessionStateEnum.Preview:
                return(
                    <div>
                        <Button
                            className={"btn-primary"}
                            onClick={() => {
                                navigate('/sessions/'+sessionId)
                            }}
                        >
                            Preview
                        </Button>
                    </div>
                );
            case SessionStateEnum.Live:
                return(
                    <div>
                        <Button
                            className={"btn-success"}
                            onClick={() => {
                                navigate('/sessions/'+sessionId)
                            }}
                        >
                            JOIN
                        </Button>
                    </div>
                );
            case SessionStateEnum.Archived:
                return(
                    <Row>
                        <Col>
                            <Button
                                className={"btn-warning"}
                                onClick={() => {
                                    navigate('/sessions/'+sessionId)
                                }}
                            >
                                Watch from archive
                            </Button>
                        </Col>
                        <Col>
                            <Button
                                className={"btn-primary"}
                                onClick={() => {
                                    navigate('/sessions/result/'+sessionId)
                                }}
                            >
                                Results
                            </Button>
                        </Col>
                    </Row>
                );
        }
    }

    if(sessions.length === 0){
        return (
            <Row>
                <Col>
                    <div className={'m-3'}>
                        No Sessions
                    </div>
                </Col>
            </Row>
        )
    }
    return(
        <>
            {
                sessions.map((sessionGroup, index1) => (
                        <div key={`group${index1}`}>
                            <h3>{new Date(sessionGroup.date).toLocaleDateString()}</h3>
                            <Row className={"m-2"}>
                                {
                                    sessionGroup.sessions.map((session, index2) => (
                                        <Col key={`group${index1}session${index2}`} lg={4} md={6} sm={8} className={"mb-4"}>
                                            <div className={"p-4 session-card"}>
                                                <div>
                                                    <div className={"pb-2"}>
                                                        <h3>{session.name}</h3>
                                                    </div>
                                                    <div className={"text-start"}>
                                                        <p>Scheduled from: {new Date(session.scheduledFrom).toLocaleString()}</p>
                                                        <p>Scheduled to: {new Date(session.scheduledTo).toLocaleString()}</p>
                                                        <p>Organizer: {session.organizer}</p>
                                                    </div>
                                                    {
                                                        getSessionButton(session.state, session.id)
                                                    }
                                                </div>
                                            </div>
                                        </Col>
                                    ))
                                }
                            </Row>
                        </div>
                ))
            }
        </>
    )
})

export default SessionCardsComponent;