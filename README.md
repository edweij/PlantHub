# PlantHub 🌱

PlantHub is a Home Assistant add-on for managing plants, watering schedules, and plant-related events.

The project is currently in **alpha** and under active development.

---

## Status

⚠️ **Alpha**

PlantHub is functional but not feature-complete.
Data models, UI, and behavior may change between versions.

---

## Features

- Create and manage plants
- Group plants into watering schedules
- Track last and next watering
- Overdue watering indicators
- Local image storage for plants

Planned:
- Home Assistant notifications
- Background watering reminders
- Sensor integrations (soil moisture, etc.)
- Statistics and history

---

## Architecture

- **Home Assistant Add-on**
- **Blazor Server** (.NET)
- **SQLite** database (persistent under `/data`)
- Runs entirely locally, no cloud dependencies
- Uses Home Assistant ingress for UI access

The application source code lives inside the add-on build context.

---

## Installation (Home Assistant)

PlantHub is installed as a custom add-on repository.

### Add repository

1. Home Assistant → **Settings → Add-ons**
2. Open **Add-on Store**
3. Click **⋮ → Git repositories**
4. Add:
