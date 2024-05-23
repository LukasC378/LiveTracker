using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(DriverId)), Index(nameof(CollectionId))]
public class DriverCollection
{
    [Key]
    public int Id { get; set; }
    public int DriverId { get; set; }
    public int CollectionId { get; set; }
}