import axios, {AxiosResponse} from "axios";
import {HttpMethod} from "../Models/Enums";

const SERVER_URL: string = import.meta.env.VITE_SERVER_URL;
export class apiClient{

    public static async get(url: string, data: any = null): Promise<AxiosResponse<any, any>> {
        if(data != null){
            data = {
                params: data
            }
        }
        return await this.request(url, HttpMethod.Get, data);
    }
    public static async post(url: string, data: any = null){
        return await this.request(url, HttpMethod.Post, data);
    }
    public static async put(url: string, data: any = null){
        return await this.request(url, HttpMethod.Put, data);
    }
    public static async delete(url: string, data: any = null){
        return await this.request(url, HttpMethod.Delete, data);
    }

    private static async request(url: string, method: HttpMethod, data: any = null): Promise<AxiosResponse<any, any>> {
        const requestUrl: string = SERVER_URL + url;
        try{
            return await this.apiCall(requestUrl, method, data)
        }
        catch (error: any){
            if(!error?.response?.status){
                window.location.replace('/error');
                return Promise.reject(error);
            }
            if (error.response.status === 401 && sessionStorage.getItem('user')) {
                try {
                    await axios.get(SERVER_URL+'/user/refresh');
                    // Retry the original request with the new access token
                    return await this.apiCall(requestUrl, method, data);
                } catch (refreshError) {
                    sessionStorage.removeItem('user');
                    window.location.replace('/login');
                    return Promise.reject(error);
                }
            }
            else if(error.response.status === 401){
                window.location.replace('/login');
            }
            else if(error.response.status === 403){
                window.location.replace('/forbidden');
            }
            else if(error.response.status === 404){
                window.location.replace('/404');
            }
            else{
                window.location.replace('/error');
            }
            return Promise.reject(error);
        }
    }

    private static async apiCall(url: string, method: HttpMethod, data: any = null): Promise<AxiosResponse<any, any>> {
        switch (method){
            case HttpMethod.Get:
                return await axios.get(url, data);
            case HttpMethod.Post:
                return await axios.post(url, data);
            case HttpMethod.Put:
                return await axios.put(url, data);
            case HttpMethod.Delete:
                return await axios.delete(url, data);
        }
    }
}

export default apiClient;