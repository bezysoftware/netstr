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
	| 9ead3323b0e26292d96b01bd6ed24cfea1973eceea878ad19c8d56e8916c625d | 5     | [["a", "5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:0:"]]      | 1722337839 |
	| 753a8fc529a44a82097d54012fd7a5c8d5d201cdb41ef1f77e3a7ff8cf70ab36 | 5     | [["a", "5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:10000:"]]  | 1722337839 |
	| 6101fb0b269a4e3c0bfa79c853c07c081fbbcc9ed562a93359ad443a757183c4 | 5     | [["a", "5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:30000:a"]] | 1722337839 |
	| 5842861d68412d6fd994e7e11b1ddac08bfd84fdd4e82100b14c2dbc41739308 | 5     | [["a", "5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627:30000:b"]] | 1722337839 |
	| 4a2a7d1fe9ea53ba1604eab98523f26eaee750a86983aa5fbe86614f9c5a2318 | 30000 | [[ "d", "a" ]]                                                                      | 1722337836 |
	And Alice sends a subscription request abcd
	| Authors                                                          |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	Then Alice receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 37b30f773a1a7ba1615f34482194a531eca4b3a353e7c73a8f0e08985f6a09e4 |
	| EVENT | abcd | 8a75f74fe8798771c98c4c17b847f95e7ef28c7822b57e399bca41dc911f8baf |
	| EVENT | abcd | 5842861d68412d6fd994e7e11b1ddac08bfd84fdd4e82100b14c2dbc41739308 |
	| EVENT | abcd | 6101fb0b269a4e3c0bfa79c853c07c081fbbcc9ed562a93359ad443a757183c4 |
	| EVENT | abcd | 753a8fc529a44a82097d54012fd7a5c8d5d201cdb41ef1f77e3a7ff8cf70ab36 |
	| EVENT | abcd | 9ead3323b0e26292d96b01bd6ed24cfea1973eceea878ad19c8d56e8916c625d |
	| EOSE  | abcd |                                                                  |
	
Scenario: It's not allowed to delete someone else's events
	Deletion event might reference someone else's events, those shouldn't be deleted
	If the deletion references other events which belong to the author, those should be deleted
	When Alice publishes events
	| Id                                                               | Content | Kind | Tags | CreatedAt  |
	| 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 | Hello   | 1    |      | 1722337838 |
	| 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 | Later   | 1    |      | 1722337848 |
	And Bob publishes an event
	| Id                                                               | Content | Kind | Tags                                                                                                                                                  | CreatedAt  |
	| a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346 | Hello 1 | 1    |                                                                                                                                                       | 1722337838 |
	| 06f7797468cf1fde45dc438288d44418f416302e94dba22e31b8ef60b74f44bc |         | 5    | [["e", "a6d166e834e78827af0770f31f15b13a772f281ad880f43ce12c24d4e3d0e346"],["e", "8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5"]] | 1722337845 |
	And Charlie sends a subscription request abcd
	| Authors                                                                                                                           |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75,5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 |
	Then Charlie receives messages
	| Type  | Id   | EventId                                                          |
	| EVENT | abcd | 86aa1ac011362326d5fdda20645fffb9de853b5c315143ea3d4df0bcb6dec927 |
	| EVENT | abcd | 06f7797468cf1fde45dc438288d44418f416302e94dba22e31b8ef60b74f44bc |
	| EVENT | abcd | 8ed8cc390eaf6db9e0ae8f3bf720a80d81ae49f95f953a9a4e26a72dc7f4a2c5 |
	| EOSE  | abcd |                                                                  |
	
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
	| EVENT | abcd | 254ab6e975fc906256f9f318e50c450cd745745031459bddb027c655124302a7 |
	| EVENT | abcd | 367ca4fcb31777b20fffc7057ca10e3f251322022b57fc4c123ecbf423f3b529 |
	| EOSE  | abcd |                                                                  |