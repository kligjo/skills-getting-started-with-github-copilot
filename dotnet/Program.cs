using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ActivityStore>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/activities", (ActivityStore activityStore) => Results.Ok(activityStore.GetActivities()));

app.MapGet("/activities/{activityName}/participants", (string activityName, ActivityStore activityStore) =>
{
    var participants = activityStore.GetParticipants(activityName);
    return participants is null
        ? Results.NotFound(new { detail = "Activity not found" })
        : Results.Ok(participants);
});

app.MapPost("/activities/{activityName}/signup", (string activityName, string email, ActivityStore activityStore) =>
{
    var result = activityStore.SignUp(activityName, email);

    return result switch
    {
        SignUpResult.ActivityNotFound => Results.NotFound(new { detail = "Activity not found" }),
        SignUpResult.AlreadyRegistered => Results.Conflict(new { detail = "Student is already signed up for this activity" }),
        _ => Results.Ok(new { message = $"Signed up {email} for {activityName}" })
    };
});

app.Run();

sealed class ActivityStore
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, Activity> activities = new()
    {
        ["Chess Club"] = new(
            "Learn strategies and compete in chess tournaments",
            "Fridays, 3:30 PM - 5:00 PM",
            12,
            "Academic",
            ["michael@mergington.edu", "daniel@mergington.edu"]),
        ["Programming Class"] = new(
            "Learn programming fundamentals and build software projects",
            "Tuesdays and Thursdays, 3:30 PM - 4:30 PM",
            20,
            "Academic",
            ["emma@mergington.edu", "sophia@mergington.edu"]),
        ["Gym Class"] = new(
            "Physical education and sports activities",
            "Mondays, Wednesdays, Fridays, 2:00 PM - 3:00 PM",
            30,
            "Physical Education",
            ["john@mergington.edu", "olivia@mergington.edu"]),
        ["Art Club"] = new(
            "Explore various art forms and participate in art exhibitions",
            "Wednesdays, 3:30 PM - 5:00 PM",
            15,
            "Arts",
            ["lucas@mergington.edu", "mia@mergington.edu"]),
        ["Music Club"] = new(
            "Practice instruments and perform in school concerts",
            "Thursdays, 3:30 PM - 5:00 PM",
            15,
            "Arts",
            [])
    };

    public Dictionary<string, Activity> GetActivities()
    {
        lock (syncRoot)
        {
            return activities.ToDictionary(entry => entry.Key, entry => entry.Value.Copy());
        }
    }

    public List<string>? GetParticipants(string activityName)
    {
        lock (syncRoot)
        {
            return activities.TryGetValue(activityName, out var activity)
                ? [.. activity.Participants]
                : null;
        }
    }

    public SignUpResult SignUp(string activityName, string email)
    {
        lock (syncRoot)
        {
            if (!activities.TryGetValue(activityName, out var activity))
            {
                return SignUpResult.ActivityNotFound;
            }

            if (activity.Participants.Contains(email))
            {
                return SignUpResult.AlreadyRegistered;
            }

            activity.Participants.Add(email);
            return SignUpResult.Success;
        }
    }
}

sealed class Activity(string description, string schedule, int maxParticipants, string category, List<string> participants)
{
    public string Description { get; } = description;
    public string Schedule { get; } = schedule;
    public int MaxParticipants { get; } = maxParticipants;
    public string Category { get; } = category;
    public List<string> Participants { get; } = participants;

    public Activity Copy() => new(Description, Schedule, MaxParticipants, Category, [.. Participants]);
}

enum SignUpResult
{
    Success,
    ActivityNotFound,
    AlreadyRegistered
}

public partial class Program;
