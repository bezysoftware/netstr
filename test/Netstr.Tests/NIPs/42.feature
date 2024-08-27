Feature: NIP-42
	Defines a way for clients to authenticate to relays by signing an ephemeral event.

Background: 
	Given a relay is running with AUTH required
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |

Scenario: Not authenticated client cannot publish or subscribe
	When Alice sends a subscription request abcd
	| Kinds |
	| 1     |
	And Alice publishes events
	| Id                                                               | Content | Kind | Tags | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |      | 1722337838 |
	Then Alice receives messages
	| Type   | Id                                                               | Success |
	| AUTH   | *                                                                |         |
	| CLOSED | abcd                                                             |         |
	| OK     | 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | false   |

Scenario: Authenticated client can publish and subscribe
	When Alice publishes an AUTH event for the challenge sent by relay
	And Alice sends a subscription request abcd
	| Kinds |
	| 2     |
	And Alice publishes events
	| Id                                                               | Content | Kind | Tags | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |      | 1722337838 |
	Then Alice receives messages
	| Type | Id                                                               | Success |
	| AUTH | *                                                                |         |
	| OK   | *                                                                | true    |
	| EOSE | abcd                                                             |         |
	| OK   | 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | true    |

Scenario: Client stays unauthenticated when invalid challenge is used
	When Alice publishes an AUTH event with invalid challenge
	When Alice sends a subscription request abcd
	| Kinds |
	| 1     |
	And Alice publishes events
	| Id                                                               | Content | Kind | Tags | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |      | 1722337838 |
	Then Alice receives messages
	| Type   | Id                                                               | Success |
	| AUTH   | *                                                                |         |
	| OK     | *                                                                | false   |
	| CLOSED | abcd                                                             |         |
	| OK     | 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | false   |
