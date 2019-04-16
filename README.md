
# SharpBCH

DotNET Core Bitcoin BCH C# Library!

Implements Block and Transaction Decoding; Cash Address and Bitcoin Script encoding and decoding; node connectivity via ZMQ, JSON/RPC, and REST interfaces.

## Early Development Warning

Rapid changes may occur. Additional functionality will be added. Reasonable effort will be made to keep public functions and constructors consistent in required arguments and return values; however, at this stage this is not guaranteed!

## Development Environment and Tools
SharpBCH can be built under any DotNET Core 2.2 environment.

Recommended Development Environment:
- Visual Studio 2017 Community Edition https://visualstudio.microsoft.com/downloads/
- JetBrains Resharper https://www.jetbrains.com/resharper/
- DotNET Core 2.2 https://dotnet.microsoft.com/download/dotnet-core/2.2

## Dependencies
SharpBCH (https://github.com/ProtocolCash/SharpBCH) aims to keep the dependency tree minimal - as much as is reasonable is implemented directly in the library.

SharpBCH depends on:
- DotNET Core 2.2.0
- NetMQ 4.0.0.1 
- - https://www.nuget.org/packages/NetMQ/4.0.0.1 
- - https://github.com/zeromq/netmq
- Newtonsoft.Json 12.0.1 
- - https://www.nuget.org/packages/Newtonsoft.Json/ 
- - https://github.com/JamesNK/Newtonsoft.Json

# Getting Started

You may want to use SharpBCH in your project to:
- Decode Bitcoin Blocks and Transacitons
- Encode and Decode Bitcoin Scripts and Cash Addresses
- Interact with a node for full blocks or specific transactions

Public interface index:

[CashAddress](#cashaddress-encoding-and-decoding)
[Bitcoin Script](#bitcoin-script-encoding-and-decoding)
[Transaction](#transaction-decoding)
[Block](#block-decoding)
[Node](#node-connections)

## Special Return and Input Types

A few enums and output classes/interfaces:

| Type | Description | Values |
| -- | -- | -- |
| AddressPrefix | Cash Address network prefix | bitcoincash, bchtest, bchreg |
| ScriptType | Cash Address script type version numbers | Encoding and Decoding: P2PKH = 0x0, P2SH = 0x8; Decoding only: DATA = -1, OTHER = -2. |
| DecodedBitcoinAddress | A decoded address, represented as raw byte data, script type, and network prefix | See public: string Prefix, ScriptType Type, byte[] Hash |


## CashAddress Encoding and Decoding

Namespace: SharpBCH.CashAddress
- CashAddress (Static Class):
	- EncodeCashAddress(AddressPrefix prefix, ScriptType scriptType, byte[] hash160)
		- prefix: Cash Address prefix
		- scriptType: Bitcoin script type
		- hash160: Byte data from bitcoin script (usually hash160)
		- \[return\]: \(string\) Cash Address formatted bitcoin address
	- DecodedBitcoinAddress DecodeCashAddress(string address)
		- address: Cash Address formatted address
		- \[return\]: \(DecodedBitcoinAddress\) prefix, scriptType, hash160 bytes
- CashAddressException:
	- Public functions throw on errors. See InternalException for failure reason.

## Bitcoin Script Encoding and Decoding
Namespace: SharpBCH.Script
- Script (Constructable Class):
	- ToString():
		- [return] (string) Human readable script with written op_codes and data chunks expressed as hex
	- Script(IEnumerable\<byte\> script):
		- script: raw bitcoin script as byte array
	- ScriptBytes
		- (byte[]) byte array of the script
	- OpCodes
		- (List\<OpCodeType\>) Ordered list of script op_codes. Data pushes are removed and replaced with internal OP_DATA.
	- DataChunks
		- (List<byte[]>) Raw data chunks in the script. Each OP_DATA in OpCodes will have an matching DataChunk in the same sequence.
- ScriptBuilder (Static Class):
	- CreateOutputScript(string cashAddress):
		- cashAddress: Cash Address to create an output script to pay
		- [return] (Script) Bitcoin Output Script to spend to the given Cash Address
	- CreateOutputScript(ScriptType scriptType, byte[] hash160):
		- scriptType: type of hash
		- hash160: public key hash or script hash
		- [return] (Script) Bitcoin Output Script to spend to the given scriptType and hash
	- CreateOpReturn(byte[] data):
		- data: data to include in the op_return
		- [return] (Script) Bitcoin OP_RETURN Output Script containing the given data
	- GetOpPushForLength(uint dataLength):
		- dataLength: length of data to push to script stack
		- [return] (byte[]) Script/Opcode bytes needed to push data of a given length
- BitcoinScriptBuilderException:
	-  Public functions throw on errors. See InternalException for failure reason.

## Transaction Decoding
Namespace: SharpBCH.Transaction
- Transaction (Constructable Class):
	- Transaction(IEnumerable\<byte\> txBytes)
		- txBytes: raw transaction as byte array
	- Inputs
		- (Input[]) all inputs in the transaction
	- Outputs
		- (Output[]) all outputs in the transaction
	- TXIDHex
		- (string) transaction ID in hex
	- TXVersion
		- (uint) transaction version number
	- LockTime
		- (uint) block locktime
- Input (Constructable Class):
	- Hash
		- (byte[]) hash of the transaction that created the output being redeemed 
	- Index
		- (uint) index of output for redeemed utxo in the previous transaction
	- Script
		- (byte[]) raw input script
- Output (Constructable Class):
	- Value
		- (ulong) value spent to the output
	- Type
		- (ScriptType) type of output, if known (or -2 for unknown)
	- Address
		- (string) cash address to which the output spends (or empty if Type is not P2PKH or P2SH)
	- Script
		- (byte[]) raw output script

## Block Decoding
Namespace: SharpBCH.Block
- Block (Constructable Class):
	- public BlockHeader(IEnumerable<\byte\> headerBytes) : base(headerBytes):
		- 	- 
	- Block(IEnumerable\<byte\> blockBytes)
		- blockBytes: >raw block of block as byte array
	- Header
		- (BlockHeader) See Below
	- Transactions
		- (Transaction[]) See Above
	- BlockSize
		- (uint) size of block in bytes
	- BlockHash
		- (string) blockhash in hex
- BlockHeader (Constructable Class):
	- BlockHeader(IEnumerable<\byte\> headerBytes)
		- headerBytes: raw block header as byte array
	- BlockVersion (uint)
	- PrevBlockHash (string)
	- MerkleRootHash (byte[])
	- TimeStamp (uint)
	- DiffTarget (uint)
	- Nonce (uint)
	- BlockHashHex:
		- (string) double sha of header in hex
- BlockException
	-  Public functions of this namespace throw on errors. See InternalException for failure reason.
## Node Connections
Namespace: SharpBCH.Node
- TODO
