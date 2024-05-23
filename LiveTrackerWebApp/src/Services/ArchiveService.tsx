import {AxiosResponse} from "axios";
import apiClient from "../Utils/ApiClient";
import {RaceData} from "../Models/Race";
import {ArchiveInfo} from "../Models/Archive";

class ArchiveService {
    static getTotalCount(sessionId: number): Promise<AxiosResponse<ArchiveInfo>> {
        return apiClient.get("/archive/info/" + sessionId)
    }

    static getChunk(sessionId: number, start: number, length: number): Promise<AxiosResponse<RaceData[]>> {
        let data = {
            sessionId: sessionId,
            start: start,
            length: length
        }
        return apiClient.get("/archive", data)
    }
}

export default ArchiveService