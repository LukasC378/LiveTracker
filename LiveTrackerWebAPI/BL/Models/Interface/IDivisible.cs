namespace BL.Models.Interface;

public interface IDivisible<T> 
{
    int Id { get; set; }
    T MapToEntity();
    void UpdateEntity(T obj);
    bool IsEqualToEntity(T obj);
}