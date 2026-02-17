# TicketingSystem

This project handles ticketing system in your company using dotnet core API.

## Frontend (React)

The frontend lives in `TicketingSystem.Frontend`.

### Why you saw `npm ERR! enoent ... TicketingSystem\\package.json`

That error happens when running `npm install` from the repository root while no root `package.json` exists.
This repo now includes a root `package.json` with helper scripts so you can run frontend commands from the root safely.

### Prerequisites

- Node.js 18+ (or 20+ recommended)
- npm 9+

### Option A: Run from repository root (recommended)

```bash
npm run frontend:install
npm run frontend:dev
```

Then open the URL shown in the terminal (typically `http://localhost:5173/`).

### Option B: Run directly inside frontend folder

```bash
cd TicketingSystem.Frontend
npm install
npm run dev
```

### Build for production

From repo root:

```bash
npm run frontend:build
```

Or from frontend folder:

```bash
cd TicketingSystem.Frontend
npm run build
```

The production files are generated in `TicketingSystem.Frontend/dist`.


### If you still see a black/empty page

1. Make sure you opened the Vite URL (usually `http://localhost:5173/`) and **not** the API URL.
2. Stop and restart the dev server:
   ```bash
   npm run frontend:dev
   ```
3. Hard refresh the browser (`Ctrl+F5`).
4. Open browser DevTools Console and check for errors (missing dependencies, blocked scripts, etc.).