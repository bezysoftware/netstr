Feature: NIP-40
	The expiration tag enables users to specify a unix timestamp at which the message SHOULD be considered expired (by relays and clients) and SHOULD be deleted by relays.

Background: 
	Given a relay is running
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |
	And Bob is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 | 3551fc7617f76632e4542992c0bc01fecb224de639c4b6a1e0956946e8bb8a29 |
	
Scenario: Unparsable expiration tag is ignored
	Event contains expiration tag but it's not a valid unix timestamp, it should be ignored and event is accepted
	When Alice publishes events
	| Id                                                               | Content | Kind | Tags                    | CreatedAt  |
	| 0921e0c46e637526c0cb2211cbab49a56a69373b0f86c2500ed530f1533df182 | Test    | 1    | [["expiration","blah"]] | 1722337838 |
	Then Alice receives messages
	| Type | Id                                                               | Success |
	| OK   | 0921e0c46e637526c0cb2211cbab49a56a69373b0f86c2500ed530f1533df182 | true    |
	
Scenario: Already expired event is rejected
	Event contains expiration tag but it's not a valid unix timestamp, it should be ignored and event is accepted
	When Alice publishes events
	| Id                                                               | Content | Kind | Tags                          | CreatedAt  |
	| 4239479a101dbeb8f189dacd6e4638a11013b5a2fc0733901f83c9e84e611778 | Test    | 1    | [["expiration","1231002905"]] | 1722337838 |
	Then Alice receives messages
	| Type | Id                                                               | Success |
	| OK   | 4239479a101dbeb8f189dacd6e4638a11013b5a2fc0733901f83c9e84e611778 | false   |
	
Scenario: Expired event already saved in a relay is omitted from sub response
	We need to save an already expired event in the relay, that would be hard using the publishing step (relay would reject it)
	So just introduce a new step for this NIP which bypasses publishing and inserts directly into DB
	Given Bob previously published events
	| Id                                                               | Content | Kind | Tags                          | CreatedAt  |
	| 4239479a101dbeb8f189dacd6e4638a11013b5a2fc0733901f83c9e84e611778 | Test    | 1    | [["expiration","1231002905"]] | 1722337838 |
	When Alice sends a subscription request abcd
	| Kinds |
	| 1     |
	Then Alice receives messages
	| Type | Id   |
	| EOSE | abcd |