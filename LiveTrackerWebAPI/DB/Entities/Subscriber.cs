using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(UserId)), Index(nameof(OrganizerId))]
public class Subscriber
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganizerId { get; set; }
}