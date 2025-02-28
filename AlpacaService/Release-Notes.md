# BN.PROJECT 

BN.PROJECT is a platform which combines brokerage operations with algorithmic trading.

## Version 2025.02.3

- testservice added for ci/cd testing

## Version 2025.02.2

- identityservice improvement

## Version 2025.02.1

- alpaca execution

## Version 2025.01.2

- maintenance page
- alpaca execution 

## Version 2025.01.1

- unit tests for all services

## Version 2024.12.1

- SMA strategy refactoring

## Version 2024.11.2

- SMA strategy implementation

## Version 2024.11.1

- implement architecture for handling of different strategies
- refactor models
- implement template for new strategies

## Version 2024.10.4

- refactor alpaca clients usage
- implement alpaca credentials storage
- use userId instead of email for user identification

## Version 2024.10.3

- settings extension
- delete backtest by user
- delete backtest by periodic task
- implement backtest bookmarking
- refactor strategy service
- implement strategyTestService and strategyOperations

## Version 2024.10.2

- settings and position models extended
- implement api for backtest results

## Version 2024.10.1

- implement multi user support for backtests
- 1. store test settings to db and start test
- 2. handle test execution 

## Version 2024.9.6

- implement core library and strategy service
- alpaca service backtest refactoring  
- 1. validate testsettings
- 2. get the historical data
- 3. start the test - implement the strategy service client
- 4. init message bus - start sending data
- 5. implement the strategy service
- 6. broker(alpaca) service controller - start backtest
- 7. implement the services - 
- 8. subscribe to datafeed from message bus 
- 9. execute the strategy

## Version 2024.9.5

- usersettings db change - user identified by email
- backtest service added 
- position manager added
- backtest api for testing strategies
- quote storage in db

## Version 2024.9.4

- storage of alpaca data (bars) moved to alpaca service
- keycloak authorization
- storage of alpaca key and secret in usersettings

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

