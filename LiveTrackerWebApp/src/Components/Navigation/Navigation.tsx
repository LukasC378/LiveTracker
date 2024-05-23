import {Link, useNavigate} from "react-router-dom";
import {Button, Container, Nav, Navbar, Offcanvas} from "react-bootstrap";
import "./Navigation.css";
import {Utils} from "../../Utils/Utils";
import {useContext, useEffect, useState} from "react";
import {UserContext} from "../../App";
import UserService from "../../Services/UserService";
import {Constants} from "../../Constants";
import {UserRoleEnum} from "../../Models/Enums";

const NavigationComponent = () => {
    const navigate = useNavigate();

    let {currentUser, setCurrentUser}: any = useContext(UserContext);
    let [isUserLogged, setIsUserLogged]: [boolean, any] = useState(false)

    useEffect(() => {
        setIsUserLogged(() => Utils.userIsLogged(currentUser))
    }, [currentUser]);

    function logout() {
        UserService.logout().then(() => {
            sessionStorage.removeItem('user');
            setCurrentUser(Constants.defaultUser);
        }).catch(err => {
            console.log(err)
            //alert(Constants.unexpectedErrorMessage);
        });
    }

    return(
        <Navbar bg={"dark"} expand={false}>
            <Container fluid>
                <Navbar.Brand className={"text-light"} onClick={() => navigate("/")}>
                    Live Tracker
                </Navbar.Brand>
                <Navbar.Toggle className={"bg-light"} aria-controls={"offcanvasNavbar"}/>
                <Navbar.Offcanvas id={"offcanvasNavbar"} aria-labelledby={"offcanvasNavbarLabel"} placement={"end"}>
                    <Offcanvas.Header closeButton>
                        <Offcanvas.Title id="offcanvasNavbarLabel">Menu</Offcanvas.Title>
                    </Offcanvas.Header>
                    <Offcanvas.Body>
                        <Nav className={"justify-content-end flex-grow-1 pe-3"}>
                            <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                <Nav.Link href={"/"}>Home</Nav.Link>
                            </Navbar.Toggle>
                            <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                <Nav.Link href={"/sessions/live"}>Live Sessions</Nav.Link>
                            </Navbar.Toggle>
                            <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                <Nav.Link href={"/sessions/schedule"}>Schedule</Nav.Link>
                            </Navbar.Toggle>
                            {
                                isUserLogged && currentUser.role === UserRoleEnum.Organizer &&
                                <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                    <Nav.Link href={"/sessions/create"}>Create Session</Nav.Link>
                                </Navbar.Toggle>
                            }
                            {
                                isUserLogged && currentUser.role === UserRoleEnum.Organizer &&
                                <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                    <Nav.Link href={"/sessions/manage"}>Manage Sessions</Nav.Link>
                                </Navbar.Toggle>
                            }
                            {
                                isUserLogged && currentUser.role === UserRoleEnum.Organizer &&
                                <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                    <Nav.Link href={"/collections"}>My Collections</Nav.Link>
                                </Navbar.Toggle>
                            }
                            {
                                isUserLogged && currentUser.role === UserRoleEnum.Organizer &&
                                <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                    <Nav.Link href={"/collections/create"}>Create Collection</Nav.Link>
                                </Navbar.Toggle>
                            }
                            {
                                isUserLogged && currentUser.role === UserRoleEnum.Organizer &&
                                <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                    <Nav.Link href={"/layouts"}>My Layouts</Nav.Link>
                                </Navbar.Toggle>
                            }
                            {
                                isUserLogged && currentUser.role === UserRoleEnum.Organizer &&
                                <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                    <Nav.Link href={"/layouts/create"}>Create Layout</Nav.Link>
                                </Navbar.Toggle>
                            }
                            {
                                isUserLogged && currentUser.role === UserRoleEnum.NormalUser &&
                                <Navbar.Toggle aria-controls={"offcanvasNavbar"}>
                                    <Nav.Link href={"/organizers"}>Organizers</Nav.Link>
                                </Navbar.Toggle>
                            }
                            {
                                isUserLogged ?
                                    (<span className={'d-flex justify-content-center mt-1'} style={{marginRight: '10px'}}>
                                        <Button variant={"outline-dark"} onClick={logout}>Logout</Button>
                                    </span>)
                                    :
                                    (<Link to="/login" state={{return_url: window.location.href}} className={"btn btn-dark me-2"}>Login</Link>)
                            }
                            {
                                isUserLogged ?
                                    (<span className={"text-light mt-2"}>{currentUser.username}</span>):
                                    (<Link to="/sign-up" className={"btn btn-outline-dark me-2"}>Sign up</Link>)
                            }
                        </Nav>
                    </Offcanvas.Body>
                </Navbar.Offcanvas>
            </Container>
        </Navbar>
    );
}

export default NavigationComponent;