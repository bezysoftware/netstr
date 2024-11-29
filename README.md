# [netstr - a nostr relay](https://relay.netstr.io/)
[![release](https://img.shields.io/github/v/release/bezysoftware/netstr)](https://github.com/bezysoftware/netstr/releases)
[![build](https://github.com/bezysoftware/netstr/workflows/build/badge.svg)](https://github.com/bezysoftware/netstr/workflows/actions)

![netstr logo](art/logo.jpg)

Netstr is a modern relay for the [nostr protocol](https://github.com/nostr-protocol/nostr) written in C#. 

 * **Prod** instance: https://relay.netstr.io/
 * **Dev** instance: https://relay-dev.netstr.io/ (feel free to play with it / try to break it, just report if you find anything that needs fixing)

## Features

NIPs with a relay-specific implementation are listed here.

- [x] NIP-01: [Basic protocol flow description](https://github.com/nostr-protocol/nips/blob/master/01.md)
- [x] NIP-02: [Follow list](https://github.com/nostr-protocol/nips/blob/master/02.md)
- [x] NIP-04: [Encrypted Direct Message](https://github.com/nostr-protocol/nips/blob/master/04.md) (deprecated in favor of NIP-17)
- [x] NIP-09: [Event deletion](https://github.com/nostr-protocol/nips/blob/master/09.md)
- [x] NIP-11: [Relay information document](https://github.com/nostr-protocol/nips/blob/master/11.md)
- [x] NIP-13: [Proof of Work](https://github.com/nostr-protocol/nips/blob/master/13.md)
- [x] NIP-17: [Private Direct Messages](https://github.com/nostr-protocol/nips/blob/master/17.md)
- [x] NIP-40: [Expiration Timestamp](https://github.com/nostr-protocol/nips/blob/master/40.md)
- [x] NIP-42: [Authentication of clients to relays](https://github.com/nostr-protocol/nips/blob/master/42.md)
- [x] NIP-45: [Counting results](https://github.com/nostr-protocol/nips/blob/master/45.md)
- [ ] NIP-50: [Search Capability](https://github.com/nostr-protocol/nips/blob/master/50.md)
- [x] NIP-62: [Request to Vanish](https://github.com/vitorpamplona/nips/blob/right-to-vanish/62.md)
- [x] NIP-70: [Protected events](https://github.com/nostr-protocol/nips/blob/master/70.md)
- [x] NIP-77: [Negentropy syncing](https://github.com/nostr-protocol/nips/pull/1494)
- [x] NIP-119: [AND operator for filters](https://github.com/nostr-protocol/nips/pull/1365)

## Tests

Each supported NIP has a set of tests written in [Specflow / Gherkin language](https://docs.specflow.org/projects/specflow/en/latest/Gherkin/Gherkin-Reference.html). 
The scenarios are described in plain English which lets anyone read them and even contribute with new ones without any programming skills. See sample (simplified):

```gherkin
Scenario: Newly subscribed client receives matching events, EOSE and future events
    Given a relay is running
    And Alice is connected to relay
    And Bob is connected to relay
    When Bob publishes events
    | Id                                                               | Content  | Kind  | CreatedAt  |
    | a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | Hello 1  | 1     | 1722337838 |
    | cb952d0ab727c3fcaf94e6809a64d1a27ff87cae5be583398ee7f0f1381d6b66 | Hello MD | 30023 | 1722337839 |
    And Alice sends a subscription request abcd
    | Kinds |
    | 1     |
    And Bob publishes an event
    | Id                                                               | Content | Kind | CreatedAt  |
    | 8013e4630a69528007355f65e01936c9b761a4bbd9340b60a4bd0222b15b7cf3 | Hello 2 | 1    | 1722337840 |
    Then Alice receives messages
    | Type  | Id   | EventId                                                          |
    | EVENT | abcd | a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 |
    | EOSE  | abcd |                                                                  |
    | EVENT | abcd | 8013e4630a69528007355f65e01936c9b761a4bbd9340b60a4bd0222b15b7cf3 |
    And Bob receives messages
    | Type | Id                                                               | Success | 
    | OK   | a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | true    |
    | OK   | cb952d0ab727c3fcaf94e6809a64d1a27ff87cae5be583398ee7f0f1381d6b66 | true    |
    | OK   | 8013e4630a69528007355f65e01936c9b761a4bbd9340b60a4bd0222b15b7cf3 | true    |
```

Above scenario simulates that `Bob` publishes events to a relay, `Alice` creates a subscription and `Bob` publishes more events. The scenario then asserts that `Alice` and `Bob`
both received their expected messages in correct order.

## Setup

Netstr is c# app backed by a Postgres database. You have several options to get up and running:

* Using `dotnet run`
* Using `docker run`
* Using `docker compose`
* Deploying to Azure

### Dotnet run

* Install .NET: https://dotnet.microsoft.com/en-us/download
* Install Postgres: https://www.postgresql.org/download/
* Edit `appsettings.json` and set a `NetstrDatabase` Connection String to point to your Postgres instance
* Run `dotnet run --project .\src\Netstr\Netstr.csproj`

### Docker run

* Install Docker: https://docs.docker.com/engine/install/
* Install Postgres: https://www.postgresql.org/download/
* Run `docker run -e ConnectionStrings__NetstrDatabase=YOUR_CONNECTION_STRING bezysoftware/netstr:latest`
  * Set your connection string to point to your Postgres instance

### Docker compose

Docker compose contains a Postgres DB service so no need to install it manually. You will however need to set the following environment variable:
 * NETSTR_DB_PASSWORD - password for Postgres DB
 
Optionally you can also set following variables:
 * NETSTR_IMAGE - docker image (default `bezysoftware/netstr:latest`)
 * NETSTR_PORT - port on which the relay will be accessible (default 8080)
 * NETSTR_ENVIRONMENT - will be used to name the compose instance (default 'prod')
 * NETSTR_ENVIRONMENT_LONG - will be used inside the application to load specific configuration (default 'Production')

### Deploying to Azure

The `scripts` folder contains scripts to setup a VM in Azure with everything you'll need to run a Netstr instance:
 * Separate VM with an attached data disk
 * Docker with Compose to run the `compose.yml`
 * Nginx with certbot which generates an SSL certificate for your domain
