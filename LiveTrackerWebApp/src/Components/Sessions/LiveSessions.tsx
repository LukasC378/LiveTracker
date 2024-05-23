import React, {useEffect, useState} from "react";
import SessionService from "../../Services/SessionService";
import SessionCards from "./SessionCards";
import {SessionGroupVM} from "../../Models/Session";
import LoadingComponent from "../Loading";

const LiveSessionsComponent = () => {

    const [sessions, setSessions]: [SessionGroupVM[], any] = useState([]);
    const [isLoading, setIsLoading]: [boolean, any] = useState(false);

    useEffect(() => {
        SessionService.getLiveSessions().then(res => {
            setSessions(() => res.data)
            setIsLoading(() => false)
        })
    }, []);

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return (
        <>
            <SessionCards sessions={sessions} />
        </>
    )
}

export default LiveSessionsComponent;