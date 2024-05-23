import {Button, Form, Modal} from "react-bootstrap";
import React, {useCallback, useContext, useEffect, useRef, useState} from "react";
import LoadingComponent from "../Loading";
import {Utils} from "../../Utils/Utils";
import PasswordProgressBar from "./PasswordProgerssBar";
import validator from "validator";
import {useGoogleReCaptcha} from "react-google-recaptcha-v3";
import RegistrationService from "../../Services/RegistrationService";
import {Constants} from "../../Constants";
import ErrorMessage from "../ErrorPages/ErrorMessage";
import {useNavigate, useParams} from "react-router-dom";
import {AxiosError} from "axios";
import {UserContext} from "../../App";
import UserService from "../../Services/UserService";
import RecaptchaService from "../../Services/RecaptchaService";
import {RegistrationFirstResultEnum, RegistrationSecondResultEnum, UserRoleEnum} from "../../Models/Enums";
import {UserToRegister} from "../../Models/User";

//region constants
const actionName: string = "registration_second";
//endregion

const RegistrationSecondComponent = () => {

    const { executeRecaptcha } = useGoogleReCaptcha();
    const { registrationLink } = useParams();
    const navigate = useNavigate();
    let {setCurrentUser}: any = useContext(UserContext);

    //region useRef
    const linkExpired: React.MutableRefObject<boolean> = useRef(false);
    const userEmail: React.MutableRefObject<string> = useRef("");
    const errorMessage: React.MutableRefObject<string> = useRef("");
    //endregion

    //region useState
    const [username, setUsername]: [string, any] = useState('');
    const [password, setPassword]: [string, any] = useState('');
    const [password2, setPassword2]: [string, any] = useState('');
    const [userRole, setUserRole]: [UserRoleEnum, any] = useState(UserRoleEnum.NormalUser);
    const [errorText, setErrorText]: [string, any] = useState('');
    const [isLoading, setIsLoading]: [boolean, any] = useState(true);
    const [userVerified, setUserVerified]: [boolean, any] = useState(true);
    const [emailIsSend, setEmailIsSend]: [boolean, any] = useState(false);
    const [showErrorModal, setShowErrorModal]: [boolean, any] = useState(false)
    //endregion

    //region useEffect
    useEffect(() => {
        RegistrationService.verifyRegistrationLink(registrationLink!).then((res)=> {
            if(res.data === RegistrationFirstResultEnum.Ok){
                setIsLoading(false);
                return;
            }
            if(res.data === RegistrationFirstResultEnum.RegistrationLinkExpired){
                errorMessage.current = "This link has expired."
                linkExpired.current = true;
                setIsLoading(false);
                return;
            }
            errorMessage.current = "Unknown registration state.";
            setIsLoading(false);
        }).catch((err: AxiosError) => {
            if(err.response?.status === 404){
                navigate("/404")
            }
            else{
                errorMessage.current = Constants.unexpectedErrorMessage;
            }
            setIsLoading(false);
        })
    }, [registrationLink])

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

    //region functions
    function handleSubmit(e: any){
        e.preventDefault();
        if(validator.isEmail(username)){
            setErrorText("Username cannot be email");
            return;
        }
        if(password != password2){
            setErrorText("Passwords do not matching");
            return;
        }
        if(Utils.resultPasswordProgressBarPercentage(password) != 100){
            setErrorText("Password is too weak");
            return;
        }
        setErrorText(() => "");
        setIsLoading(() => true);
        getRecaptchaToken().then(token => {
            verifyReCaptchaAndDoAction(token).then((res) => {
                if (res) {
                    registerUser();
                } else {
                    setIsLoading(() => false);
                }
            }).catch(() => {
                errorMessage.current = Constants.unexpectedErrorMessage;
                setIsLoading(() => false);
            });
        });
    }

    function registerUser() {
        let user: UserToRegister = {
            username: username,
            password: password,
            link: registrationLink!,
            role: userRole
        }
        RegistrationService.registerUser(user).then((res) => {
            if(res.data === RegistrationSecondResultEnum.UserNameExists){
                errorMessage.current = "Username already exists"
                setShowErrorModal(() => true)
                setIsLoading(() => false);
                return;
            }
            if(res.data === RegistrationSecondResultEnum.UserAccountExists){
                errorMessage.current = "User account for this email already exists"
                setShowErrorModal(() => true)
                setIsLoading(() => false);
                return;
            }
            if(res.data === RegistrationSecondResultEnum.OrganizerAccountExists){
                errorMessage.current = "Organizer account for this email already exists"
                setShowErrorModal(() => true)
                setIsLoading(() => false);
                return;
            }
            if(res.data === RegistrationSecondResultEnum.Ok){
                console.log("Registration succeed");
                login()
                return;
            }
            errorMessage.current = "Something went wrong"
        }).catch((err: AxiosError) =>{
            console.log("Registration failed " + err.message);
            setIsLoading(() => false);
        });
    }

    function login(){
        UserService.login(username, password).then((res) => {
            console.log("Login succeed");
            setCurrentUser(res.data);
            navigate("/");
        }).catch((err: AxiosError) => {
            console.log("Login failed " + err.message);
            setIsLoading(() => false);
        })
    }

    async function verifyReCaptchaAndDoAction(token: string) {
        if (token == "") {
            errorMessage.current = Constants.unexpectedErrorMessage;
            setIsLoading(() => false);
            return false;
        }
        let res = (await RecaptchaService.verifyRecaptchaToken(token, actionName)).data
        setUserVerified(() => res)
        return res
    }

    function handleResendEmail(e: any){
        e.preventDefault();
        setIsLoading(() => true);
        getRecaptchaToken().then(token => {
            verifyReCaptchaAndDoAction(token).then((res) => {
                if (res) {
                    resendEmail();
                } else {
                    setIsLoading(() => false);
                }
            }).catch(() => {
                errorMessage.current = Constants.unexpectedErrorMessage;
                setIsLoading(() => false);
            });
        });
    }

    function resendEmail() {
        RegistrationService.resendRegistrationLink(registrationLink!).then((res) => {
            userEmail.current = res.data;
            setIsLoading(() => false);
            setEmailIsSend(() => true)
        }).catch(() => {
            errorMessage.current = Constants.unexpectedErrorMessage;
            setEmailIsSend(() => false);
            setIsLoading(() => false);
        })
    }
    //endregion

    //region return
    if(emailIsSend){
        return(
            <div className={"container-fluid d-flex justify-content-center pt-5"} style={{maxWidth: "500px"}}>
                <div className={"email-send p-4"}>
                    <p>We sent verification email to your email address <strong>{userEmail.current}</strong>.
                        Please verify your email to continue registration.</p>
                </div>
            </div>
        );
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
    if(errorMessage.current != "" && linkExpired.current){
        return(
            <div>
                <ErrorMessage message={errorMessage.current}/>
                <Button onClick={handleResendEmail}>Resend email</Button>
            </div>
        );
    }
    return(
        <div className={"container-fluid d-flex justify-content-center"} style={{maxWidth: "400px"}}>
            <div >
                <Form className={"sign-up-form"} onSubmit={handleSubmit}>
                    <Form.Group className="mb-3" controlId="username">
                        <Form.Label>Username</Form.Label>
                        <Form.Control
                            type="text"
                            autoFocus
                            required
                            value={username}
                            onChange={event => setUsername(() => event.target.value)}
                        />
                    </Form.Group>
                    <Form.Group className="mb-3" controlId="password1">
                        <Form.Label>Password</Form.Label>
                        <Form.Control
                            type="password"
                            required
                            value={password}
                            onChange={event => {
                                setPassword(() => event.target.value);
                            }}
                        />
                    </Form.Group>
                    <Form.Group className="mb-3" controlId="password2">
                        <Form.Label>Password</Form.Label>
                        <Form.Control
                            type="password"
                            required
                            value={password2}
                            onChange={event => {
                                setPassword2(() => event.target.value);
                            }}
                        />
                    </Form.Group>
                    <div className={"mb-3 note"}>
                        Your password must contain at least 8 characters, at least one uppercase character,
                        one digit and one spacial character
                    </div>
                    <div className={"mb-3 note"}>
                        <span style={{width: 590}}>
                            <PasswordProgressBar password={password}/>
                        </span>
                    </div>
                    <Form.Group className="mb-3" controlId="userRole">
                        <Form.Label>Account Type</Form.Label>
                        <Form.Select
                            id={"user-role-select"}
                            required={true}
                            value={userRole}
                            onChange={e => {
                                setUserRole(() => e.target.value)
                            }}
                        >
                            <option key={0} value={UserRoleEnum.NormalUser}>User</option>
                            <option key={1} value={UserRoleEnum.Organizer}>Organizer</option>
                        </Form.Select>
                    </Form.Group>
                    <div className={"mb-3"}>
                        <p className={"text-warning"}>{errorText}</p>
                    </div>
                    <div>
                        <Button style={{width: 200}} type={"submit"} variant={"dark"}>Sign up</Button>
                    </div>
                </Form>
            </div>
            <Modal show={showErrorModal} onHide={() => setShowErrorModal(() => false)}>
                <Modal.Header closeButton></Modal.Header>
                <Modal.Body>{errorMessage.current}</Modal.Body>
                <Modal.Footer>
                </Modal.Footer>
            </Modal>
        </div>
    );
    //endregion
}

export default RegistrationSecondComponent;