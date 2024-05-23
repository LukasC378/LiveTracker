import {AxiosResponse} from "axios";
import {UserToRegister} from "../Models/User";
import {RegistrationFirstResultEnum, RegistrationSecondResultEnum} from "../Models/Enums";
import apiClient from "../Utils/ApiClient";

class RegistrationService {

    static sendRegistrationLink(userEmail: string): Promise<AxiosResponse<RegistrationFirstResultEnum>>{
        return apiClient.post("/register/"+userEmail);
        //return axios.post(this.SERVER_URL+"/register/"+userEmail);
    }

    static resendRegistrationLink(link: string): Promise<AxiosResponse<string>>{
        return apiClient.put("/register/"+link);
        //return axios.put(this.SERVER_URL+"/register/"+link);
    }

    static registerUser(userToRegister: UserToRegister): Promise<AxiosResponse<RegistrationSecondResultEnum>>{
        return apiClient.post("/register", userToRegister);
        //return apiClient.post(this.SERVER_URL+"/register", userToRegister);
    }

    static verifyRegistrationLink(registrationLink: string): Promise<AxiosResponse<RegistrationFirstResultEnum>>{
        return apiClient.get("/register/"+registrationLink);
        //return axios.get(this.SERVER_URL+"/register/"+registrationLink);
    }
}

export default RegistrationService;