using System.Text.Json;
using Crm.Models;

namespace Crm.Services;

public class CustomerService
{
    private string path = "customers.json";

    // Read all customers from the file
    public List<Customer> GetAll()
    {
        if (!File.Exists(path))
            return new List<Customer>();

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Customer>>(json) ?? new List<Customer>();
    }

    public Customer? GetById(int id)
    {
        return GetAll().FirstOrDefault(c => c.Id == id);
    }

    // Filter by free text (name, email, company) and status
    public List<Customer> Search(string? text, string? status)
    {
        var result = GetAll();

        if (!string.IsNullOrWhiteSpace(status))
            result = result.Where(c => c.Status == status).ToList();

        if (!string.IsNullOrWhiteSpace(text))
        {
            string term = text.ToLower();
            result = result
                .Where(c => c.Name.ToLower().Contains(term)
                         || c.Email.ToLower().Contains(term)
                         || (c.Company ?? "").ToLower().Contains(term))
                .ToList();
        }

        return result.OrderBy(c => c.Name).ToList();
    }

    public void Add(Customer customer)
    {
        var all = GetAll();
        customer.Id = all.Count == 0 ? 1 : all.Max(c => c.Id) + 1; // next free Id
        customer.Created = DateTime.Now;
        all.Add(customer);
        Save(all);
    }

    public bool Update(Customer updated)
    {
        var all = GetAll();
        int index = all.FindIndex(c => c.Id == updated.Id);
        if (index == -1) return false;

        updated.Created = all[index].Created; // keep original creation date
        all[index] = updated;
        Save(all);
        return true;
    }

    public bool Delete(int id)
    {
        var all = GetAll();
        if (all.RemoveAll(c => c.Id == id) == 0) return false;

        Save(all);
        return true;
    }

    // Write the whole list back to the file
    private void Save(List<Customer> all)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, JsonSerializer.Serialize(all, options));
    }
}
