import {AxiosResponse} from "axios";
import {
    Session,
    SessionFilter,
    SessionGroupVM, SessionResultVM,
    SessionToEdit,
    SessionToManageVM,
    SessionVM
} from "../Models/Session";
import apiClient from "../Utils/ApiClient";
import {SessionStateEnum} from "../Models/Enums";

class SessionService {

    public static getSession(sessionId: number): Promise<AxiosResponse<SessionVM>>{
        return apiClient.get("/sessions/" + sessionId);
        //return axios.get(this.SERVER_URL+"/sessions/" + sessionId)
    }

    public static getSessionToEdit(sessionId: number): Promise<AxiosResponse<SessionToEdit>>{
        return apiClient.get("/sessions/edit/" + sessionId);
    }

    public static getSessionsToManage(): Promise<AxiosResponse<SessionToManageVM[]>>{
        return apiClient.get("/sessions/manage");
    }

    public static getSessions(filter: SessionFilter): Promise<AxiosResponse<SessionGroupVM[]>>{
        return apiClient.post("/sessions/filter", filter);
        //return axios.post(this.SERVER_URL+"/sessions/filter", filter)
    }

    public static getLiveSessions(): Promise<AxiosResponse<SessionGroupVM[]>> {
        return apiClient.get("/sessions/live");
        //return axios.get(this.SERVER_URL+"/sessions/live")
    }

    public static getSessionState(sessionId: number): Promise<AxiosResponse<SessionStateEnum>> {
        return apiClient.get("/sessions/state/" + sessionId);
    }

    public static getSessionResult(sessionId: number): Promise<AxiosResponse<SessionResultVM>>{
        return apiClient.get("/sessions/result/" + sessionId)
    }

    public static createSession(session: Session): Promise<AxiosResponse>{
        return apiClient.post("/sessions", session);
        //return axios.post(this.SERVER_URL+"/sessions", session)
    }

    public static updateSession(session: Session): Promise<AxiosResponse>{
        return apiClient.put("/sessions", session);
    }

    public static cancelSession(sessionId: number): Promise<AxiosResponse>{
        return apiClient.delete("sessions/cancel/" + sessionId)
    }
}

export default SessionService;