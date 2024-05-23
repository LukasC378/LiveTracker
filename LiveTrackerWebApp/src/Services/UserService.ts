import axios, {AxiosResponse} from "axios";
import {OrganizersFilter, OrganizerBasicVM, User, OrganizerVM} from "../Models/User";
import apiClient from "../Utils/ApiClient";

class UserService {

    private static SERVER_URL: string = import.meta.env.VITE_SERVER_URL;

    static getCurrentUser(): Promise<AxiosResponse<User|null>>{
        axios.defaults.withCredentials = true;
        return apiClient.get("/user");
        //return axios.get(this.SERVER_URL+"/user");
    }

    static getCurrentUserWithAuthorization(): Promise<AxiosResponse<User|null>>{
    axios.defaults.withCredentials = true;
    //return apiClient.get("/user/authorize");
    return axios.get(this.SERVER_URL+"/user/authorize");
}

    static login(username: string, password: string): Promise<AxiosResponse<User>>{
        const params = new URLSearchParams();
        params.append('username', username);
        params.append('password', password);

        axios.defaults.withCredentials = true;
        //return apiClient.post("/user/login", params)
        return axios.post(this.SERVER_URL+"/user/login", params);
    }

    static logout() {
        axios.defaults.withCredentials = true;
        return apiClient.delete("/user/logout");
        //return axios.delete(this.SERVER_URL+"/user/logout");
    }

    public static getOrganizersForSearch(searchTerm: string): Promise<AxiosResponse<OrganizerBasicVM[]>>{
        const params = new URLSearchParams();
        params.append('searchTerm', searchTerm);

        return apiClient.get("/user/organizersForSearch?searchTerm="+searchTerm);
        //return axios.get(this.SERVER_URL+"/sessions/organizers?searchTerm="+searchTerm)
    }

    public static getOrganizers(filter: OrganizersFilter): Promise<AxiosResponse<OrganizerVM[]>>{
        return apiClient.get("/user/organizers", filter);
        //return axios.get(this.SERVER_URL+"/sessions/organizers?searchTerm="+searchTerm)
    }

    public static subscribe(organizerId: number){
        const params = new URLSearchParams();
        params.append('organizerId', organizerId.toString());

        return apiClient.post("/subscribe/subscribe", params);
    }

    public static unsubscribe(organizerId: number){
        let data = {
            params: {
                organizerId: organizerId
            }
        }
        return apiClient.delete("/subscribe/unsubscribe", data);
    }

}

export default UserService;