import React, {useEffect, useRef, useState} from "react";
import SessionService from "../../Services/SessionService";
import {Col, Form, Row} from "react-bootstrap";
import SessionCards from "./SessionCards";
import {List, ListItem} from "@mui/material";
import "./Sessions.css"
import {SessionFilerUI, SessionGroupVM} from "../../Models/Session";
import UserService from "../../Services/UserService";
import LoadingComponent from "../Loading";
import {useBottomScrollListener} from "react-bottom-scroll-listener";
import LoadingComponentSmall from "../LoadingSmall";
import {OrganizerVM} from "../../Models/User";
import {SessionStateFilterEnum} from "../../Models/Enums";
import {Constants} from "../../Constants";

const SessionsScheduleComponent = () => {
    const offset: React.MutableRefObject<number> = useRef(0);
    const limit: React.MutableRefObject<number> = useRef(20);
    const loadingEnabled: React.MutableRefObject<boolean> = useRef(true);
    const sessions: React.MutableRefObject<SessionGroupVM[]> = useRef([])

    const [isLoading, setIsLoading]: [boolean, any] = useState(true)
    const [organizers, setOrganizers]: [OrganizerVM[], any] = useState([]);

    const [filterUI, setFilterUI]: [SessionFilerUI, any] = useState(Constants.defaultSessionFilterUI)
    const [organizerSearchTerm, setOrganizerSearchTerm]: [string, any] = useState('');

    const handleScroll = () => {
        console.log(isLoading, loadingEnabled.current)
        if(isLoading || !loadingEnabled.current)
            return
        console.log(':)')
        offset.current += limit.current
        getSessions()
    };

    const scrollRef = useBottomScrollListener(handleScroll, {
        offset: 100,
        debounce: 0,
        triggerOnNoScroll: true
    });

    useEffect(() => {
        offset.current = 0
        sessions.current = []
        //setSessions(() => [])
        getSessions()
    }, [filterUI]);

    function getOrganizers(searchTermValue: string){
        if(!searchTermValue){
            if(organizers.length > 0)
                setOrganizers(() => [])
            return
        }
        UserService.getOrganizersForSearch(searchTermValue).then(res => {
            setOrganizers(() => res.data)
        })
    }

    function getSessions(){
        setIsLoading(() => true)
        SessionService.getSessions({
            ...filterUI,
            limit: limit.current,
            offset: offset.current
        }).then(res => {
            console.log(sessions)
            if(res.data.length === 0){
                loadingEnabled.current = false
                setIsLoading(() => false)
                return
            }

            if(sessions.current.length > 0){
                if(sessions.current[sessions.current.length - 1].date === res.data[0].date){
                    sessions.current[sessions.current.length - 1].sessions = [...sessions.current[sessions.current.length - 1].sessions, ...res.data[0].sessions]
                    sessions.current = [...sessions.current, ...res.data.slice(1)]
                }
                else{
                    sessions.current = [...sessions.current, ...res.data]
                }
            }
            else{
                sessions.current = [...sessions.current, ...res.data]
            }

            loadingEnabled.current = true
            setIsLoading(() => false)
        }).catch(e => console.log(e))
    }


    if(isLoading && sessions.current.length === 0){
        return(
            <LoadingComponent/>
        )
    }
    return (
        <>
            <Row className={"m-2"}>
                <Form>
                    <Row>
                        <Col className={'col-6 col-sm-5 col-md-2 m-1'}>
                            <Form.Group controlId="date">
                                <Form.Label>Select Date</Form.Label>
                                <Form.Control
                                    type="date"
                                    name="date"
                                    defaultValue={filterUI.date}
                                    onChange={e => {
                                        let filterUITmp = {...filterUI}
                                        filterUITmp.date = e.target.value
                                        setFilterUI(() => filterUITmp)
                                    }}
                                />
                            </Form.Group>
                        </Col>
                        <Col className={'col-6 col-sm-5 col-md-3 m-1'}>
                            <Form.Group controlId="organizer">
                                <Form.Label>Search Organizer</Form.Label>
                                <Form.Control
                                    type="search"
                                    placeholder="Search"
                                    className="me-2"
                                    aria-label="Search"
                                    value={organizerSearchTerm}
                                    onChange={e => {
                                        const searchTermValue = e.target.value
                                        getOrganizers(searchTermValue)
                                        setOrganizerSearchTerm(() => searchTermValue)
                                    }}
                                />
                                {
                                    organizers.length > 0 &&
                                    <List className={'search-bar'}>
                                        {
                                            organizers.map((organizer, index) => (
                                                <ListItem
                                                    key={index}
                                                    className={'search-item'}
                                                    onClick={() => {
                                                        setOrganizerSearchTerm(() => organizer.name)
                                                        setOrganizers(() => [])

                                                        let filterUITmp = {...filterUI}
                                                        filterUITmp.organizerId = organizer.id
                                                        setFilterUI(() => filterUITmp)
                                                    }}
                                                >
                                                    {organizer.name}
                                                </ListItem>
                                            ))
                                        }
                                    </List>
                                }
                            </Form.Group>
                        </Col>
                        <Col className={'col-6 col-sm-5 col-md-3 m-1'}>
                            <Form.Group controlId="organizer">
                                <Form.Label>Search Session</Form.Label>
                                <Form.Control
                                    type="search"
                                    placeholder="Search"
                                    className="me-2"
                                    aria-label="Search"
                                    value={filterUI.searchTerm}
                                    onChange={e => {
                                        let filterUITmp = {...filterUI}
                                        filterUITmp.searchTerm = e.target.value
                                        setFilterUI(() => filterUITmp)
                                    }}
                                />
                            </Form.Group>
                        </Col>
                        <Col className={'col-6 col-sm-5 col-md-2 m-1'}>
                            <Form.Group controlId="type">
                                <Form.Label>Event</Form.Label>
                                <Form.Select
                                    value={filterUI.sessionState}
                                    onChange={e => {
                                        let filterUITmp = {...filterUI}
                                        filterUITmp.sessionState = +e.target.value
                                        setFilterUI(() => filterUITmp)
                                    }}
                                >
                                    <option value={SessionStateFilterEnum.Upcoming}>Upcoming</option>
                                    <option value={SessionStateFilterEnum.Live}>Live</option>
                                    <option value={SessionStateFilterEnum.Archived}>Archived</option>
                                    <option value={SessionStateFilterEnum.All}>All</option>
                                </Form.Select>
                            </Form.Group>
                        </Col>
                        <Col className={'col-6 col-sm-5 col-md-2 m-1'}>
                            <Form.Group controlId="order">
                                <Form.Label>Date order</Form.Label>
                                <Form.Select
                                    value={filterUI.orderAsc ? 0 : 1}
                                    onChange={e => {
                                        let filterUITmp = {...filterUI}
                                        filterUITmp.orderAsc = +e.target.value === 0
                                        setFilterUI(() => filterUITmp)
                                    }}
                                >
                                    <option value={0}>Ascending</option>
                                    <option value={1}>Descending</option>
                                </Form.Select>
                            </Form.Group>
                        </Col>
                    </Row>
                </Form>
            </Row>
                <SessionCards ref={scrollRef} sessions={sessions.current} />
            {
                isLoading &&
                <LoadingComponentSmall/>
            }
        </>
    )
}

export default SessionsScheduleComponent;