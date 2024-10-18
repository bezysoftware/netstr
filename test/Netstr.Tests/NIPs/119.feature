Feature: NIP-119
	Enable AND within a single tag filter by using an & modifier in filters for indexable tags.

Background: 
	Given a relay is running
	And Alice is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75 | 512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02 |
	And Bob is connected to relay
	| PublicKey                                                        | PrivateKey                                                       |
	| 5bc683a5d12133a96ac5502c15fe1c2287986cff7baf6283600360e6bb01f627 | 3551fc7617f76632e4542992c0bc01fecb224de639c4b6a1e0956946e8bb8a29 |

Scenario: Tag filter with & is treated as AND
	Alice asks for events tagged with both "meme" AND "cat" that have the tag "black" OR "white"
	When Bob publishes events
	| Id                                                               | Content  | Kind | Tags                                          | CreatedAt  |
	| 828a22e778269e7ba35ae7fa8b23d9506561700f176677f7a8dc7858282f4be3 | Cute cat | 1    | [["t", "meme"], ["t", "cat"], ["t", "black"]] | 1722337838 |
	| d711c1bdaf9fc9aa9a1b91580d98991531e95d22870817ba122d248b4151fde8 | Cute dog | 1    | [["t", "meme"], ["t", "dog"], ["t", "black"]] | 1722337838 |
	And Alice sends a subscription request moarcats
	| Kinds | &t       | #t          |
	| 1     | meme,cat | black,white |
	And Bob publishes an event
	| Id                                                               | Content  | Kind | Tags                                          | CreatedAt  |
	| dad216b3cebb2754fcef13dfd6299879cd2b4cb7988e38e36bc01874c90fab47 | Cute cat | 1    | [["t", "meme"], ["t", "cat"], ["t", "white"]] | 1722337840 |
	| a88cc99d717189d32aa5361386a0654a7b5a0c99f52e1377821bcf5302f64c76 | Cute dog | 1    | [["t", "meme"], ["t", "dog"], ["t", "white"]] | 1722337840 |
	Then Alice receives messages
 	| Type  | Id       | EventId                                                          |
 	| EVENT | moarcats | 828a22e778269e7ba35ae7fa8b23d9506561700f176677f7a8dc7858282f4be3 |
 	| EOSE  | moarcats |                                                                  |
 	| EVENT | moarcats | dad216b3cebb2754fcef13dfd6299879cd2b4cb7988e38e36bc01874c90fab47 |