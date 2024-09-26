# BN.PROJECT 

## source code

https://github.com/uliherwig/BN.PROJECT

## Description

BN.PROJECT is a platform which combines brokerage operations with algorithmic trading.

## Archtecture

The microservice archtecture uses DDD principles

## Features / Services

### AlpacaService

Alpaca Service - provides access to Alpaca API. 

### IdentityService

Identity Service - IAM Functionality
Integration with Keycloak

### TradingStrategyService
Trading Strategies Service - provides access to trading strategies.
- BacktestService




## CI/CD

The CI/CD pipeline is implemented with Github Actions. 
The pipeline is triggered on push to the main branch. 
The pipeline builds the docker image and pushes it to the docker hub.




## Installation

local setup
docker build -t alpacaservice .
docker run -t -p 5130:5130 alpacaservice

or use docker-compose

## Usage






	