Feature: NIP-70
	When the "-" tag is present, that means the event is "protected".
	A protected event is an event that can only be published to relays by its author.

Background: 
	Given a relay is running with AUTH enabled
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |
	
Scenario: Not authenticated client tries to publish protected event
	Alice cannot publish protected events when she isn't authenticated
	When Alice publishes an event
	| Id                                                               | Content   | Kind | Tags      | CreatedAt  |
	| 92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5 | Protected | 1    | [[ "-" ]] | 1722337837 |
	Then Alice receives a message
	| Type  | Id                                                               | Success |
	| AUTH  | *                                                                |         |
	| OK    | 92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5 | false   |

Scenario: Authenticated client publishes their protected event
	Once Alice authenticates she can publish protected events
	When Alice publishes an AUTH event for the challenge sent by relay
	When Alice publishes an event
	| Id                                                               | Content   | Kind | Tags      | CreatedAt  |
	| 92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5 | Protected | 1    | [[ "-" ]] | 1722337837 |
	Then Alice receives a message
	| Type  | Id                                                               | Success |
	| AUTH  | *                                                                |         |
	| OK    | *                                                                | true    |
	| OK    | 92f3f4bfb1c756108b242dc02169fa96bd53d5ac5331c6ac5e377045637e2cf5 | true    |

Scenario: Authenticated client tries to publish someone else's protected event
	The event Alice tries to publish was signed by Bob, relay should reject it
	When Alice publishes an AUTH event for the challenge sent by relay
	When Alice publishes an event
	| Id                                                               | PublicKey                                                        | Content   | Kind | Tags      | CreatedAt  |
	| 1c982ee8b0f2484815a4befbb26bb02d6b20b4b3a85bfe6568a3712f943aa940 | 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 | Protected | 1    | [[ "-" ]] | 1722337837 |
	Then Alice receives a message
	| Type  | Id                                                               | Success |
	| AUTH  | *                                                                |         |
	| OK    | *                                                                | true    |
	| OK    | 1c982ee8b0f2484815a4befbb26bb02d6b20b4b3a85bfe6568a3712f943aa940 | false   |
