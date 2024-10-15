Feature: NIP-62
	Nostr-native way to request a complete reset of a key's fingerprint on the web. 
	This procedure is legally binding in some jurisdictions, and thus, supporters of this NIP should truly delete events from their database.

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

Scenario: Request to Vanish deletes user's data
	Only requestor's data is deleted, including GiftWraps where they are tagged
	Only events from before the request's createdAt timestamp is deleted
	No-one else's events are deleted
	When Bob publishes events
	| Id                                                               | Content     | Kind | Tags                                                                       | CreatedAt  |
	| 1e4ef30065360dd8ba6a4b74c99b6d70447946fa17e31e2960f12d3d7a9fb643 | Hello       | 1    |                                                                            | 1728905459 |
	| bb5d31b0522faee9582dfede36a042a3209dc297f34c4850f2de3bbef05ad957 | Hello Later | 1    |                                                                            | 1728905481 |
	| 5c19b5808ee4ad3d31e4129cc112679147e28f3d88e24683a3afa327ba0a2ee8 | DM          | 1059 | [["p","5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75"]] | 1728905459 |
	| 78a1df26e6e30633663934dfb6da696184497ee98964aeae87292aae54bf166f | DM Late     | 1059 | [["p","5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75"]] | 1728905480 |
	When Alice publishes events
	| Id                                                               | Content        | Kind | Tags                     | CreatedAt  |
	| ff1092c354d94060a185f8b5e4349499079872babe27b882fd4632efcdd001c2 | Hello          | 1    |                          | 1728905459 |
	| f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd | Hello Later    | 1    |                          | 1728905480 |
	| 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e | I'm outta here | 62   | [["relay","ALL_RELAYS"]] | 1728905470 |
	And Charlie sends a subscription request abcd
	| Authors                                                                                                                           |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75,5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	Then Charlie receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | bb5d31b0522faee9582dfede36a042a3209dc297f34c4850f2de3bbef05ad957 |
	| EVENT | abcd | 78a1df26e6e30633663934dfb6da696184497ee98964aeae87292aae54bf166f |
	| EVENT | abcd | f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd |
	| EVENT | abcd | 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e |
	| EVENT | abcd | 1e4ef30065360dd8ba6a4b74c99b6d70447946fa17e31e2960f12d3d7a9fb643 |
	| EOSE  | abcd |                                                                  |

Scenario: Old events published after Request to Vanish are rejected
	After Request to Vanish events older than it cannot be re-published. Newer ones can be published normally.
	When Alice publishes events
	| Id                                                               | Content        | Kind | Tags                     | CreatedAt  |
	| ff1092c354d94060a185f8b5e4349499079872babe27b882fd4632efcdd001c2 | Hello          | 1    |                          | 1728905459 |
	| 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e | I'm outta here | 62   | [["relay","ALL_RELAYS"]] | 1728905470 |
	| ff1092c354d94060a185f8b5e4349499079872babe27b882fd4632efcdd001c2 | Hello          | 1    |                          | 1728905459 |
	| f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd | Hello Later    | 1    |                          | 1728905480 |
	Then Alice receives messages
	| Type | EventId                                                          | Success |
	| OK   | ff1092c354d94060a185f8b5e4349499079872babe27b882fd4632efcdd001c2 | true    |
	| OK   | 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e | true    |
	| OK   | ff1092c354d94060a185f8b5e4349499079872babe27b882fd4632efcdd001c2 | false   |
	| OK   | f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd | true    |

Scenario: Deleting Request to Vanish is rejected
	Publishing a deletion request event (Kind 5) against a request to vanish has no effect. 
	Clients and relays are not obliged to support "unrequest vanish" functionality.
	When Alice publishes events
	| Id                                                               | Content        | Kind | Tags                                                                        | CreatedAt  |
	| 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e | I'm outta here | 62   | [["relay","ALL_RELAYS"]]                                                    | 1728905470 |
	| bb8db141cc129fd5fbc792f871bca9f14a04cfb80607feacd19698b4a7dd878a |                | 5    | [["e", "9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e"]] | 1728905471 |
	Then Alice receives messages
	| Type | EventId                                                          | Success |
	| OK   | 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e | true    |
	| OK   | bb8db141cc129fd5fbc792f871bca9f14a04cfb80607feacd19698b4a7dd878a | false   |

Scenario: Older Request to Vanish does nothing, newer deletes newer events
	First vanish request works as expected. 
	Second (older) one should be ignored and old events should still be rejetected.
	Third (newer) is accepted and its CreatedAt is used to reject old events.
	Newer events are still accepted.
	When Alice publishes events
	| Id                                                               | Content               | Kind | Tags                     | CreatedAt  |
	| ff1092c354d94060a185f8b5e4349499079872babe27b882fd4632efcdd001c2 | Hello                 | 1    |                          | 1728905459 |
	| f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd | Hello Later           | 1    |                          | 1728905480 |
	| 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e | I'm outta here        | 62   | [["relay","ALL_RELAYS"]] | 1728905470 |
	| 2f965ea6c9d085a2c0a55b90e6b38ba8d3f64cc022bd0117fc529037bce93cc9 | I'm outta here sooner | 62   | [["relay","ALL_RELAYS"]] | 1728905460 |
	| 8ac0adbfb1340ac100e13f756dcd47e1ac23b84264147924c854351b8ddd1173 | Hello                 | 1    |                          | 1728905465 |
	| e2ccbd594526fe5c81144dc9d0ed1164757e21da3b6ce82486fa4bba81a86590 | I'm outta here later  | 62   | [["relay","ALL_RELAYS"]] | 1728905490 |
	| f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd | Hello Later           | 1    |                          | 1728905480 |
	| e4262ef3899cb75be630c2940897226d8dca15e81cc4588ed812c86e8bcdabbc | Hello                 | 1    |                          | 1728905495 |
	Then Alice receives messages
	| Type | EventId                                                          | Success |
	| OK   | ff1092c354d94060a185f8b5e4349499079872babe27b882fd4632efcdd001c2 | true    |
	| OK   | f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd | true    |
	| OK   | 9766e0efe45ecd90c508e66a8dd3eee3a7f16be33af87aded9fc779f40237d0e | true    |
	| OK   | 2f965ea6c9d085a2c0a55b90e6b38ba8d3f64cc022bd0117fc529037bce93cc9 | false   |
	| OK   | 8ac0adbfb1340ac100e13f756dcd47e1ac23b84264147924c854351b8ddd1173 | false   |
	| OK   | e2ccbd594526fe5c81144dc9d0ed1164757e21da3b6ce82486fa4bba81a86590 | true    |
	| OK   | f45c291b8c4e3a164e68932f251e50b4182f4dfe2eca76081a7ca2d759568dfd | false   |
	| OK   | e4262ef3899cb75be630c2940897226d8dca15e81cc4588ed812c86e8bcdabbc | true    |

Scenario: Request to Vanish is ignored when relay tag doesn't match current relay
	Event is rejected for missing or incorrect relay tag.
	Correct one assumes the connection is on ws://localhost/. Relay should be able to normalize its own URL and the one in tag (e.g. trim ws:// or wss://, trailing / etc)
	When Alice publishes events
	| Id                                                               | Content        | Kind | Tags                          | CreatedAt  |
	| 95a19f740a0415634581033596cdc5596e43a41a9a73bf3775d37d32b6734b72 | I'm outta here | 62   |                               | 1728905470 |
	| 7fbc1941a2a9c07931ad62510283464ff69c8b2a386f47c129a6aecc4e350adc | I'm outta here | 62   | [["relay","blabla"]]          | 1728905470 |
	| 845c4d3df838caaf98e45c06578a2dea7c77d384e43bfc27d239b121e6320020 | I'm outta here | 62   | [["relay","ws://localhost/"]] | 1728905470 |
	Then Alice receives messages
	| Type | EventId                                                          | Success |
	| OK   | 95a19f740a0415634581033596cdc5596e43a41a9a73bf3775d37d32b6734b72 | false   |
	| OK   | 7fbc1941a2a9c07931ad62510283464ff69c8b2a386f47c129a6aecc4e350adc | false   |
	| OK   | 845c4d3df838caaf98e45c06578a2dea7c77d384e43bfc27d239b121e6320020 | true    |