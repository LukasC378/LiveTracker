import axios, {AxiosResponse} from "axios";
import {Layout} from "../Models/Layout";
import apiClient from "../Utils/ApiClient";

class LayoutService{

    static getLayouts(): Promise<AxiosResponse<Layout[]>>{
        return apiClient.get("/layouts");
    }

    static getLayout(layoutId: number): Promise<AxiosResponse<Layout>>{
        return apiClient.get("/layouts/" + layoutId);
    }

    static createLayout(layout: Layout): Promise<AxiosResponse<number>>{
        return apiClient.post("/layouts", layout);
    }

    static updateLayout(layout: Layout): Promise<AxiosResponse>{
        return apiClient.put("/layouts", layout);
    }

    static renameLayout(layoutId: number, layoutName: string): Promise<AxiosResponse>{
        const params = new URLSearchParams();
        params.append('layoutId', layoutId.toString());
        params.append('layoutName', layoutName);

        axios.defaults.withCredentials = true;
        return apiClient.put("/layouts/rename", params);
    }

    static deleteLayout(layoutId: number): Promise<AxiosResponse>{
        return apiClient.delete("/layouts/" + layoutId);
    }

}

export default LayoutService;