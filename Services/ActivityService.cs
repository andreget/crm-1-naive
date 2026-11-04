using System.Text.Json;
using NaiveCrm.Models;

namespace NaiveCrm.Services;

public class ActivityService
{
    private string path = "activities.json";

    public List<Activity> GetAll()
    {
        if (!File.Exists(path))
            return new List<Activity>();

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Activity>>(json) ?? new List<Activity>();
    }

    // Newest first
    public List<Activity> GetByCustomer(int customerId)
    {
        return GetAll()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    // Not done, soonest first
    public List<Activity> GetOpen()
    {
        return GetAll().Where(a => !a.Done).OrderBy(a => a.Date).ToList();
    }

    public void Add(Activity activity)
    {
        var all = GetAll();
        activity.Id = all.Count == 0 ? 1 : all.Max(a => a.Id) + 1;
        all.Add(activity);
        Save(all);
    }

    public bool SetDone(int id, bool done)
    {
        var all = GetAll();
        var activity = all.FirstOrDefault(a => a.Id == id);
        if (activity == null) return false;

        activity.Done = done;
        Save(all);
        return true;
    }

    public bool Delete(int id)
    {
        var all = GetAll();
        if (all.RemoveAll(a => a.Id == id) == 0) return false;

        Save(all);
        return true;
    }

    // Used when a customer is deleted
    public void DeleteByCustomer(int customerId)
    {
        var all = GetAll();
        all.RemoveAll(a => a.CustomerId == customerId);
        Save(all);
    }

    private void Save(List<Activity> all)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, JsonSerializer.Serialize(all, options));
    }
}
