# BN.PROJECT 

BN.PROJECT is a platform which combines brokerage operations with algorithmic trading.


## to be done

- add unit test project for alpaca service
- add unit tests for alpaca service
- implement code coverage tool coverlet
- implement code coverage report generator 

## Version 2024.9.3

- storage of identity data (users, sessions) moved to identity service
- keycloak role handling implemented

## Version 2024.9.2

- data service added. 

## Version 2024.9.1

- identity service refactioring

## Version 2024.8.4

- identity service integration with keycloak 
- create config for keycloak
- api for user registration and login

## Version 2024.8.3

- extend alpaca api implementation for trading methods (orders/trades)
- extend data layer for storing trading data
- extend asset model to include selection of assets for trading
- implement api methods for upating asset data

## Version 2024.8.2

- hosted service for periodical data retrieval
- service updates historical data for specified assets

## Version 2024.8.1

- alpaca api implementation for trading and data retrieval
- data layer for storing historical data and assets
- integration postgres db - code first approach
	dotnet ef migrations add InitialCreate
	dotnet ef database update

- alpaca historical data storage to db for local investigations

