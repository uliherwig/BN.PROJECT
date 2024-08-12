# AlpacaService

API implementation for trading and data retrieval from alpaca
Manages historical data, where data is stored in a postgres db

## Version 2024.8.2

- hosted service for data retrieval
- service updates historical data for specified assets

## Version 2024.8.1

- alpaca api implementation for trading and data retrieval
- data layer for storing historical data and assets
- integration postgres db - code first approach
	dotnet ef migrations add custom_migration_name
	dotnet ef database update
- alpaca historical data storage to db for local investigations

