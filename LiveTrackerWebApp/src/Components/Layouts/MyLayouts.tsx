import {Layout} from "../../Models/Layout";
import React, {useEffect, useState} from "react";
import LayoutService from "../../Services/LayoutService";
import {Button, Col, Row} from "react-bootstrap";
import {Pencil, Trash} from "react-bootstrap-icons";
import {Link, useNavigate} from "react-router-dom";
import GeoJsonImage from "../Race/GeoJsonImage";

const MyLayoutsComponent = () => {

    const navigate = useNavigate()

    const [layouts, setLayouts]: [Layout[], any] = useState([]);

    useEffect(() => {
        LayoutService.getLayouts().then(res => {
            setLayouts(() => res.data)
        })
    }, []);

    function handleRemoveLayout(layoutId: number, index: number) {
        let layoutsTmp = [...layouts]
        layoutsTmp.splice(index, 1)
        setLayouts(() => layoutsTmp)
        LayoutService.deleteLayout(layoutId).then(() => {})
    }

    return(
        <div>
            <Row>
                <Col className={'m-3'}>
                    <Link to="/layouts/create" className={"btn btn-primary"}>Create New Layout</Link>
                </Col>
            </Row>
            {
                layouts.length == 0 ?
                    <div>
                        <p>You have no layouts</p>
                    </div>
                    :
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
                                            <div className={"d-flex justify-content-evenly"}>
                                                <Button
                                                    className={"btn-secondary"}
                                                    onClick={() => {
                                                        navigate("/layouts/edit/" + layout.id)
                                                    }}
                                                >
                                                    <Pencil></Pencil>
                                                </Button>
                                                <Button
                                                    className={"btn-danger"}
                                                    onClick={() => {
                                                        handleRemoveLayout(layout.id, index)
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

export default MyLayoutsComponent;