\# PlantHub 🌱



PlantHub is a Home Assistant add-on for managing plants, watering schedules, and plant-related events in a simple and structured way.



The project is currently in \*\*alpha\*\* and under active development.



---



\## Status



⚠️ \*\*Alpha release\*\*



PlantHub is functional but not feature-complete.  

Data models, UI, and integrations may change between versions.



---



\## Features



\- Create and manage plants

\- Group plants into watering schedules

\- Track last and next watering dates

\- Visual indicators for overdue watering

\- Local image storage for plants



Planned features:

\- Home Assistant notifications

\- Background services for watering reminders

\- Sensor integrations (soil moisture, etc.)

\- Statistics and history views



---



\## Architecture



\- \*\*Blazor Server\*\* (.NET)

\- \*\*SQLite\*\* database (stored in HA add-on data volume)

\- Runs as a \*\*Home Assistant Add-on\*\*

\- Designed for local-first usage (no cloud dependencies)



---



\## Home Assistant Add-on



The Home Assistant add-on files are located in the `addon/` directory.



\### Installation (manual)



1\. Clone this repository

2\. Copy the `addon/PlantHub` directory to: /addons/PlantHub

3\. Update the version number in:

\- `addon/config.yaml`

\- `addon/Dockerfile`

4\. In Home Assistant:

\- Go to \*\*Settings → Add-ons\*\*

\- Install the PlantHub add-on

\- Start the add-on



> Note: The version number in the Dockerfile is used to break Docker build cache.



---



\## Data \& Storage



\- \*\*Database\*\*: SQLite (created automatically on first run)

\- \*\*Database location\*\*: `/data`

\- \*\*Plant images\*\*: `/config/www/plant-hub/`



No database files are committed to the repository.



---



\## Development



Local development is possible without Home Assistant.



\- Run with `dotnet run`

\- SQLite database will be created automatically

\- Configuration is loaded from `appsettings.json`



Home Assistant–specific paths are abstracted behind services.



---



\## Versioning



\- The Home Assistant add-on version is defined in `config.yaml`

\- The Docker image build version is defined in the Dockerfile

\- Both should be kept in sync



---



\## License



License not yet defined.

