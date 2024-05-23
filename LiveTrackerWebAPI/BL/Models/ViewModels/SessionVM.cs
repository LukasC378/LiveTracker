namespace BL.Models.ViewModels;

public class SessionVM : SessionBasicVM
{
    public required IList<SessionDriverVM> Drivers { get; set; }
    public required string GeoJson { get; set; }
    public bool UseTeams { get; set; }
    public int Laps { get; set; }
}

public class SessionBasicVM : BaseModel
{
    public DateTime ScheduledFrom { get; set; }
    public DateTime ScheduledTo { get; set; }
    public required string Organizer { get; set; }
    public SessionStateEnum State { get; set; }
}

public class SessionToManageVM : BaseModel
{
    public DateTime ScheduledFrom { get; set; }
    public DateTime ScheduledTo { get; set; }
}


public class SessionEmailVM : BaseModel
{
    public DateTime ScheduledFrom { get; set; }
    public DateTime ScheduledTo { get; set; }
}

public class SessionEmailWMExtended : SessionEmailVM
{
    public int OrganizerId { get; set; }
    public required string Organizer { get; set; }
}

public class SessionDriverVM : BaseModel
{
    public int Number { get; set; }
    public string Color { get; set; }
    public string? TeamName { get; set; }
    public Guid CarId { get; set; }
}

public class SessionsGroupVM
{
    public DateTime Date { get; set; }
    public IList<SessionBasicVM> Sessions { get; set; } = new List<SessionBasicVM>();

    public class SessionResultVM : BaseModel
    {
        public required IEnumerable<SessionDriverVM> Drivers { get; set; }
    }
}