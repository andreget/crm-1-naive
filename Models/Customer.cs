namespace Crm.Models;

public class Customer
{
    public static readonly string[] Statuses = { "Lead", "Active", "Inactive" };

    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Lead";
    public DateTime Created { get; set; } = DateTime.Now;
}
