# SharpBCH

DotNET Core Bitcoin BCH C# Library!

Implements Block and Transaction Decoding; Cash Address and Bitcoin Script encoding and decoding; node connectivity via ZMQ, JSON/RPC, and REST interfaces.

## Early Development Warning

Changes may occur rapid. Additional functionality will be addied. Reasonable effort will be made to keep public functions and constructors consistent in required arguments and return values; however, at this stage this is not guaranteed!

# Getting Started

You can pull in SharpBCH to your project for any number of reasons.

Presuming you already have a node, there isn't even any sync time required! Edit bitcoin.conf to include zmqpubrawtx and zmqpubrawblock, setup BCHSocket's app.config to match, then just start BCHSocket!

## New Types

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
- Docs TODO

## Block Decoding
Namespace: SharpBCH.Block
- Docs TODO

## Node Connections
Namespace: SharpBCH.Node
- Docs TODO

## Development Environment and Tools
SharpBCH can be built under any DotNET Core 2.2 environment.

Recommended Development Environment:
- Visual Studio 2017 Community Edition https://visualstudio.microsoft.com/downloads/
- JetBrains Resharper https://www.jetbrains.com/resharper/
- DotNET Core 2.2 https://dotnet.microsoft.com/download/dotnet-core/2.2

## Dependencies

SharpBCH (https://github.com/ProtocolCash/SharpBCH) aims to keep the dependency tree minimal - as much as is reasonable is implemented directly in the library.

SharpBCH depends on:
- NETCore 2.2.0
- NetMQ 4.0.0.1 
- - https://www.nuget.org/packages/NetMQ/4.0.0.1 
- - https://github.com/zeromq/netmq
- Newtonsoft.Json 12.0.1 
- - https://www.nuget.org/packages/Newtonsoft.Json/ 
- - https://github.com/JamesNK/Newtonsoft.Json
