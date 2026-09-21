# Mergington High School Activities

A small FastAPI application for viewing extracurricular activities and registering students.

## Prerequisites

- Python 3.10 or later

## Run Locally

From the repository root, create and activate a virtual environment:

```powershell
python -m venv .venv
.venv\Scripts\Activate.ps1
```

Install the dependencies and start the development server:

```powershell
pip install -r requirements.txt
cd src
uvicorn app:app --reload
```

Open the application at http://localhost:8000/. FastAPI's interactive API documentation is available at http://localhost:8000/docs.

Stop the server with `Ctrl+C`. Activity data is stored in memory, so registrations reset whenever the server restarts.

## Test

After adding tests, run them from the `src` directory:

```powershell
pytest
```

## API Routes

| Method | Route                                                             | Description                            |
| ------ | ----------------------------------------------------------------- | -------------------------------------- |
| `GET`  | `/activities`                                                     | List all activities and their details. |
| `GET`  | `/activities/{activity_name}/participants`                        | List participants for one activity.    |
| `POST` | `/activities/{activity_name}/signup?email=student@mergington.edu` | Register a student for an activity.    |
