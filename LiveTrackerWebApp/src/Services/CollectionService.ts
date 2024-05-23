import axios, {AxiosResponse} from "axios";
import {Collection, CollectionBasicVM} from "../Models/Collection";
import apiClient from "../Utils/ApiClient";

class CollectionService {
    static getCollections(): Promise<AxiosResponse<CollectionBasicVM[]>>{
        return apiClient.get("/collections");
        //return axios.get(this.SERVER_URL+"/collections")
    }

    static getCollection(collectionId: number): Promise<AxiosResponse<Collection>>{
        return apiClient.get("/collections/" + collectionId);
        //return axios.get(this.SERVER_URL+"/collections/" + collectionId)
    }

    static createCollection(collection: Collection): Promise<AxiosResponse>{
        axios.defaults.withCredentials = true;
        return apiClient.post("/collections", collection);
        //return axios.post(this.SERVER_URL+"/collections", collection)
    }

    static updateCollection(collection: Collection): Promise<AxiosResponse>{
        axios.defaults.withCredentials = true;
        return apiClient.put("/collections", collection);
        //return axios.put(this.SERVER_URL+"/collections", collection)
    }

    static renameCollection(collectionId: number, collectionName: string): Promise<AxiosResponse>{
        const params = new URLSearchParams();
        params.append('collectionId', collectionId.toString());
        params.append('collectionName', collectionName);

        axios.defaults.withCredentials = true;
        return apiClient.put("/collections/rename", params);
    }

    static deleteCollection(collectionId: number): Promise<AxiosResponse>{
        axios.defaults.withCredentials = true;
        return apiClient.delete("/collections", collectionId);
    }
}

export default CollectionService;