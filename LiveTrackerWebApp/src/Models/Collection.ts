import {Driver} from "./Driver";
import {Team} from "./Team";

export interface Collection{
    id: number,
    name: string,
    drivers: Driver[]
    teams: Team[]
    useTeams: boolean
}

export interface CollectionBasicVM {
    id: number,
    name: string
}