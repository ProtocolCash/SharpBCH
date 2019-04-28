using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpBCH.Util;

namespace SharpBCH.SLP
{
    public class TokenType1Message : Token.SLPMessage
    {
        public override string ToString()
        {
            return base.ToString() + " TokenType1";
        }

        protected static readonly List<byte[]> DefaultDataHeader = new List<byte[]>
        {
            "SLP\0".Select(x => (byte) x).ToArray(), // 534c5000 in hex
            new byte[] {1}
        };

        protected TokenType1Message(TokenType1.TransactionType transactionType) : base(DefaultDataHeader)
        {
            TransactionType = transactionType;
        }

        public TokenType1.TransactionType TransactionType { get; }

        public class GenesisMessage : TokenType1Message
        {
            public GenesisMessage(byte[] tokenTickerBytes, byte[] tokenNameBytes, byte[] tokenDocumentUrlBytes,
                byte[] tokenDocumentHash, byte decimals, byte mintBatonVOut, ulong initialTokenMintQuantity) : base(
                TokenType1.TransactionType.GENESIS)
            {
                TokenTickerBytes = tokenTickerBytes;
                TokenNameBytes = tokenNameBytes;
                TokenDocumentUrlBytes = tokenDocumentUrlBytes;
                TokenDocumentHash = tokenDocumentHash;
                Decimals = decimals;
                MintBatonVOut = mintBatonVOut;
                InitialTokenMintQuantity = initialTokenMintQuantity;

                DataChunks.AddRange(new List<byte[]>
                {
                    "GENESIS".Select(x => (byte) x).ToArray(),
                    tokenTickerBytes,
                    tokenNameBytes,
                    tokenDocumentUrlBytes,
                    tokenDocumentHash,
                    new[] { decimals },
                    new[] { mintBatonVOut },
                    BitConverter.GetBytes(initialTokenMintQuantity).Reverse().ToArray()
                });
            }

            public GenesisMessage(List<byte[]> dataChunks) : this(dataChunks[3], dataChunks[4],
            dataChunks[5], dataChunks[6], dataChunks[7][0],
            dataChunks[8][0], BitConverter.ToUInt64(dataChunks[9].Reverse().ToArray()))
            {
                DataChunks = dataChunks;
            }

            public override string ToString()
            {
                return base.ToString() + " GENESIS TokenTicker: " + TokenTicker + "; TokenName: " + TokenName + "; TokenDocumentUrl: " +
                       TokenDocumentUrl + "; TokenDocumentHash: " + TokenDocumentHashHex + "; Decimals: " + Decimals +
                       "; MintBatonVOut: " + MintBatonVOut + "; InitialTokenMintQuantity: " + InitialTokenMintQuantity;
            }

            public string TokenTicker => Encoding.ASCII.GetString(TokenTickerBytes);
            public byte[] TokenTickerBytes { get; }
            public string TokenName => Encoding.ASCII.GetString(TokenNameBytes);
            public byte[] TokenNameBytes { get; }
            public string TokenDocumentUrl => Encoding.ASCII.GetString(TokenDocumentUrlBytes);
            public byte[] TokenDocumentUrlBytes { get; }
            public string TokenDocumentHashHex => ByteHexConverter.ByteArrayToHex(TokenDocumentHash);
            public byte[] TokenDocumentHash { get; }
            public byte Decimals { get; }
            public byte MintBatonVOut { get; }
            public ulong InitialTokenMintQuantity { get; }
        }

        public class MintMessage : TokenType1Message
        {
            public MintMessage(byte[] tokenId, byte mintBatonVOut, ulong additionalTokenQuantity) : base(TokenType1.TransactionType.MINT)
            {
                TokenId = tokenId;
                MintBatonVOut = mintBatonVOut;
                AdditionalTokenQuantity = additionalTokenQuantity;

                DataChunks.AddRange(new List<byte[]>
                {
                    "MINT".Select(x => (byte) x).ToArray(),
                    tokenId,
                    new[] { mintBatonVOut },
                    BitConverter.GetBytes(additionalTokenQuantity).Reverse().ToArray()
                });
            }

            public override string ToString()
            {
                return base.ToString() + " MINT TokenId: " + TokenIdHex + "; MintBatonVOut: " + MintBatonVOut +
                       " AdditionalTokenQuantity: " + AdditionalTokenQuantity;
            }

            public MintMessage(List<byte[]> dataChunks) : this(dataChunks[3], dataChunks[4].Length == 0 ? (byte) 0 : dataChunks[4][0], BitConverter.ToUInt64(dataChunks[5].Reverse().ToArray()))
            {
                DataChunks = dataChunks;
            }

            public byte[] TokenId { get; }
            public string TokenIdHex => ByteHexConverter.ByteArrayToHex(TokenId);
            public byte MintBatonVOut { get; }
            public ulong AdditionalTokenQuantity { get; }
        }

        public class SendMessage : TokenType1Message
        {
            public SendMessage(byte[] tokenId, ulong[] tokenOutputQuantities) : base(TokenType1.TransactionType.SEND)
            {
                TokenId = tokenId;
                TokenOutputQuantities = tokenOutputQuantities;

                DataChunks.Add("SEND".Select(x => (byte)x).ToArray());
                DataChunks.Add(tokenId);
                DataChunks.AddRange(TokenOutputQuantityBytes);
            }

            public override string ToString()
            {
                return base.ToString() + " SEND TokenId: " + TokenIdHex + "; TokenOutputQuantities (" + TokenOutputQuantities.Length +
                       "): [" + string.Join(", ", TokenOutputQuantities.ToList()) + "]";
            }

            public SendMessage(List<byte[]> dataChunks) : this(dataChunks[3], dataChunks.Skip(4).ToArray().Select(x => BitConverter.ToUInt64(x.Reverse().ToArray())).ToArray())
            {
                DataChunks = dataChunks;
            }

            public byte[] TokenId { get; }
            public string TokenIdHex => ByteHexConverter.ByteArrayToHex(TokenId);
            public byte[][] TokenOutputQuantityBytes => TokenOutputQuantities.Select(x => BitConverter.GetBytes(x).Reverse().ToArray()).ToArray();
            public ulong[] TokenOutputQuantities { get; }
        }

        public class CommitMessage : TokenType1Message
        {
            public CommitMessage(byte[] tokenId, byte[] forBitcoinBlockHash, ulong forBitcoinBlockHeight,
                byte[] tokenTransactionSetHash,
                byte[] transactionSetDocumentUrlBytes) : base(TokenType1.TransactionType.COMMIT)
            {
                TokenId = tokenId;
                ForBitcoinBlockHash = forBitcoinBlockHash;
                ForBitcoinBlockHeight = forBitcoinBlockHeight;
                TokenTransactionSetHash = tokenTransactionSetHash;
                TransactionSetDocumentUrlBytes = transactionSetDocumentUrlBytes;

                DataChunks.AddRange(new List<byte[]>
                {
                    "COMMIT".Select(x => (byte) x).ToArray(),
                    tokenId,
                    forBitcoinBlockHash,
                    BitConverter.GetBytes(forBitcoinBlockHeight).Reverse().ToArray(),
                    tokenTransactionSetHash,
                    transactionSetDocumentUrlBytes
                });
            }

            public override string ToString()
            {
                return base.ToString() + " COMMIT TokenId: " + TokenIdHex + "; ForBitcoinBlockHash: " + ForBitcoinBlockHashHex +
                       "; ForBitcoinBlockHeight: " + ForBitcoinBlockHeight + "; TokenTransactionSetHash: " +
                       TokenTransactionSetHashHex + "; TransactionSetDocumentUrl: " + TransactionSetDocumentUrl;
            }

            public CommitMessage(List<byte[]> dataChunks) : this(dataChunks[3], dataChunks[4],
                BitConverter.ToUInt64(dataChunks[5]), dataChunks[6], dataChunks[7])
            {
                DataChunks = dataChunks;
            }

            public byte[] TokenId { get; }
            public string TokenIdHex => ByteHexConverter.ByteArrayToHex(TokenId);
            public byte[] ForBitcoinBlockHash { get; }
            public string ForBitcoinBlockHashHex => ByteHexConverter.ByteArrayToHex(ForBitcoinBlockHash);
            public ulong ForBitcoinBlockHeight { get; }
            public byte[] TokenTransactionSetHash { get; }
            public string TokenTransactionSetHashHex => ByteHexConverter.ByteArrayToHex(TokenTransactionSetHash);
            public string TransactionSetDocumentUrl => Encoding.ASCII.GetString(TransactionSetDocumentUrlBytes);
            public byte[] TransactionSetDocumentUrlBytes { get; }
        }
    }
}