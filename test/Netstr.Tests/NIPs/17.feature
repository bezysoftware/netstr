Feature: NIP-17
	This NIP defines an encrypted direct messaging scheme using NIP-44 encryption and NIP-59 seals and gift wraps.

Background: 
	Given a relay is running with AUTH enabled
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |
	And Bob is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 | 3551fc7617f76632e4542992c0bc01fecb224de639c4b6a1e0956946e8bb8a29 |
	
Scenario: Not authenticated client tries to fetch kind 1059 events
	Alice can't fetch kind 1059 events when she isn't authenticated
	When Alice sends a subscription request abcd
	| Authors                                                          | Kinds  |
	|                                                                  | 1,1059 |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |        |
	Then Alice receives messages
	| Type   | Id   |
	| AUTH   | *    |
	| CLOSED | abcd |

Scenario: Authenticated client tries to fetch kind 1059 events
	Once Alice authenticates she can fetch their kind 1059 events, but no one else's
	When Alice publishes an AUTH event for the challenge sent by relay
	And Bob publishes events
	| Id                                                               | Content          | Kind | Tags                                                                       | CreatedAt  |
	| ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90 | Secret           | 1059 | [["p","5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75"]] | 1722337838 |
	| fb90964eba126b74bc71bf31e9e198dc4fbdd79e3de4d4f02dacddbe8a6ac71c | Charlie's Secret | 1059 | [["p","fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614"]] | 1722337838 |
	When Alice sends a subscription request abcd
	| Kinds |
	| 1059  |
	And Bob publishes events
	| Id                                                               | Content            | Kind | Tags                                                                       | CreatedAt  |
	| 03403b4d4c4fad3ff1f561f030dff80daa256c66a4a195e3eb58bce90b2457bd | Secret 2           | 1059 | [["p","5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75"]] | 1722337838 |
	| 0e9391da7663a19e77d11966f57396a89a3a7bef1be1d045475e75be8eca246e | Charlie's Secret 2 | 1059 | [["p","fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614"]] | 1722337838 |
	Then Alice receives messages
	| Type  | Id   | EventId                                                          | Success |
	| AUTH  | *    |                                                                  |         |
	| OK    | *    |                                                                  | true    |
	| EVENT | abcd | ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90 |         |
	| EOSE  | abcd |                                                                  |         |
	| EVENT | abcd | 03403b4d4c4fad3ff1f561f030dff80daa256c66a4a195e3eb58bce90b2457bd |         |

Scenario: Authenticated client tries to fetch kind 1059 events through other filters
	Even when using complex filters, authenticated client should still not receive someone else's kind 1059 events
	When Alice publishes an AUTH event for the challenge sent by relay
	And Bob publishes events
	| Id                                                               | Content          | Kind | Tags                                                                       | CreatedAt  |
	| ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90 | Secret           | 1059 | [["p","5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75"]] | 1722337838 |
	| fb90964eba126b74bc71bf31e9e198dc4fbdd79e3de4d4f02dacddbe8a6ac71c | Charlie's Secret | 1059 | [["p","fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614"]] | 1722337838 |
	When Alice sends a subscription request abcd
	| Ids                                                              | Authors                                                             | Kinds |
	|                                                                  |                                                                     | 1059  |
	| fb90964eba126b74bc71bf31e9e198dc4fbdd79e3de4d4f02dacddbe8a6ac71c |                                                                     |       |
	|                                                                  | fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f611059 |       |
	|                                                                  | fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f611059 | 1059  |
	Then Alice receives messages
	| Type  | Id   | EventId                                                          | Success |
	| AUTH  | *    |                                                                  |         |
	| OK    | *    |                                                                  | true    |
	| EVENT | abcd | ff526515d15975c3839f027cd301ba49afca237fa0d84f53765e9c320a269d90 |         |
	| EOSE  | abcd |                                                                  |         |