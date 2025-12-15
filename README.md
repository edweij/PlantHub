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
4. Add: https://github.com/edweij/PlantHub
5. Reload the Add-on Store

### Install add-on

1. Select **PlantHub**
2. Click **Install**
3. Start the add-on
4. Open the Web UI via ingress

---

## Data & Storage

- **Database:** SQLite
- Stored at `/data/planthub.db`
- Created automatically on first start
- **Plant images:**
- Stored under `/config/www/plant-hub/`

Removing the add-on data will remove all stored plants.

---

## Configuration

At the moment, PlantHub does not require any mandatory configuration.

Optional settings may be added in future versions.

---

## Versioning & Updates

- The add-on version is defined in `config.yaml`
- Updating the version triggers a rebuild of the add-on image
- Dockerfile does not need to be changed between releases

---

## Development notes

This repository is structured as a Home Assistant add-on project.

- The add-on directory contains:
- Dockerfile
- config.yaml
- rootfs
- application source (`src/`)
- The add-on build context is self-contained
- No external downloads or private credentials are required during build

---

## License

License not yet defined.
