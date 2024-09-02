Feature: NIP-45
	Relays may support the verb COUNT, which provides a mechanism for obtaining event counts. 

Background: 
	Given a relay is running with AUTH enabled
	And Alice is connected to relay 
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |
	And Bob is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 | 3551fc7617f76632e4542992c0bc01fecb224de639c4b6a1e0956946e8bb8a29 |
	And Charlie is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614 | f77f81a6a223eb15f81fee569161a4f729401a9cbc31bb69fef6a949b9d3c23a |

Scenario: Counting followers
	Bob follows Alice, Charlie follows Bob. Alice's follower count should be 1
	When Bob publishes an event
	| Id                                                               | Content | Tags                                                                       | Kind | CreatedAt  |
	| d589498c49776340a9bf83f63cc4cf960a17360cc3d9fd2a2ec2de4f11ba82b4 |         | [["p","5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75"]] | 3    | 1722337838 |
	And Charlie publishes an event
	| Id                                                               | Content | Tags                                                                       | Kind | CreatedAt  |
	| 2ef0ecd7341f5fdb5634210a4505d1c4ba25cb6ff4721282fd45412f93842c66 |         | [["p","5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627"]] | 3    | 1722337838 |
	And Alice sends a count message abcd
	| Kinds | #p                                                               |
	| 3     | 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 |
	Then Alice receives a message
	| Type  | Id   | Count |
	| AUTH  | *    |       |
	| COUNT | abcd | 1     |

Scenario: Counting DMs is rejected when not authenticated
	When Alice sends a count message abcd
	| Kinds |
	| 4     |
	Then Alice receives a message
	| Type   | Id   | Count |
	| AUTH   | *    |       |
	| CLOSED | abcd |       |

Scenario: Counting someone elses DMs returns only those from me
	Bob sends a DM to Charlie
	Alice sends a DM to Charlie
	Alice tries to count all Charlie's DMs but only those from her are counted
	Charlie counts his own DMs which should return count of all
	When Alice publishes an AUTH event for the challenge sent by relay
	And Charlie publishes an AUTH event for the challenge sent by relay
	And Bob publishes an event
	| Id                                                               | Content          | Kind | Tags                                                                       | CreatedAt  |
	| a8b0f9d313888642257af20fc4dbe4a3d71d3c3a72bcfc06c540a235172b7f37 | Charlie's Secret | 4    | [["p","fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614"]] | 1722337838 |
	And Alice publishes an event
	| Id                                                               | Content          | Kind | Tags                                                                       | CreatedAt  |
	| 7b0535b94878efb18b7c7a13630db8227e30961aed6f5556823b612423d676af | Charlie's Secret | 4    | [["p","fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614"]] | 1722337838 |
	And Alice sends a count message abcd
	| Kinds | #p                                                               |
	| 4     | fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614 |
	And Charlie sends a count message abcd
	| Kinds | #p                                                               |
	| 4     | fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614 |
	Then Alice receives messages
	| Type  | Id                                                               | Success | Count |
	| AUTH  | *                                                                |         |       |
	| OK    | *                                                                | true    |       |
	| OK    | 7b0535b94878efb18b7c7a13630db8227e30961aed6f5556823b612423d676af | true    |       |
	| COUNT | abcd                                                             |         | 1     |
	And Charlie receives messages
	| Type  | Id   | Success | Count |
	| AUTH  | *    |         |       |
	| OK    | *    | true    |       |
	| COUNT | abcd |         | 2     |