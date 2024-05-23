import {OrganizerTypeEnum, UserRoleEnum} from "./Enums";
import {Filter, BaseModel} from "./Base";

export interface User{
    username: string,
    email: string,
    role: UserRoleEnum
}

export interface UserToRegister{
    username: string,
    password: string,
    link: string,
    role: UserRoleEnum
}

export interface OrganizersFilter extends Filter{
    type: OrganizerTypeEnum,
    searchTerm: string
}

export interface OrganizerBasicVM extends BaseModel{
}

export interface OrganizerVM extends OrganizerBasicVM{
    subscribed: boolean
}