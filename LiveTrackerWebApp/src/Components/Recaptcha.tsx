import React from "react";
import {GoogleReCaptchaProvider} from "react-google-recaptcha-v3";

const RECAPTCHA_SITE_KEY: string = import.meta.env.VITE_RECAPTCHA_SITE_KEY;

type RecaptchaComponentProps = {
    component: React.ComponentType<any>;
};

const RecaptchaComponent: React.FC<RecaptchaComponentProps> = ({ component: Component }) =>{
    return(
        <GoogleReCaptchaProvider reCaptchaKey={RECAPTCHA_SITE_KEY}>
            <Component />
        </GoogleReCaptchaProvider>
    )
}

export default RecaptchaComponent;