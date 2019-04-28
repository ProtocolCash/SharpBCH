using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpBCH.Util;

namespace SharpBCH.SLP
{
    public class TokenType1Message : Token.SLPMessage
    {
        public TokenType1Message(TokenType1.TransactionType transactionType)
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
            }

            public GenesisMessage(List<byte[]> dataChunks) : this(dataChunks[4], dataChunks[5],
            dataChunks[6], dataChunks[7], dataChunks[8][0],
            dataChunks[9][0], BitConverter.ToUInt64(dataChunks[10]))
            {
                DataChunks = dataChunks;
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
            public MintMessage(byte[] tokenId, byte mintBatonVOut, ulong additionalTokenQuantity) : base(TokenType1
                .TransactionType.MINT)
            {
                TokenId = tokenId;
                MintBatonVOut = mintBatonVOut;
                AdditionalTokenQuantity = additionalTokenQuantity;
            }

            public MintMessage(List<byte[]> dataChunks) : this(dataChunks[3], dataChunks[4].Length == 0 ? (byte) 0 : dataChunks[4][0], BitConverter.ToUInt64(dataChunks[5]))
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
            public SendMessage(byte[] tokenId, byte[][] tokenOutputQuantities) : base(TokenType1.TransactionType.SEND)
            {
                TokenId = tokenId;
                TokenOutputQuantities = tokenOutputQuantities;
            }

            public SendMessage(List<byte[]> dataChunks) : this(dataChunks[3], dataChunks.Skip(3).ToArray())
            {
                DataChunks = dataChunks;
            }

            public byte[] TokenId { get; }
            public string TokenIdHex => ByteHexConverter.ByteArrayToHex(TokenId);
            public byte[][] TokenOutputQuantities { get; }
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