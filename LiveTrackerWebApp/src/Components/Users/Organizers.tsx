import React, {useEffect, useRef, useState} from "react";
import UserService from "../../Services/UserService";
import LoadingComponent from "../Loading";
import {BottomScrollListener} from "react-bottom-scroll-listener";
import {Button, Col, Form, Row} from "react-bootstrap";
import LoadingComponentSmall from "../LoadingSmall";
import {OrganizerTypeEnum} from "../../Models/Enums";
import {OrganizersFilter, OrganizerVM} from "../../Models/User";
import Spinner from "react-spinner-material";
import "./Organizers.css"

const OrganizersComponent = () => {
    const initLoad: React.MutableRefObject<boolean> = useRef(true);
    const offset: React.MutableRefObject<number> = useRef(0);
    const limit: React.MutableRefObject<number> = useRef(50);
    const loadingEnabled: React.MutableRefObject<boolean> = useRef(true);
    const organizers: React.MutableRefObject<OrganizerVM[]> = useRef([]);
    const loadings: React.MutableRefObject<number[]> = useRef([]);

    const [, forceUpdate]: [any, any] = useState();
    const [isLoading, setIsLoading]: [boolean, any] = useState(true)
    const [searchTerm, setSearchTerm]: [string, any] = useState('');
    const [organizersType, setOrganizersType]: [OrganizerTypeEnum, any] = useState(OrganizerTypeEnum.All)

    useEffect(() => {
        offset.current = 0
        organizers.current = []
        getOrganizers()
    }, [searchTerm, organizersType]);

    const handleScroll = () => {
        if(isLoading || !loadingEnabled.current)
            return
        offset.current += limit.current
        getOrganizers()
    };

    function handleSubscribe(organizerId: number){
        loadings.current.push(organizerId)
        forceUpdate({})

        UserService.subscribe(organizerId).then(() => {
            organizers.current.forEach(x => {
                if(x.id === organizerId){
                    x.subscribed = true
                    return
                }
            })
            loadings.current = loadings.current.filter(x => x !== organizerId)
            forceUpdate({})
        })
    }

    function handleUnsubscribe(organizerId: number){
        loadings.current.push(organizerId)
        forceUpdate({})

        UserService.unsubscribe(organizerId).then(() => {
            organizers.current.forEach(x => {
                if(x.id === organizerId){
                    x.subscribed = false
                    return
                }
            })
            loadings.current = loadings.current.filter(x => x !== organizerId)
            forceUpdate({})
        })
    }

    function getOrganizers(){
        setIsLoading(() => true)
        const filter: OrganizersFilter = {
            limit: limit.current,
            offset: offset.current,
            searchTerm: searchTerm,
            type: organizersType
        }
        UserService.getOrganizers(filter).then(res => {
            if(res.data.length === 0){
                loadingEnabled.current = false
                initLoad.current = false
                setIsLoading(() => false)
                return
            }
            organizers.current = [...organizers.current, ...res.data]
            setIsLoading(() => false)
        })
    }

    if(isLoading && initLoad){
        return(
            <LoadingComponent/>
        )
    }
    return (
        <BottomScrollListener onBottom={handleScroll}>
            <>
                <Row className={"m-2"}>
                    <Form className="d-flex">
                        <Col className={'col-md-3 m-1'}>
                            <Form.Group controlId="subscription-type">
                                <Form.Label>Type</Form.Label>
                                <Form.Select
                                    id={"organizer-sessionState-select"}
                                    required={true}
                                    value={organizersType}
                                    onChange={e => {
                                        setOrganizersType(() => e.target.value)
                                    }}
                                >
                                    <option key={0} value={OrganizerTypeEnum.All}>All</option>
                                    <option key={1} value={OrganizerTypeEnum.Subscribed}>Subscribed</option>
                                    <option key={2} value={OrganizerTypeEnum.Others}>Others</option>
                                </Form.Select>
                            </Form.Group>
                        </Col>
                        <Col className={'col-md-3 m-1'}>
                            <Form.Group controlId="organizer">
                                <Form.Label>Search Organizer</Form.Label>
                                <Form.Control
                                    type="search"
                                    placeholder="Search"
                                    className="me-2"
                                    aria-label="Search"
                                    value={searchTerm}
                                    autoFocus={true}
                                    onChange={e => {
                                        const searchTermValue = e.target.value
                                        setSearchTerm(() => searchTermValue)
                                    }}
                                />
                            </Form.Group>
                        </Col>
                    </Form>
                </Row>
                {
                    organizers.current.map((organizer, index) => {
                        const isLoadingButton = loadings.current.includes(organizer.id)

                        return (
                        <Row key={index} className={"p-2"}>
                            <Col className={"col-md-4 col-sm-2"}>
                                <div className={"d-flex ms-5"}>
                                    {organizer.name}
                                </div>
                            </Col>
                            <Col className={"col-md-4 col-sm-2"}>
                                {
                                    organizer.subscribed &&
                                    <Button
                                        disabled={isLoadingButton}
                                        className={"btn-danger subscribe-button"}
                                        onClick={() => handleUnsubscribe(organizer.id)}
                                    >
                                        {
                                            isLoadingButton &&
                                            <div className={"d-flex justify-content-center"}>
                                                <Spinner radius={15} color={"white"} stroke={2} visible={true}/>
                                            </div>
                                        }
                                        {
                                            !isLoadingButton &&
                                            <div className={'d-flex justify-content-center'}>
                                                Unsubscribe
                                            </div>
                                        }
                                    </Button>
                                }
                                {
                                    !organizer.subscribed &&
                                    <Button
                                        disabled={isLoadingButton}
                                        className={"btn-primary subscribe-button"}
                                        onClick={() => handleSubscribe(organizer.id)}
                                    >
                                        {
                                            isLoadingButton &&
                                            <div className={"d-flex justify-content-center"}>
                                                <Spinner radius={15} color={"white"} stroke={2} visible={true}/>
                                            </div>
                                        }
                                        {
                                            !isLoadingButton &&
                                            <div className={'d-flex justify-content-center'}>
                                                Subscribe
                                            </div>
                                        }
                                    </Button>
                                }
                            </Col>
                        </Row>
                    )})
                }
                {
                    isLoading &&
                    <LoadingComponentSmall/>
                }
            </>
        </BottomScrollListener>
    )
}

export default OrganizersComponent;