# BN.PROJECT 

## source code

https://github.com/uliherwig/BN.PROJECT

## Description

The BN Project is a platform which combines brokerage operations with algorithmic trading, encompassing both traditional methodologies and AI-driven approaches. 
Users have the flexibility to create and fine-tune their own strategies to suit their trading preferences.

Currently, the platform is under development, with new features planned for regular release post-launch.
During this phase, Alpaca paper accounts are utilized for strategy testing, ensuring a robust and reliable trading experience.

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

### NotificationService

NotificationService - provides notifications to React FE per signalR/Redis

### FinAIService / python

FinAIService - provides AI based trading strategies 
and analysis - implemented in Python- pending

## CI/CD

The CI/CD pipeline is implemented with Github Actions. 
The pipeline is triggered on push to the main branch. 
The pipeline builds the docker image and pushes it to the docker hub.
The application is deployed to the Kubernetes cluster with Helm.

## Installation

local setup with docker-compose (https://github.com/uliherwig/bn-docker)	

dev setup - local VM - linux ubuntu server - rancher kubernetes - install with helm

production setup - pending