using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace tests;

public sealed class ActivitiesApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task GetActivities_ReturnsTheExpectedActivityContract()
    {
        var response = await client.GetAsync("/activities");

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var activities = document.RootElement;
        var chessClub = activities.GetProperty("Chess Club");

        Assert.Equal(5, activities.EnumerateObject().Count());
        Assert.Equal("Learn strategies and compete in chess tournaments", chessClub.GetProperty("description").GetString());
        Assert.Equal("Fridays, 3:30 PM - 5:00 PM", chessClub.GetProperty("schedule").GetString());
        Assert.Equal(12, chessClub.GetProperty("maxParticipants").GetInt32());
        Assert.Equal("Academic", chessClub.GetProperty("category").GetString());
        Assert.Contains(
            chessClub.GetProperty("participants").EnumerateArray().Select(participant => participant.GetString()),
            participant => participant == "michael@mergington.edu");
    }

    [Fact]
    public async Task GetParticipants_ForUnknownActivity_ReturnsNotFound()
    {
        var response = await client.GetAsync("/activities/Unknown%20Activity/participants");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            "Activity not found",
            (await response.Content.ReadFromJsonAsync<ApiError>())?.Detail);
    }

    [Fact]
    public async Task SignUp_ForUnknownActivity_ReturnsNotFound()
    {
        var response = await client.PostAsync("/activities/Unknown%20Activity/signup?email=student%40mergington.edu", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            "Activity not found",
            (await response.Content.ReadFromJsonAsync<ApiError>())?.Detail);
    }

    [Fact]
    public async Task SignUp_WithMissingEmail_ReturnsBadRequest()
    {
        var response = await client.PostAsync("/activities/Chess%20Club/signup", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_WithUrlEncodedActivityName_AddsParticipant()
    {
        const string email = "integration-signup@mergington.edu";

        var signUpResponse = await client.PostAsync($"/activities/Chess%20Club/signup?email={Uri.EscapeDataString(email)}", null);
        var participantsResponse = await client.GetAsync("/activities/Chess%20Club/participants");

        Assert.Equal(HttpStatusCode.OK, signUpResponse.StatusCode);
        Assert.Equal(
            $"Signed up {email} for Chess Club",
            (await signUpResponse.Content.ReadFromJsonAsync<SignUpResponse>())?.Message);
        Assert.Equal(HttpStatusCode.OK, participantsResponse.StatusCode);
        Assert.Contains(
            email,
            await participantsResponse.Content.ReadFromJsonAsync<List<string>>() ?? []);
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_ReturnsConflict()
    {
        const string email = "integration-duplicate@mergington.edu";

        var firstResponse = await client.PostAsync($"/activities/Chess%20Club/signup?email={Uri.EscapeDataString(email)}", null);
        var duplicateResponse = await client.PostAsync($"/activities/Chess%20Club/signup?email={Uri.EscapeDataString(email)}", null);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        Assert.Equal(
            "Student is already signed up for this activity",
            (await duplicateResponse.Content.ReadFromJsonAsync<ApiError>())?.Detail);
    }

    [Fact]
    public async Task Root_ServesTheActivitiesPage()
    {
        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        Assert.Contains("Mergington High School", await response.Content.ReadAsStringAsync());
    }

    private sealed record ApiError(string Detail);

    private sealed record SignUpResponse(string Message);
}
