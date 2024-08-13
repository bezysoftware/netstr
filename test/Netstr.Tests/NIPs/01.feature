Feature: NIP-01
	Defines the basic protocol that should be implemented by everybody. 

Background: 
	Given a relay is running
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |
	And Bob is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 | 3551fc7617f76632e4542992c0bc01fecb224de639c4b6a1e0956946e8bb8a29 |
	And Charlie is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614 | f77f81a6a223eb15f81fee569161a4f729401a9cbc31bb69fef6a949b9d3c23a |

Scenario: Invalid messages are discarded, valid ones accepted
	Relay shouldn't broadcast messages with invalid Id or Signnature. It should also reply with OK false.
	This also covers correct validation of events with special characters
	When Alice sends a subscription request abcd
	| Kinds |
	| 1     |
	And Bob publishes events
	| Id                                                               | Content              | Kind | CreatedAt  | Signature |
	| ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff | Hello 1              | 1    | 1722337838 |           |
	| a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | Hello 1              | 1    | 1722337838 | Invalid   |
	| 9a6b4cefcd17f3bf7fb03c02da044c628836a118c47d5b92503c1d2bdb796296 | Hi ' \" \b \t \r \n  | 1    | 1722337838 |           |
	Then Bob receives messages
 	| Type | Id                                                               | Success |
 	| OK   | ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff | false   |
 	| OK   | a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | false   |
 	| OK   | 9a6b4cefcd17f3bf7fb03c02da044c628836a118c47d5b92503c1d2bdb796296 | true    |
	And Alice receives a message
 	| Type  | Id   | EventId                                                          |
 	| EOSE  | abcd |                                                                  |
 	| EVENT | abcd | 9a6b4cefcd17f3bf7fb03c02da044c628836a118c47d5b92503c1d2bdb796296 |

Scenario: Newly subscribed client receives matching events, EOSE and future events
	Bob publishes events which are stored by the relay before any subscription exists. 
	Alice then connects to the relay and should receive the matching stored events and EOSE.
	Bob publishes a new event which should be broadcast to Alice. 
	Bob receives OK for all of his messages.
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

Scenario: Closed subscriptions should no longer receive events
	After a subscription is closed the relay should no longer forward events for that subscription
	However it should still forward them for other existing subscriptions
	When Alice sends a subscription request abcd
	| Kinds |
	| 1     |
	And Alice sends a subscription request efgh
	| Kinds |
	| 1     |
	And Alice closes a subscription abcd
	And Bob publishes an event
	| Id                                                               | Content  | Kind  | CreatedAt  |
	| a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | Hello 1  | 1     | 1722337838 |
	Then Alice receives a message
	| Type  | Id   | EventId                                                          |
	| EOSE  | abcd |                                                                  |
	| EOSE  | efgh |                                                                  |
	| EVENT | efgh | a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 |
 	
Scenario: Events are treated differently based on their kind
	Regular events are covered by other scenarios
	Replaceable events have a unique combination of PublicKey+Kind and only the last version should be stored
	Ephemeral events shouldn't be stored
	Parametrized replaceable events have a unique combination of PublicKey+Kind+[d tag] and only the last version should be stored
	Relay should discard older versions of existing events
	Events returned for initial subscription request should be ordered by CreatedAt (newer first), then by Id lexically
	When Alice sends a subscription request abcd
	| Authors                                                          |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	And Bob publishes events
	| Id                                                               | Content | Kind  | Tags           | CreatedAt  |
	| eb480e60d0d3da6197602fd9d40172414cac1a0e777909f4451cdf3ebb8def2b | First   | 0     |                | 1722337838 |
	| 7dbe9b166930f9d6bb08279b785c8b28a9bc9cf1a060b0a3813a6bd521efce8e | Second  | 0     |                | 1722337839 |
	| a17c92627639d45cb31d2c63f7e1e852b37a753d27d59bae7522ffd0799e50fa | Third   | 0     |                | 1722337837 |
	| 5c05963d796eaeec7f72731a4c6c4241ed0f6e57b9ea4c640448efbaba34b8fc | Hello   | 20000 |                | 1722337838 |
	| 7e5931a00d6ebf4434515f32173feb98fc222a0cef55b8258acf01374984e37f | First   | 30000 | [[ "d", "a" ]] | 1722337837 |
	| 7e62d0e5a7869b4aa5d0f1e5f58ba0ca09c9c907fce17850b1622f7bbb6f7bde | Second  | 30000 | [[ "d", "a" ]] | 1722337838 |
	| cbefb02df14d326dcf8a0b8cb16aa264a041502d25c1e1952ebe3c54fbe9c53c | Third   | 30000 | [[ "d", "b" ]] | 1722337839 |
	| 8ba97fc616706391a663c60bb542427fdfaa1f743703077fb01439965fac751b | Fourth  | 30000 | [[ "d", "b" ]] | 1722337836 |
	And Charlie sends a subscription request abcd
	| Authors                                                          |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	Then Alice receives messages
	| Type  | Id   | EventId                                                          |
	| EOSE  | abcd |                                                                  |
	| EVENT | abcd | eb480e60d0d3da6197602fd9d40172414cac1a0e777909f4451cdf3ebb8def2b |
	| EVENT | abcd | 7dbe9b166930f9d6bb08279b785c8b28a9bc9cf1a060b0a3813a6bd521efce8e |
	| EVENT | abcd | 5c05963d796eaeec7f72731a4c6c4241ed0f6e57b9ea4c640448efbaba34b8fc |
	| EVENT | abcd | 7e5931a00d6ebf4434515f32173feb98fc222a0cef55b8258acf01374984e37f |
	| EVENT | abcd | 7e62d0e5a7869b4aa5d0f1e5f58ba0ca09c9c907fce17850b1622f7bbb6f7bde |
	| EVENT | abcd | cbefb02df14d326dcf8a0b8cb16aa264a041502d25c1e1952ebe3c54fbe9c53c |
	And Charlie receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 7dbe9b166930f9d6bb08279b785c8b28a9bc9cf1a060b0a3813a6bd521efce8e |
	| EVENT | abcd | cbefb02df14d326dcf8a0b8cb16aa264a041502d25c1e1952ebe3c54fbe9c53c |
	| EVENT | abcd | 7e62d0e5a7869b4aa5d0f1e5f58ba0ca09c9c907fce17850b1622f7bbb6f7bde |
	| EOSE  | abcd |                                                                  |

Scenario: Sending a subscription request with the same name restarts it
	Alice is initially subscribed to Bob (no events) but then resubscribes to Charlie
	Charlie previously published an event and publishes another one after Alice's new subscription
	Bob also publishes an event after Alice re-subscribes
	Alice should receive EOSE from Bob, then stored event+EOSE+new event from Charlie and no more events from Bob
	When Charlie publishes an event
	| Id                                                               | Content | Kind  | CreatedAt  |
	| 5138028d66a909d302d8283319eb2c0830b42694f6137f71c47c64b4bdab3ad1 | Hello   | 1     | 1722337836 |
	When Alice sends a subscription request abcd
	| Authors                                                          |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	And Alice sends a subscription request abcd
	| Authors                                                          |
	| fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614 |
	And Charlie publishes an event
	| Id                                                               | Content     | Kind | CreatedAt  |
	| a56ce3b0684d78d3ebe3d6d3e06d3a82317b8f7fdde9830727ee914b582a6091 | Hello again | 1    | 1722337837 |
	And Bob publishes events
	| Id                                                               | Content  | Kind  | CreatedAt  |
	| a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | Hello 1  | 1     | 1722337838 |
	Then Alice receives messages
	| Type  | Id   | EventId                                                          |
	| EOSE  | abcd |                                                                  |
	| EVENT | abcd | 5138028d66a909d302d8283319eb2c0830b42694f6137f71c47c64b4bdab3ad1 |
	| EOSE  | abcd |                                                                  |
	| EVENT | abcd | a56ce3b0684d78d3ebe3d6d3e06d3a82317b8f7fdde9830727ee914b582a6091 |

Scenario: Relay can handle complex filters
	Subscription requests can contain multiple filter objects which are interpreted as || conditions
	When Bob publishes events
	| Id                                                               | Content  | Kind  | CreatedAt  |
	| a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | Hello 1  | 1     | 1722337838 |
	| 0f5ba539c8ebb386336bc259ddc5d268a4959b012f56e3a2dcc1f9ea48d3591c |          | 0     | 1722337850 |
	| cb952d0ab727c3fcaf94e6809a64d1a27ff87cae5be583398ee7f0f1381d6b66 | Hello MD | 30023 | 1722337839 |
	When Charlie publishes events
	| Id                                                               | Content     | Kind  | CreatedAt  |
	| 4a173b1eaaf881eccaf28d943d4d028a652603d0718282a9d877a8dbbff02965 | Hello       | 30023 | 1722337835 |
	| 5138028d66a909d302d8283319eb2c0830b42694f6137f71c47c64b4bdab3ad1 | Hello       | 1     | 1722337836 |
	| a56ce3b0684d78d3ebe3d6d3e06d3a82317b8f7fdde9830727ee914b582a6091 | Hello again | 1     | 1722337837 |
	And Alice sends a subscription request abcd
	| Ids                                                              | Authors                                                          | Kinds | Tags                 | Since      | Until      | Limit |
	|                                                                  |                                                                  |       |                      |            |            | 1     |
	|                                                                  | fe8d7a5726ea97ce6140f9fb06b1fe7d3259bcbf8de42c2a5d2ec9f8f0e2f614 | 1,2   |                      | 1722337830 | 1722337836 |       |
	| a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 |                                                                  |       |                      |            |            |       |
	|                                                                  |                                                                  | 30023 |                      |            |            |       |
	Then Alice receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 0f5ba539c8ebb386336bc259ddc5d268a4959b012f56e3a2dcc1f9ea48d3591c |
	| EVENT | abcd | cb952d0ab727c3fcaf94e6809a64d1a27ff87cae5be583398ee7f0f1381d6b66 |
	| EVENT | abcd | a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 |
	| EVENT | abcd | 5138028d66a909d302d8283319eb2c0830b42694f6137f71c47c64b4bdab3ad1 |
	| EOSE  | abcd |                                                                  |