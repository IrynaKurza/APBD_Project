using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models;

[Table("EmployeeRole")]
public class EmployeeRole
{
    [Key]
    public int Id { get; set; }

    [MaxLength(30)] 
    public string Name { get; set; }

    public virtual ICollection<Employee> Employees { get; set; }
}