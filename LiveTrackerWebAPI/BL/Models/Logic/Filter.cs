namespace BL.Models.Logic;

public class Filter
{
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
    public string SearchTerm { get; set; } = string.Empty;
}

public class SessionFilter : Filter
{
    public DateTime? Date { get; set; }
    public int? OrganizerId { get; set; }
    public SessionStateFilterEnum SessionState { get; set; }
    public bool OrderAsc { get; set; } = true;
}

public class OrganizersFilter : Filter
{
    public OrganizerTypeEnum Type { get; set;}
}