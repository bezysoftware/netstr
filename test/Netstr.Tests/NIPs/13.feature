Feature: NIP-13
	 Proof of Work (PoW) is a way to add a proof of computational work to a note.
	 This proof can be used as a means of spam deterrence.

Background: 
	Given a relay is running with options
	| Key              | Value |
	| MinPowDifficulty | 20    |
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |

Scenario: Messages with low difficulty and those off target are rejected, those with high and on target difficulty accepted
	1) Low diff
	2) High diff but doesn't match target
	3) High diff
	4) High diff matching target
	When Alice publishes events
	| Id                                                               | Content | Tags                                                      | Kind | CreatedAt  |
	| 00387d3bb57ceab60effbefffcaecff27614c60c75d7b36b01caa71249e3ca3c | Hello   | [["nonce", "cc2e9737-e4f5-48d2-8c55-1461aeca3c87"]]       | 1    | 1722337838 |
	| 0000017cb9da5d1295c5d9e902055c25280ae95ea6767ad89a02f928742b703d | Hello   | [["nonce", "84fe8193-f35e-4d9e-9871-b509caaa6412", "5"]]  | 1    | 1722337838 |
	| 00000ed0cf8d67d9cb4f5b211ad9c8daea5b7bbf7721e345070d98a91cc289ff | Hello   | [["nonce", "49c7c782-8f45-4dbb-adac-5ebc71c3363c"]]       | 1    | 1722337838 |
	| 000005e3b3172e58be368ed6b51b7ecf96a3d32b1107496bf6d786f8084aa17f | Hello   | [["nonce", "045b7487-e889-4179-9d52-ce46beffef66", "21"]] | 1    | 1722337838 |
	Then Alice receives messages
 	| Type | Id                                                               | Success |
 	| OK   | 00387d3bb57ceab60effbefffcaecff27614c60c75d7b36b01caa71249e3ca3c | false   |
 	| OK   | 0000017cb9da5d1295c5d9e902055c25280ae95ea6767ad89a02f928742b703d | false   |
 	| OK   | 00000ed0cf8d67d9cb4f5b211ad9c8daea5b7bbf7721e345070d98a91cc289ff | true    |
 	| OK   | 000005e3b3172e58be368ed6b51b7ecf96a3d32b1107496bf6d786f8084aa17f | true    |
