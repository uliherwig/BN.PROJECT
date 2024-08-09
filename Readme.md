# BN.TRADER 

## source code

https://github.com/uliherwig/BN.TRADER

## Description

BN.TRADER is a platform which combines brokerage operations with algorithmic trading.

## Archtecture

The microservice archtecture uses DDD principles

## Features / Services

### AlpacaService

Alpaca Service - provides access to Alpaca API. 
local usage - http://localhost:5130/swagger

### TradingStrategyService
Trading Strategies Service - provides access to trading strategies.

## CI/CD

The CI/CD pipeline is implemented with Github Actions. 
The pipeline is triggered on push to the master branch. 
The pipeline builds the docker image and pushes it to the docker hub.




## Installation

local setup
docker build -t bntraderapi .
docker run -t -p 5130:5130 bntraderapi

or use docker-compose

## Usage






	