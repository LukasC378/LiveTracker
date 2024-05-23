import React, {useEffect, useRef, useState} from "react";
import LoadingComponent from "../Loading";
import {Button, Col, Modal, Row} from "react-bootstrap";
import CollectionService from "../../Services/CollectionService";
import {Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow} from "@mui/material";
import {Pencil, Eye, Trash} from "react-bootstrap-icons";
import {Link, useNavigate} from "react-router-dom";
import {CollectionBasicVM} from "../../Models/Collection";
import CollectionView from "./CollectionView";

const MyCollectionsComponent = () => {

    const navigate = useNavigate()

    //region useRef

    const collectionToShow: React.MutableRefObject<number> = useRef(0);

    //endregion

    //region useState

    const [isLoading, setIsLoading]: [boolean, any] = useState(true);
    const [collections, setCollections]: [CollectionBasicVM[], any] = useState([]);
    const [showCollectionModal, setShowCollectionModal]: [boolean, any] = useState(false);

    //endregion

    //region useEffect

    useEffect(() => {
        CollectionService.getCollections().then(res => {
            setCollections(() => [...res.data])
            setIsLoading(() => false)
        }).catch(() => {
            //setIsLoading(() => false)
        })
    }, []);

    //endregion

    // region functions

    function handleCloseCollectionModal() {
        setShowCollectionModal(() => false)
    }

    function handleRemoveCollection(collectionId: number, index: number) {
        let collectionsTmp = [...collections]
        collectionsTmp.splice(index, 1)
        setCollections(() => collectionsTmp)

        setIsLoading(() => true)
        CollectionService.deleteCollection(collectionId).then(() => {
            setIsLoading(() => false)
        })
    }

    //endregion

    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    return (
        <div>
            <Row>
                <Col className={'m-3'}>
                    <Link to="/collections/create" className={"btn btn-primary"}>Create New Collection</Link>
                </Col>
            </Row>
            <Row>
                {
                    collections.length == 0 ?
                        <div>
                            <p>You have no collections</p>
                        </div>
                        :
                        <TableContainer component={Paper}>
                            <Table sx={{ minWidth: 650 }} aria-label="collections table">
                                <TableHead className={"table-head"}>
                                    <TableRow>
                                        <TableCell className={"white-font"}>Name</TableCell>
                                        <TableCell className={"white-font"}>View</TableCell>
                                        <TableCell className={"white-font"}>Edit</TableCell>
                                        <TableCell className={"white-font"}>Delete</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {collections.map((collection, index) => (
                                        <TableRow
                                            key={index}
                                            sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                        >
                                            <TableCell>{collection.name}</TableCell>
                                            <TableCell>
                                                <Button
                                                    className={"btn-secondary"}
                                                    onClick={() => {
                                                        collectionToShow.current = collection.id
                                                        setShowCollectionModal(() => true)
                                                    }}
                                                >
                                                    <Eye></Eye>
                                                </Button>
                                            </TableCell>
                                            <TableCell>
                                                <Button
                                                    className={"btn-secondary"}
                                                    onClick={() => {
                                                        navigate("/collections/edit/"+collection.id)
                                                    }}
                                                >
                                                    <Pencil></Pencil>
                                                </Button>
                                            </TableCell>
                                            <TableCell>
                                                <Button
                                                    className={"btn-danger"}
                                                    onClick={() => {
                                                        handleRemoveCollection(collection.id, index)
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
                }
            </Row>

            <Modal
                show={showCollectionModal}
                onHide={handleCloseCollectionModal}
                animation={false}
                dialogClassName={'wider-modal'}
            >
                <Modal.Header closeButton></Modal.Header>
                <Modal.Body style={{overflowY: "auto"}}>
                    {collectionToShow.current !== 0 && <CollectionView collectionId={collectionToShow.current} />}
                </Modal.Body>
            </Modal>
        </div>
    )
}

export default MyCollectionsComponent;