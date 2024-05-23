import {BaseModel, Filter} from "./Base";
import {SessionStateEnum, SessionStateFilterEnum} from "./Enums";

export interface Session {
    id: number,
    name: string,
    collectionId: number,
    geoJson: string,
    layoutId?: number,
    laps: number,
    scheduledFrom: string,
    scheduledTo: string
}

export interface SessionToEdit extends Session {
    collectionName: string,
    layoutName: string
}

export interface SessionFilter extends Filter, SessionFilerUI {
}

export interface SessionFilerUI {
    date: string,
    organizerId: number,
    searchTerm: string,
    sessionState: SessionStateFilterEnum,
    orderAsc: boolean
}

export interface SessionVM extends SessionBasicVM {
    drivers: SessionDriverVM[],
    geoJson: string,
    useTeams: boolean,
    laps: number
}

export interface SessionBasicVM extends BaseModel {
    scheduledFrom: string,
    scheduledTo: string,
    organizer: string,
    state: SessionStateEnum
}

export interface SessionToManageVM extends BaseModel {
    scheduledFrom: string,
    scheduledTo: string
}

export interface SessionGroupVM{
    date: string,
    sessions: SessionBasicVM[]
}

export interface SessionDriverVM{
    name: string,
    number: number,
    color: string,
    teamName?: string
    carId: string
}

export interface SessionResultVM extends BaseModel{
    drivers: SessionDriverVM[]
}