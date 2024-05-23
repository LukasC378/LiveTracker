namespace BL;

public enum OrganizerTypeEnum
{
    All,
    Subscribed,
    Others
}

public enum RegistrationFirstResultEnum
{
    Ok,
    EmailExists,
    RegistrationLinkExists,
    RegistrationLinkExpired
}

public enum RegistrationSecondResultEnum
{
    Ok,
    UserNameExists,
    UserAccountExists,
    OrganizerAccountExists
}

public enum SessionStateEnum
{
    Preview,
    Live,
    Archived
}

public enum SessionStateFilterEnum
{
    Upcoming,
    Live,
    Archived,
    All
}