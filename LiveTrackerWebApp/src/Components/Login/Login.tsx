import {Button, Form} from "react-bootstrap";
import "./Login.css"
import {Link, useLocation, useNavigate} from "react-router-dom";
import React, {useCallback, useContext, useEffect, useState} from "react";
import LoadingComponent from "../Loading";
import ErrorMessage from "../ErrorPages/ErrorMessage";
import {Utils} from "../../Utils/Utils";
import {UserContext} from "../../App";
import UserService from "../../Services/UserService";
import {Constants} from "../../Constants";
import {AxiosError} from "axios";
import {useGoogleReCaptcha} from "react-google-recaptcha-v3";
import RecaptchaService from "../../Services/RecaptchaService";
import {User} from "../../Models/User";

const actionName: string = "login";

const LoginComponent = () => {

    const location = useLocation();
    const navigate = useNavigate();
    const { executeRecaptcha } = useGoogleReCaptcha();
    let {currentUser, setCurrentUser}: any = useContext(UserContext);

    //region useState
    const [returnUrl, setReturnUrl]: [string, any] = useState('/')
    const [username, setUsername]: [string, any] = useState('');
    const [password, setPassword]: [string, any] = useState('');
    const [errorText, setErrorText]: [string, any] = useState('');
    const [isLoading, setIsLoading]: [boolean, any] = useState(false);
    const [userVerified, setUserVerified]: [boolean, any] = useState(true);
    //endregion

    //region useCallback
    const getRecaptchaToken = useCallback(async (): Promise<string> => {
        if (!executeRecaptcha) {
            console.log('Execute recaptcha not yet available');
            return "";
        }
        return await executeRecaptcha(actionName);
    }, [executeRecaptcha]);
    //endregion

    //region useEffect
    useEffect(() => {
        if (location.state) {
            let state = location.state as any
            if (state.return_url.endsWith('/401') || state.return_url.endsWith('/404')) {
                setReturnUrl("/")
            }
            else {
                setReturnUrl(state.return_url)
            }
        }
        else {
            setReturnUrl("/")
        }
    }, [])
    //endregion

    //region functions
    function loginUser(){
        UserService.login(username, password).then((res)=> {
            let user: User = res.data;
            sessionStorage.setItem('user', JSON.stringify(user));
            setCurrentUser(() => user);
            window.location.replace(returnUrl) //Redirect to last visited page
        }).catch((err: AxiosError)=> {
            console.log(err);
            setIsLoading(() => false);
            if(err.response!.status === 401){
                setErrorText("Username or password are incorrect")
            }
            else {
                setErrorText(Constants.unexpectedErrorMessage);
            }
        });
    }

    function verifyReCaptchaToken(token: string) {
        if (token == "") {
            setErrorText(() => Constants.unexpectedErrorMessage);
            setIsLoading(() => false);
            return;
        }
        RecaptchaService.verifyRecaptchaToken(token, actionName).then((res) => {
            if (res.data) {
                setUserVerified(() => true);
                loginUser();
            } else {
                setIsLoading(() => false);
                setUserVerified(() => false);
            }
        }).catch(() => {
            setErrorText(() => Constants.unexpectedErrorMessage);
            setIsLoading(() => false);
        });
    }

    function handleSubmit(e: any){
        e.preventDefault();
        setErrorText(() => "");
        setIsLoading(() => true);
        getRecaptchaToken().then(token => verifyReCaptchaToken(token));
    }

    function logout() {
        UserService.logout().then(() => {
            sessionStorage.removeItem('user');
            setCurrentUser(Constants.defaultUser);
        }).catch(err => {
            console.log(err)
            alert(Constants.unexpectedErrorMessage);
        });
    }
    //endregion

    //region return
    if(Utils.userIsLogged(currentUser) && !isLoading) {
        return (
            <div className={"container-fluid"}>
                <p className={'m-2'} style={{fontSize: '20px'}}>Logged as <strong>{currentUser.username}</strong></p>
                <Button className={"me-2"} variant={"primary"}
                        onClick={() => navigate("/")}>Home</Button>
                <Button className={"me-2"} variant={"outline-dark"} onClick={logout}>Logout</Button>
                <div className={"me-2"}>
                    <p className={"text-warning"}>{errorText}</p>
                </div>
            </div>
        )
    }
    if(isLoading){
        return(
            <LoadingComponent/>
        )
    }
    if(!userVerified){
        return(
            <ErrorMessage message={"You are not verified user"}/>
        );
    }
    return(
        <div className={"container-fluid d-flex justify-content-center"} style={{maxWidth: "400px"}}>
            <div >
                <Form className={"login-form"} onSubmit={handleSubmit}>
                    <Form.Group className="mb-3" controlId="username">
                        <Form.Label>Username</Form.Label>
                        <Form.Control
                            type="text"
                            autoFocus
                            value={username}
                            onChange={event => setUsername(event.target.value)}
                        />
                    </Form.Group>
                    <Form.Group className="mb-3" controlId="password">
                        <Form.Label>Password</Form.Label>
                        <Form.Control
                            type="password"
                            value={password}
                            onChange={event => setPassword(event.target.value)}
                        />
                    </Form.Group>
                    <div className={"mb-3"}>
                        <p className={"text-warning"}>{errorText}</p>
                    </div>
                    <div className={"mb-3"}>
                        <Button style={{width: 200}} type={"submit"} variant={"dark"}>Login</Button>
                    </div>
                </Form>
                {/*<div className={"pt-1"}>*/}
                {/*    <Link to={"/"}>Forgot password</Link>*/}
                {/*</div>*/}
                <div className={"pt-1"}>
                    <Link to={"/sign-up"}>Create account</Link>
                </div>
            </div>
        </div>
    );
    //endregion
}

export default LoginComponent;