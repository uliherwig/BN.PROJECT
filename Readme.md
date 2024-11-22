# BN.PROJECT 

## source code

https://github.com/uliherwig/BN.PROJECT

## Description

BN.PROJECT is a platform which combines brokerage operations with algorithmic trading.
The platform is designed to be modular and scalable. It is based on microservices and uses DDD principles.
The UI is implemented with Next.js. (https://github.com/uliherwig/BN.NEXT.CLIENT)

## Database

Postgres

## Message Broker

Kafka

## Services

### CORE

Shared Kernel - Shared models

### AlpacaService

Alpaca Service - provides access to Broker Alpaca. 

### IdentityService

IdentityService - Identity and Access Management Service
Integration with Keycloak

### StrategyService

StrategyService - contains and applies the trading strategies

## CI/CD

The CI/CD pipeline is implemented with Github Actions. 
The pipeline is triggered on push to the main branch. 
The pipeline builds the docker image and pushes it to the docker hub.

## Installation

local setup with docker-compose (https://github.com/uliherwig/bn-docker)	