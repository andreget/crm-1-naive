using System.Net;
using System.Text.RegularExpressions;
using NaiveCrm.Models;
using NaiveCrm.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles(); // serves wwwroot (css and js)

// ---------- List ----------

app.MapGet("/", (string? search, string? status) =>
{
    var service = new CustomerService();
    var customers = service.Search(search, status);

    string rows = "";
    foreach (var c in customers)
    {
        rows += $@"
            <tr>
                <td><a href=""/customers/{c.Id}"">{Encode(c.Name)}</a></td>
                <td>{Encode(c.Company)}</td>
                <td>{Encode(c.Email)}</td>
                <td>{Encode(c.Phone)}</td>
                <td>{Encode(c.Status)}</td>
                <td class=""actions"">
                    <a class=""button small"" href=""/customers/{c.Id}/edit"">Edit</a>
                    <a class=""button small danger"" href=""/customers/{c.Id}/delete"">Delete</a>
                </td>
            </tr>";
    }

    string html = Render("index.html", new Dictionary<string, string>
    {
        ["search"] = Encode(search),
        ["statusOptions"] = StatusOptions(status, true),
        ["rows"] = rows,
        ["empty"] = customers.Count == 0 ? "<p class=\"muted\">No customers found.</p>" : ""
    });

    return Results.Content(html, "text/html");
});

// ---------- Create ----------

app.MapGet("/customers/new", () =>
{
    return Results.Content(FormPage(new Customer(), "New customer", "/customers/new", ""), "text/html");
});

app.MapPost("/customers/new", async (HttpRequest request) =>
{
    var customer = ReadCustomer(await request.ReadFormAsync());

    string error = Validate(customer);
    if (error != "")
        return Results.Content(FormPage(customer, "New customer", "/customers/new", error), "text/html");

    new CustomerService().Add(customer);
    return Results.Redirect("/");
});

// ---------- Customer details ----------

app.MapGet("/customers/{id:int}", (int id) =>
{
    var customer = new CustomerService().GetById(id);
    if (customer == null)
        return Results.NotFound();

    var activities = new ActivityService().GetByCustomer(id);

    string rows = "";
    foreach (var a in activities)
        rows += ActivityRow(a, null, $"/customers/{id}");

    string html = Render("customer.html", new Dictionary<string, string>
    {
        ["id"] = id.ToString(),
        ["name"] = Encode(customer.Name),
        ["company"] = Encode(customer.Company),
        ["email"] = Encode(customer.Email),
        ["phone"] = Encode(customer.Phone),
        ["status"] = Encode(customer.Status),
        ["notes"] = Encode(customer.Notes),
        ["typeOptions"] = TypeOptions(),
        ["defaultDate"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm"),
        ["rows"] = rows,
        ["empty"] = activities.Count == 0 ? "<p class=\"muted\">No activities yet.</p>" : ""
    });

    return Results.Content(html, "text/html");
});

// ---------- Edit ----------

app.MapGet("/customers/{id:int}/edit", (int id) =>
{
    var customer = new CustomerService().GetById(id);
    if (customer == null)
        return Results.NotFound();

    return Results.Content(FormPage(customer, "Edit customer", $"/customers/{id}/edit", ""), "text/html");
});

app.MapPost("/customers/{id:int}/edit", async (int id, HttpRequest request) =>
{
    var customer = ReadCustomer(await request.ReadFormAsync());
    customer.Id = id;

    string error = Validate(customer);
    if (error != "")
        return Results.Content(FormPage(customer, "Edit customer", $"/customers/{id}/edit", error), "text/html");

    if (!new CustomerService().Update(customer))
        return Results.NotFound();

    return Results.Redirect("/");
});

// ---------- Delete ----------

app.MapGet("/customers/{id:int}/delete", (int id) =>
{
    var customer = new CustomerService().GetById(id);
    if (customer == null)
        return Results.NotFound();

    string html = Render("delete.html", new Dictionary<string, string>
    {
        ["name"] = Encode(customer.Name),
        ["company"] = Encode(customer.Company),
        ["action"] = $"/customers/{id}/delete"
    });

    return Results.Content(html, "text/html");
});

app.MapPost("/customers/{id:int}/delete", (int id) =>
{
    new CustomerService().Delete(id);
    new ActivityService().DeleteByCustomer(id); // remove the customer's activities too
    return Results.Redirect("/");
});

// ---------- Activities ----------

// All open activities across customers
app.MapGet("/activities", () =>
{
    var customers = new CustomerService().GetAll();
    var activities = new ActivityService().GetOpen();

    string rows = "";
    foreach (var a in activities)
    {
        var customer = customers.FirstOrDefault(c => c.Id == a.CustomerId);
        rows += ActivityRow(a, customer, "/activities");
    }

    string html = Render("activities.html", new Dictionary<string, string>
    {
        ["rows"] = rows,
        ["empty"] = activities.Count == 0 ? "<p class=\"muted\">No open activities.</p>" : ""
    });

    return Results.Content(html, "text/html");
});

app.MapPost("/customers/{id:int}/activities", async (int id, HttpRequest request) =>
{
    if (new CustomerService().GetById(id) == null)
        return Results.NotFound();

    var form = await request.ReadFormAsync();
    string type = form["type"].ToString();
    string title = form["title"].ToString().Trim();

    if (title != "")
    {
        if (!DateTime.TryParse(form["date"].ToString(), out var date))
            date = DateTime.Now;

        new ActivityService().Add(new Activity
        {
            CustomerId = id,
            Type = Activity.Types.Contains(type) ? type : "Task",
            Title = title,
            Date = date,
            Notes = form["notes"].ToString().Trim()
        });
    }

    return Results.Redirect($"/customers/{id}");
});

app.MapPost("/activities/{id:int}/done", async (int id, HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    new ActivityService().SetDone(id, form["done"] == "true");
    return Results.Redirect(SafeBack(form["back"].ToString()));
});

app.MapPost("/activities/{id:int}/delete", async (int id, HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    new ActivityService().Delete(id);
    return Results.Redirect(SafeBack(form["back"].ToString()));
});

app.Run();

// ---------- Helpers ----------

// Fill {{placeholders}} in an html template
static string Render(string file, Dictionary<string, string> values)
{
    string html = File.ReadAllText($"Templates/{file}");
    return Regex.Replace(html, @"\{\{(\w+)\}\}",
        m => values.TryGetValue(m.Groups[1].Value, out var value) ? value : "");
}

static string Encode(string? text)
{
    return WebUtility.HtmlEncode(text ?? "");
}

static string StatusOptions(string? selected, bool includeAll)
{
    string options = includeAll ? "<option value=\"\">All statuses</option>" : "";
    foreach (var status in Customer.Statuses)
    {
        string isSelected = status == selected ? " selected" : "";
        options += $"<option value=\"{status}\"{isSelected}>{status}</option>";
    }
    return options;
}

static string TypeOptions()
{
    string options = "";
    foreach (var type in Activity.Types)
        options += $"<option value=\"{type}\">{type}</option>";
    return options;
}

static string FormPage(Customer c, string title, string action, string error)
{
    return Render("form.html", new Dictionary<string, string>
    {
        ["title"] = title,
        ["action"] = action,
        ["error"] = error == "" ? "" : $"<p class=\"error\">{Encode(error)}</p>",
        ["name"] = Encode(c.Name),
        ["email"] = Encode(c.Email),
        ["phone"] = Encode(c.Phone),
        ["company"] = Encode(c.Company),
        ["notes"] = Encode(c.Notes),
        ["statusOptions"] = StatusOptions(c.Status, false)
    });
}

static Customer ReadCustomer(IFormCollection form)
{
    string status = form["status"].ToString();

    return new Customer
    {
        Name = form["name"].ToString().Trim(),
        Email = form["email"].ToString().Trim(),
        Phone = form["phone"].ToString().Trim(),
        Company = form["company"].ToString().Trim(),
        Notes = form["notes"].ToString().Trim(),
        Status = Customer.Statuses.Contains(status) ? status : "Lead"
    };
}

static string Validate(Customer c)
{
    if (c.Name == "") return "Name is required.";
    if (!c.Email.Contains('@')) return "A valid email is required.";
    return "";
}

// One table row; the customer column is only shown when a customer is passed in
static string ActivityRow(Activity a, Customer? customer, string back)
{
    string customerCell = customer == null
        ? ""
        : $"<td><a href=\"/customers/{customer.Id}\">{Encode(customer.Name)}</a></td>";

    string rowClass = a.Done ? "done" : a.Date < DateTime.Now ? "overdue" : "";
    string doneValue = a.Done ? "false" : "true";
    string doneLabel = a.Done ? "Reopen" : "Done";

    return $@"
        <tr class=""{rowClass}"">
            <td>{a.Date.ToString("yyyy-MM-dd HH:mm")}</td>
            <td>{Encode(a.Type)}</td>
            <td>{Encode(a.Title)}</td>
            {customerCell}
            <td class=""actions"">
                <form method=""post"" action=""/activities/{a.Id}/done"">
                    <input type=""hidden"" name=""back"" value=""{back}"">
                    <input type=""hidden"" name=""done"" value=""{doneValue}"">
                    <button class=""small"">{doneLabel}</button>
                </form>
                <form method=""post"" action=""/activities/{a.Id}/delete"">
                    <input type=""hidden"" name=""back"" value=""{back}"">
                    <button class=""small danger"">Delete</button>
                </form>
            </td>
        </tr>";
}

// Only allow redirects to local paths
static string SafeBack(string? back)
{
    if (string.IsNullOrEmpty(back) || !back.StartsWith("/") || back.StartsWith("//"))
        return "/";

    return back;
}