from fastapi.testclient import TestClient

from app import app, activities

client = TestClient(app)


def test_get_activities_returns_200():
    response = client.get("/activities")
    assert response.status_code == 200


def test_get_activities_returns_all_activities():
    response = client.get("/activities")
    data = response.json()

    assert set(data.keys()) == set(activities.keys())


def test_get_activities_contains_expected_activity_fields():
    response = client.get("/activities")
    data = response.json()

    chess_club = data["Chess Club"]
    assert chess_club["description"] == "Learn strategies and compete in chess tournaments"
    assert chess_club["schedule"] == "Fridays, 3:30 PM - 5:00 PM"
    assert chess_club["max_participants"] == 12
    assert chess_club["category"] == "Academic"
    assert "michael@mergington.edu" in chess_club["participants"]


def test_get_activities_matches_in_memory_data():
    response = client.get("/activities")
    assert response.json() == activities