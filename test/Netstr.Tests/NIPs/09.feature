Feature: NIP-09
	A special event with kind 5, meaning "deletion" is defined as having a list of one or more e or a tags, 
	each referencing an event the author is requesting to be deleted.

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

Scenario: Deletion removes referenced regular events and is itself broadcast
	Deletion event can contain multiple "e" tags referencing known and unknown events
	When Alice publishes events
	| Id                                                               | Content | Kind | Tags                                                                                                                                                   | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |                                                                                                                                                        | 1722337838 |
	| 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 | Later   | 1    |                                                                                                                                                        | 1722337848 |
	| 04c4ee3333f6f4c59ee5d476e5c86d77922976ea0134c5e19eae665324f735c7 |         | 5    | [["e", "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5"], ["e", "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"]] | 1722337845 |
	And Bob sends a subscription request abcd
	| Authors                                                          |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 |
	Then Bob receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 |
	| EVENT | abcd | 04c4ee3333f6f4c59ee5d476e5c86d77922976ea0134c5e19eae665324f735c7 |
	| EOSE  | abcd |                                                                  |


Scenario: Deletion removes referenced replaceable events and is itself broadcast
	Deletion event can contain "a" tags referencing replaceable or addressable events,
	but only those which took place before the deletion event.
	If a newer event arives after it was previously deleted, it is saved.
	If a newer event which was created before the deleted event arrives, it is ignored.
	When Bob publishes events
	| Id                                                               | Kind  | Tags                                                                                | CreatedAt  |
	| af3224801d0ea862ceb45e3d75998373ff8726541f133dd0bc5badc79c832e88 | 0     |                                                                                     | 1722337838 |
	| 37b30f773a1a7ba1615f34482194a531eca4b3a353e7c73a8f0e08985f6a09e4 | 10000 |                                                                                     | 1722337840 |
	| a23d28af8e9395478f297bd649d71a80b3d6c6c2af2c1dc1c9036ac4f451263d | 30000 | [[ "d", "a" ]]                                                                      | 1722337835 |
	| 8a75f74fe8798771c98c4c17b847f95e7ef28c7822b57e399bca41dc911f8baf | 30000 | [[ "d", "b" ]]                                                                      | 1722337840 |
	| dd593bc09c98e958eab2414912ad097df6efdef8b99768915d2361aac4c4ceac | 5     | [["a", "0:5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:"]]      | 1722337839 |
	| fa740ac70b991cd3955945d9799d881cd15971f37bf71902f271b00c6aa8f7f7 | 5     | [["a", "10000:5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:"]]  | 1722337839 |
	| 8f1dbc29af4b5c96c26ee5c8932409017a1af538dbbf5207d1dc6470b488580e | 5     | [["a", "30000:5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:a"]] | 1722337839 |
	| b74adc27515ad9fa78a86acfbc03375b1ab8fc63822c826cad7564b7d23c8051 | 5     | [["a", "30000:5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:b"]] | 1722337839 |
	| 4a2a7d1fe9ea53ba1604eab98523f26eaee750a86983aa5fbe86614f9c5a2318 | 30000 | [[ "d", "a" ]]                                                                      | 1722337836 |
	And Alice sends a subscription request abcd
	| Authors                                                          |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	Then Alice receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 37b30f773a1a7ba1615f34482194a531eca4b3a353e7c73a8f0e08985f6a09e4 |
	| EVENT | abcd | 8a75f74fe8798771c98c4c17b847f95e7ef28c7822b57e399bca41dc911f8baf |
	| EVENT | abcd | 8f1dbc29af4b5c96c26ee5c8932409017a1af538dbbf5207d1dc6470b488580e |
	| EVENT | abcd | b74adc27515ad9fa78a86acfbc03375b1ab8fc63822c826cad7564b7d23c8051 |
	| EVENT | abcd | dd593bc09c98e958eab2414912ad097df6efdef8b99768915d2361aac4c4ceac |
	| EVENT | abcd | fa740ac70b991cd3955945d9799d881cd15971f37bf71902f271b00c6aa8f7f7 |
	| EOSE  | abcd |                                                                  |
	
Scenario: It's not allowed to delete someone else's events
	Deletion event might reference someone else's events, those shouldn't be deleted
	If the deletion references other events which belong to the author, those should be deleted
	This also verifies that multi deletion events where even a single deletion fails (e.g. wrong Author) then the whole deletion fails
	When Alice publishes events
	| Id                                                               | Content | Kind  | Tags         | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1     |              | 1722337838 |
	| 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 | Later   | 1     |              | 1722337848 |
	| da4e33af3793fd4f9d5487a116ee1a03142599e9b1115af38838e469473a8c6b | Tags    | 30000 | [["d", "a"]] | 1722337848 |
	And Bob publishes events
	| Id                                                               | Content | Kind  | Tags                                                                                                                                                  | CreatedAt  |
	| a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | Hello 1 | 1     |                                                                                                                                                       | 1722337838 |
	| 3abeb55eb9e6a58acf06269f5e93dabd4c91d1e51d08beeab884917180b9248f | Tags    | 30000 | [["d", "a"]]                                                                                                                                          | 1722337848 |
	| 06f7797468cf1fde45dc438288d44418f416302e94dba22e31b8ef60b74f44bc |         | 5     | [["e", "a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346"],["e", "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5"]] | 1722337845 |
	| b644d0e9b646df95eee0fba09fd7b742df1a6c878ae752112639302ef0aa2da1 |         | 5     | [["e", "a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346"]]                                                                           | 1722337845 |
	| 9b061a1d369cae854f8d518f0cedceb7ea0169cf9736a92e5362b0535dfa96fb |         | 5     | [["a", "30000:5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:a"]]                                                                   | 1722337849 |
	And Charlie sends a subscription request abcd
	| Authors                                                                                                                           |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75,5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	Then Charlie receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 9b061a1d369cae854f8d518f0cedceb7ea0169cf9736a92e5362b0535dfa96fb |
	| EVENT | abcd | 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 |
	| EVENT | abcd | da4e33af3793fd4f9d5487a116ee1a03142599e9b1115af38838e469473a8c6b |
	| EVENT | abcd | b644d0e9b646df95eee0fba09fd7b742df1a6c878ae752112639302ef0aa2da1 |
	| EVENT | abcd | 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 |
	| EOSE  | abcd |                                                                  |
	And Bob receives messages
	| Type | Id                                                               | Success |
	| OK   | a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | true    |
	| OK   | 3abeb55eb9e6a58acf06269f5e93dabd4c91d1e51d08beeab884917180b9248f | true    |
	| OK   | 06f7797468cf1fde45dc438288d44418f416302e94dba22e31b8ef60b74f44bc | false   |
	| OK   | b644d0e9b646df95eee0fba09fd7b742df1a6c878ae752112639302ef0aa2da1 | true    |
	| OK   | 9b061a1d369cae854f8d518f0cedceb7ea0169cf9736a92e5362b0535dfa96fb | true    |

Scenario: Deleting a deletion has no affect
	Clients and relays are not obliged to support "undelete" functionality
	When Alice publishes events
	| Id                                                               | Content | Kind | Tags                                                                        | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |                                                                             | 1722337838 |
	| 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 | Later   | 1    |                                                                             | 1722337848 |
	| 367ca4fcb31777b20fffc7057ca10e3f251322022b57fc4c123ecbf423f3b529 |         | 5    | [["e", "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5"]] | 1722337845 |
	| 254ab6e975fc906256f9f318e50c450cd745745031459bddb027c655124302a7 |         | 5    | [["e", "367ca4fcb31777b20fffc7057ca10e3f251322022b57fc4c123ecbf423f3b529"]] | 1722337845 |
	And Charlie sends a subscription request abcd
	| Authors                                                                                                                           |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75,5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	Then Charlie receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 |
	| EVENT | abcd | 367ca4fcb31777b20fffc7057ca10e3f251322022b57fc4c123ecbf423f3b529 |
	| EOSE  | abcd |                                                                  |

Scenario: Resubmission of deleted event is rejected
	When Alice publishes events
	| Id                                                               | Content | Kind | Tags                                                                        | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |                                                                             | 1722337838 |
	| 367ca4fcb31777b20fffc7057ca10e3f251322022b57fc4c123ecbf423f3b529 |         | 5    | [["e", "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5"]] | 1722337845 |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |                                                                             | 1722337838 |
	And Bob sends a subscription request abcd
	| Authors                                                          | Kinds |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 1     |
	Then Bob receives messages
	| Type | Id   | 
	| EOSE | abcd |
	And Alice receives messages
	| Type | Id                                                               | Success |
	| OK   | 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | true    |
	| OK   | 367ca4fcb31777b20fffc7057ca10e3f251322022b57fc4c123ecbf423f3b529 | true    |
	| OK   | 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | false   |