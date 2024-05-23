import {Button, Form, Modal} from "react-bootstrap";
import React, {useCallback, useRef, useState} from "react";
import validator from "validator";
import "./Registration.css";
import RegistrationService from "../../Services/RegistrationService";
import {useGoogleReCaptcha} from "react-google-recaptcha-v3";
import ErrorMessage from "../ErrorPages/ErrorMessage";
import LoadingComponent from "../Loading";
import {Constants} from "../../Constants";
import RecaptchaService from "../../Services/RecaptchaService";
import {RegistrationFirstResultEnum} from "../../Models/Enums";

//region constants

const actionName: string = "registration_first";

//endregion

const RegistrationFirstComponent = () => {

    const { executeRecaptcha } = useGoogleReCaptcha();

    const errorModalText: React.MutableRefObject<string> = useRef("");

    //region useState
    const [userEmail, setUserEmail]: [string, any] = useState('');
    const [errorMessage, setErrorMessage]: [string, any] = useState('');
    const [emailIsSend, setEmailIsSend]: [boolean, any] = useState(false);
    const [userVerified, setUserVerified]: [boolean, any] = useState(true);
    const [isLoading, setIsLoading]: [boolean, any] = useState(false);
    const [showErrorModal, setShowErrorModal]: [boolean, any] = useState(false);
    //endregion

    const getRecaptchaToken = useCallback(async (): Promise<string> => {
        if (!executeRecaptcha) {
            console.log('Execute recaptcha not yet available');
            return "";
        }
        return await executeRecaptcha(actionName);
    }, [executeRecaptcha]);


    //region functions
    function sendRegistrationLink() {
        RegistrationService.sendRegistrationLink(userEmail).then((res) => {
            setIsLoading(() => false);
            if(res.data === RegistrationFirstResultEnum.Ok){
                setEmailIsSend(() => true);
                return;
            }
            if(res.data === RegistrationFirstResultEnum.RegistrationLinkExists){
                errorModalText.current = "Valid registration link has been already sent on your email."
                setShowErrorModal(() => true)
                return;
            }
            if(res.data === RegistrationFirstResultEnum.EmailExists){
                errorModalText.current = "Account for this email already exists."
                setShowErrorModal(() => true)
                return;
            }
            errorModalText.current = "Unknown registration state."
            setShowErrorModal(() => true)
        }).catch(() => {
            setErrorMessage(() => Constants.unexpectedErrorMessage);
            setEmailIsSend(() => false);
            setIsLoading(() => false);
        })
    }

    async function verifyReCaptchaToken(token: string): Promise<boolean>{
        if(token == ""){
            setErrorMessage(() => Constants.unexpectedErrorMessage);
            setIsLoading(() => false);
            return false;
        }
        return (await RecaptchaService.verifyRecaptchaToken(token, actionName)).data;
    }

    function handleSubmit(e: any) {
        e.preventDefault()
        setIsLoading(() => true);
        if(!validator.isEmail(userEmail) || userEmail == ''){
            setErrorMessage(() => "Email in not valid");
            setIsLoading(() => false);
            return;
        }
        else{
            setErrorMessage(() => "");
            getRecaptchaToken().then(token => {
                verifyReCaptchaToken(token).then((res) => {
                    if(res){
                        setUserVerified(() => true);
                        sendRegistrationLink()
                    }
                    else{
                        setIsLoading(() => false);
                        setUserVerified(() => false);
                    }
                }).catch(() => {
                    setErrorMessage(() => Constants.unexpectedErrorMessage);
                    setIsLoading(() => false);
                });
            });
        }
    }
    //endregion

    //region return
    if(!userVerified){
        return(
            <ErrorMessage message={"You are not verified user"}/>
        );
    }
    if(isLoading){
        return(
            <LoadingComponent/>
        );
    }

    function handleCloseErrorModal() {
        setShowErrorModal(() => false);
    }

    if(emailIsSend){
        return(
            <div className={"container-fluid d-flex justify-content-center pt-5"} style={{maxWidth: "500px"}}>
                <div className={"email-send p-4"}>
                    <p>We sent verification email to your email address <strong>{userEmail}</strong>.
                        Please verify your email to continue registration.</p>
                </div>
            </div>
        );
    }
    else{
        return(
            <div className={"container-fluid d-flex justify-content-center"} style={{maxWidth: "400px"}}>
                <div >
                    <Form className={"sign-up-form"} onSubmit={handleSubmit}>
                        <Form.Group className="mb-3" controlId="email">
                            <Form.Label>Enter your email</Form.Label>
                            <Form.Control
                                type="email"
                                autoFocus
                                required
                                value={userEmail}
                                onChange={event => setUserEmail(() => event.target.value.toLowerCase())}
                            />
                        </Form.Group>
                        <div>
                            <Button style={{width: 200}} type={"submit"} variant={"dark"}>Send email</Button>
                        </div>
                    </Form>
                    <div>
                        <p className={"mt-2 text-warning"}>{errorMessage}</p>
                    </div>
                </div>

                <Modal show={showErrorModal} onHide={handleCloseErrorModal}>
                    <Modal.Header closeButton></Modal.Header>
                    <Modal.Body>{errorModalText.current}</Modal.Body>
                    <Modal.Footer>
                        <Button variant="primary" onClick={handleCloseErrorModal}>
                            OK
                        </Button>
                    </Modal.Footer>
                </Modal>
            </div>
        );
    }
    //endregion
}

export default RegistrationFirstComponent;