import React, {useEffect, useState} from "react";
import {useNavigate, useParams} from "react-router-dom";
import sessionService from "../../Services/SessionService";
import LoadingComponent from "../Loading";
import {SessionStateEnum} from "../../Models/Enums";
import LiveRace from "./LiveRace";
import ArchivedRace from "./ArchivedRace";
import RacePreview from "./RacePreview";

const RaceComponent = () => {

    const {sessionId} = useParams();
    const navigate = useNavigate();

    const [component, setComponent]: [any, any] = useState(<LoadingComponent />)

    useEffect(() => {
        if(sessionId === undefined || isNaN(+sessionId)){
            console.log(':)')
            navigate('/sessions/schedule')
            return
        }
        sessionService.getSessionState(+sessionId).then(res => {
            switch (res.data){
                case SessionStateEnum.Preview:
                    setComponent(() => <RacePreview/>)
                    break
                case SessionStateEnum.Live:
                    setComponent(() => <LiveRace/>)
                    break
                case SessionStateEnum.Archived:
                    setComponent(() => <ArchivedRace/>)
                    break
            }
        })
    }, []);

    return(
        <>
            {
                component
            }
        </>
    )
}

export default RaceComponent;