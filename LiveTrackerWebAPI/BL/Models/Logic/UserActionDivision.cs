namespace BL.Models.Logic;

public class UserActionDivision<T>
{
    public IList<T> Created { get; set; } = new List<T>();
    public IList<T> Updated { get; set; } = new List<T>();
    public IList<T> NotChanged { get; set; } = new List<T>();
    public IList<int> Deleted { get; set; } = new List<int>(); 
}