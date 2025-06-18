using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class CompanyClient : Client
{
    [MaxLength(100)]
    public string CompanyName { get; set; }
    
    [MaxLength(10)]
    public string KRSNumber { get; set; }
}