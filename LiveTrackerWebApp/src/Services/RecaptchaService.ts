import {AxiosResponse} from "axios";
import apiClient from "../Utils/ApiClient";

class RecaptchaService {
    static verifyRecaptchaToken(token: string, action: string): Promise<AxiosResponse<boolean>>{
        return apiClient.get("/recaptcha?token="+token+"&actionType="+action);
        //return axios.get(this.SERVER_URL+"/recaptcha?token="+token+"&actionType="+action);
    }
}

export default RecaptchaService