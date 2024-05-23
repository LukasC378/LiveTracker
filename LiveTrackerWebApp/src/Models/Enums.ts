export enum RegistrationFirstResultEnum {
    Ok,
    EmailExists,
    RegistrationLinkExists,
    RegistrationLinkExpired
}

export enum RegistrationSecondResultEnum {
    Ok,
    UserNameExists,
    UserAccountExists,
    OrganizerAccountExists,
}

export enum UserRoleEnum
{
    NormalUser,
    Organizer
}

export enum OrganizerTypeEnum{
    All,
    Subscribed,
    Others
}

export enum SessionStateEnum {
    Preview,
    Live,
    Archived
}

export enum SessionStateFilterEnum {
    Upcoming,
    Live,
    Archived,
    All
}

export enum HttpMethod {
    Get,
    Post,
    Put,
    Delete
}