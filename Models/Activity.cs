namespace NaiveCrm.Models;

public class Activity
{
    public static readonly string[] Types = { "Meeting", "Call", "Email", "Task" };

    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Type { get; set; } = "Meeting";
    public string Title { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string? Notes { get; set; }
    public bool Done { get; set; }
}
