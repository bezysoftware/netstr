Feature: NIP-11

Background: 
	Given a relay is running
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                      |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | nsec12y4pgafw6kpcqjtfyrdyxtcupnddj5kdft768kdl55wzq50ervpqauqnw4 |
	
Scenario: Relay sends an information document
	GET HTTP request to the websockets endpoint with a application/nostr+json Accept header should
	produce a json Relay Information Document
	When Alice sends a GET HTTP request to its websockets endpoint
	| Header | Value                  |
	| Accept | application/nostr+json |
	Then Alice receives a response with headers
	| Header                      | Value |
	| Access-Control-Allow-Origin | *     |
	And Alice receives a response with json content
	| Field          | Type   |
	| name           | string |
	| description    | string |
	| contact        | string |
	| pubkey         | string |
	| software       | string |
	| version        | string |
	| supported_nips | int[]  |
