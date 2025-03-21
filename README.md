# Job Manager

This solution contains a server and a client application to demonstrate an asynchronous job management system using .NET 9, C#, and React.

## 📁 Projects

- `JobManager`: Self-hosted ASP.NET Core Web API that handles asynchronous jobs
- `JobClient`: Console-based CLI to interact with the job server
- `JobConsole`: Web-based UI built in React with Vite, styled like a terminal
- `JobManagerTest`: Unit tests for the backend logic

## 🚀 How to Run

### Option 1: Run Locally

```bash
# Start the server
cd JobManager
dotnet run

# Start the CLI client
cd ../JobClient
dotnet run

# Start the React Console
cd ../job-console
npm install
npm run dev
```

Ensure the backend runs at `http://localhost:5049`

### Option 2: Run with Docker

```bash
docker-compose up --build
```

Then visit:

- Backend: [http://localhost:5049](http://localhost:5049)
- React Console: [http://localhost:3000](http://localhost:3000)

> The CLI client should be run manually using `dotnet run` in development mode.

## ✨ Features

- Create, cancel, list and restart jobs via REST endpoints
- React UI updates in real time using SignalR
- Console client interacts with all endpoints
- Max 5 concurrent jobs per type enforced
- Input validation, exception handling, and extensible architecture

---

## 🧪 Testing

Run tests with:

```bash
cd JobManagerTest
dotnet test
```
