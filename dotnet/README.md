# Mergington High School Activities (.NET)

An ASP.NET Core 10 minimal API and static web UI that mirrors the Python/FastAPI application in the repository root.

Open `Mergington.Activities.sln` in Visual Studio to load both the application and its test project into Solution Explorer and Test Explorer.

## Run locally

From this directory:

```powershell
dotnet run
```

Open the URL printed by the application, such as `http://localhost:5000`.

## Routes

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/activities` | List activities and their details. |
| `GET` | `/activities/{activityName}/participants` | List an activity's participants. |
| `POST` | `/activities/{activityName}/signup?email=student@mergington.edu` | Register a student. |

Activity data is held in memory, so registrations are reset when the application restarts.

## Test

From this directory:

```powershell
dotnet test tests
```
