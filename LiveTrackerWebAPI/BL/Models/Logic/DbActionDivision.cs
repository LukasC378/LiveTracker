namespace BL.Models.Logic;

public class DbActionDivision<T>
{
    public IList<T> ToInsert { get; set; } = new List<T>();
    public IList<T> ToUpdate { get; set; } = new List<T>();
    public IList<T> ToDelete { get; set; } = new List<T>();
}