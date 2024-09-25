# [netstr - a nostr relay](https://netstr.io/)
[![release](https://img.shields.io/github/v/release/bezysoftware/netstr)](https://github.com/bezysoftware/netstr/releases)
[![build](https://github.com/bezysoftware/netstr/workflows/build/badge.svg)](https://github.com/bezysoftware/netstr/workflows/actions)

![netstr logo](art/logo.jpg)

Netstr is a relay for the [nostr protocol](https://github.com/nostr-protocol/nostr) written in C#. Currently in early stages of development.

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
- [x] NIP-70: [Protected events](https://github.com/nostr-protocol/nips/blob/master/70.md)

## Setup

Netstr is c# app backed by a Postgres database. You have several options to get up and running:

* Using dotnet tools
* Using docker run
* Using docker compose
* Deploying to Azure

https://learn.microsoft.com/en-us/cli/azure/
https://dotnet.microsoft.com/en-us/download