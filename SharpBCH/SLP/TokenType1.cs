using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpBCH.Script;

namespace SharpBCH.SLP
{
    public class TokenType1 : Token
    {
        public enum TransactionType
        {
            GENESIS,
            MINT,
            SEND,
            COMMIT
        }

        private IEnumerable<byte> Combine(IEnumerable<byte> a1, IEnumerable<byte> a2)
        {
            foreach (var b in a1)
                yield return b;
            foreach (var b in a2)
                yield return b;
        }

        public Script.Script CreateCommitScript(byte[] tokenId, byte[] forBitcoinBlockHash, ulong blockHeight,
            byte[] tokenTransactionMerkleRoot, string tokenTransactionSetUrl)
        {
            if (tokenId.Length != 32)
                throw new ArgumentException(
                    "Invalid tokenId! Expected 32 bytes, received " + tokenId.Length + " bytes.");
            if (forBitcoinBlockHash.Length != 32)
                throw new ArgumentException("Invalid forBitcoinBlockHash! Expected 32 bytes, received " +
                                            forBitcoinBlockHash.Length + " bytes.");
            if (tokenTransactionMerkleRoot.Length != 32)
                throw new ArgumentException("Invalid tokenTransactionMerkleRoot! Expected 32 bytes, received " +
                                            tokenTransactionMerkleRoot.Length + " bytes.");

            // header
            var byteData = GetHeaderBytes();
            /* After Header:
                <transaction_type: 'COMMIT'> (6 bytes, ascii)
                <token_id> (32 bytes)
                <for_bitcoin_block_hash> (32 bytes)
                <block_height> (8 byte integer)
                <token_txn_set_hash> (32 bytes)
                <txn_set_data_url> (0 to ∞ bytes, ascii)
                [to be determined]
             */

            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(5)).Concat("COMMIT".Select(x => (byte) x));
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(32)).Concat(tokenId);
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(32)).Concat(forBitcoinBlockHash);
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(8)).Concat(BitConverter.GetBytes(blockHeight));
            byteData = byteData
                .Concat(ScriptBuilder.GetOpPushForLength((ulong) Encoding.ASCII.GetByteCount(tokenTransactionSetUrl),
                    false)).Concat(Encoding.ASCII.GetBytes(tokenTransactionSetUrl));

            return ScriptBuilder.CreateOpReturn(byteData.ToArray());
        }

        /// <summary>
        ///     Create SLP Token GENESIS Output Script
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="name"></param>
        /// <param name="documentUrl"></param>
        /// <param name="documentHash"></param>
        /// <param name="decimals"></param>
        /// <param name="mintBatonVOut"></param>
        /// <param name="mintQuantity"></param>
        /// <returns></returns>
        public Script.Script CreateGenesisScript(byte[] ticker, byte[] name, byte[] documentUrl, byte[] documentHash,
            byte decimals, byte mintBatonVOut, ulong mintQuantity)
        {
            // header
            var byteData = GetHeaderBytes();
            /* After Header:
                <transaction_type: 'GENESIS'> (4 bytes, ascii)
                <token_ticker> (0 to ∞ bytes, suggested utf-8)
                <token_name> (0 to ∞ bytes, suggested utf-8)
                <token_document_url> (0 to ∞ bytes, suggested ascii)
                <token_document_hash> (0 bytes or 32 bytes)
                <decimals> (1 byte in range 0x00-0x09)
                <mint_baton_vout> (0 bytes, or 1 byte in range 0x02-0xff)
                <initial_token_mint_quantity> (8 byte integer)
            */

            if (documentHash.Length != 0 && documentHash.Length != 32)
                throw new ArgumentException("Invalid TokenDocumentHash! Expected either 0 or 32 bytes, but received " +
                                            documentHash.Length + " bytes.");

            if (0 < decimals || decimals > 9)
                throw new ArgumentException("Invalid digits after decimal! Expected 0-9 inclusive, but received " +
                                            (int) decimals + ".");
            if (mintBatonVOut == 1)
                throw new ArgumentException(
                    "Invalid mintBatonVOut! Expected 0 (create no baton), or 1-255 (vout), but received 1.");

            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(7)).Concat("GENESIS".Select(x => (byte) x));
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength((uint) ticker.Length, false)).Concat(ticker);
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength((uint) name.Length, false)).Concat(name);
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength((uint) documentUrl.Length, false))
                .Concat(documentUrl);
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength((uint) documentHash.Length, false))
                .Concat(documentHash);
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(1)).Concat(new[] {decimals});
            byteData = mintBatonVOut == 0
                ? byteData.Concat(ScriptBuilder.GetOpPushForLength(0, false))
                : byteData.Concat(ScriptBuilder.GetOpPushForLength(1)).Concat(new[] {mintBatonVOut});
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(8)).Concat(BitConverter.GetBytes(mintQuantity));

            return ScriptBuilder.CreateOpReturn(byteData.ToArray());
        }

        /// <summary>
        ///     Create SLP Token MINT Output Script
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="mintBatonVOut"></param>
        /// <param name="mintQuantity"></param>
        /// <returns></returns>
        public Script.Script CreateMintScript(byte[] tokenId, byte mintBatonVOut, ulong mintQuantity)
        {
            if (tokenId.Length != 32)
                throw new ArgumentException(
                    "Invalid tokenId! Expected 32 bytes, received " + tokenId.Length + " bytes.");
            if (mintBatonVOut == 1)
                throw new ArgumentException(
                    "Invalid mintBatonVOut! Expected 0 (create no baton), or 1-255 (vout), but received 1.");

            // header
            var byteData = GetHeaderBytes();
            /* After Header:
                <transaction_type: 'MINT'> (4 bytes, ascii)
                <token_id> (32 bytes)
                <mint_baton_vout> (0 bytes or 1 byte between 0x02-0xff)
                <additional_token_quantity> (8 byte integer) 
             */

            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(4)).Concat("MINT".Select(x => (byte) x));
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(32)).Concat(tokenId);
            byteData = mintBatonVOut == 0
                ? byteData.Concat(ScriptBuilder.GetOpPushForLength(0, false))
                : byteData.Concat(ScriptBuilder.GetOpPushForLength(1)).Concat(new[] {mintBatonVOut});
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(8)).Concat(BitConverter.GetBytes(mintQuantity));

            return ScriptBuilder.CreateOpReturn(byteData.ToArray());
        }

        /// <summary>
        ///     Create SLP Token SEND Output Script
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="tokenOutputQuantities"></param>
        /// <returns></returns>
        public Script.Script CreateSendScript(byte[] tokenId, ulong[] tokenOutputQuantities)
        {
            // header
            var byteData = GetHeaderBytes();

            /* After Header:
               <transaction_type: 'SEND'> (4 bytes, ascii)
               <token_id> (32 bytes)
               <token_output_quantity1> (required, 8 byte integer)
               <token_output_quantity2> (optional, 8 byte integer)
               ...
               <token_output_quantity19> (optional, 8 byte integer)
             *
             */
            if (tokenId.Length != 32)
                throw new ArgumentException(
                    "Invalid tokenId! Expected 32 bytes, received " + tokenId.Length + " bytes.");

            if (tokenOutputQuantities.Length < 1 || tokenOutputQuantities.Length > 19)
                throw new ArgumentException("Invalid tokenOutputQuantities! Expected 1-19 outputs, received " +
                                            tokenOutputQuantities.Length + " outputs.");

            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(4)).Concat("SEND".Select(x => (byte) x));

            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(32)).Concat(tokenId);

            byteData = tokenOutputQuantities.Aggregate(byteData, (current, tokenOutputQuantity) =>
                current.Concat(ScriptBuilder
                    .GetOpPushForLength(8)
                    .Concat(BitConverter.GetBytes(tokenOutputQuantity))));

            return ScriptBuilder.CreateOpReturn(byteData.ToArray());
        }

        private IEnumerable<byte> GetHeaderBytes()
        {
            // SLP\x00 (4 byte ascii header)
            var byteData = ScriptBuilder.GetOpPushForLength(4).Concat(_opReturnPrefix);
            // token_type
            byteData = byteData.Concat(ScriptBuilder.GetOpPushForLength(1)).Concat(new byte[] {0x1});
            return byteData;
        }

        private static bool IsValidAscii(IEnumerable<byte> source)
        {
            return null == source || source.All(b => b <= 127);
        }

        /// <summary>
        ///     ReadScript
        /// </summary>
        /// <param name="script"></param>
        /// <exception cref="InvalidSLPScriptException"></exception>
        public override SLPMessage ReadSLPScript(Script.Script script)
        {
            /*
                <lokad_id: 'SLP\x00'> (4 bytes, ascii)
                <token_type: 1> (1 to 2 byte integer)
             */
            ValidateSLPScriptHeader(script);

            if (!script.DataChunks[1].Equals(1))
                throw new InvalidSLPScriptException(
                    "Script is for an unsupport SLP version - expected 1, but received " +
                    (int) script.DataChunks[1][0] + ".");

            if (script.DataChunks.Count == 2)
                throw new InvalidSLPScriptException("Script is missing an SLP command in the 3rd data chunk.");


            if (new[] {4, 6, 7}.Contains(script.DataChunks[2].Length))
                throw new InvalidSLPScriptException(
                    "SLP command is invalid length. Excepted 4, 6, or 7 ASCII characters, but received " +
                    script.DataChunks[2].Length);

            if (!IsValidAscii(script.DataChunks[2]))
                throw new InvalidSLPScriptException(
                    "SLP command is invalid. Excepted ASCII, but received non-ASCII characters.");

            TransactionType type;
            if (!Enum.TryParse(Encoding.ASCII.GetString(script.DataChunks[2]), out type))
                throw new InvalidSLPScriptException("SLP command is invalid. Excepted " +
                                                    string.Join(", ", Enum.GetNames(typeof(TransactionType))) +
                                                    "; but received " + Encoding.ASCII.GetString(script.DataChunks[2]) +
                                                    ".");

            switch (type)
            {
                case TransactionType.GENESIS:
                    if (script.DataChunks.Count != 10)
                        throw new InvalidSLPScriptException(
                            "Script contains incorrect number of data chunks for GENESIS operations. Expected 10, but received " +
                            script.DataChunks.Count + ".");

                    // first 3 params after header (first 5 params) require no validation:
                    // token_ticker, token_name, token_document_url can be any value of any length

                    // token_document_hash must be 0 or 32 bytes
                    if (script.DataChunks[6].Length != 0 && script.DataChunks[6].Length != 32)
                        throw new InvalidSLPScriptException(
                            "Invalid GENESIS Message. Expected TokenDocumentHash with 0 or 32 bytes, but received " +
                            script.DataChunks[6].Length);
                    // decimals must be between 0 and 9
                    if (script.DataChunks[7].Length != 1)
                        throw new ArgumentException("Invalid digits after decimal! Expected 1 byte but received " +
                                                    script.DataChunks[7].Length + " bytes.");
                    if (script.DataChunks[7][0] > 9)
                        throw new ArgumentException(
                            "Invalid digits after decimal! Expected 0-9 inclusive, but received " +
                            (int) script.DataChunks[7][0] + ".");
                    // mint_baton_vout can be 0 or 1 bytes, but must not equal 1
                    if (script.DataChunks[8].Length > 1)
                        throw new ArgumentException("Invalid MintBatonVOut! Expected 0 or 1 bytes but received " +
                                                    script.DataChunks[8].Length + " bytes.");
                    if (script.DataChunks[8].Length == 1 &&
                        (script.DataChunks[8][0] == 1 || script.DataChunks[8][0] == 0))
                        throw new ArgumentException("Invalid MintBatonVOut! Expected 0x02-0xff inclusive received " +
                                                    (int) script.DataChunks[8][0] + ".");
                    // initial_token_mint_quantity must be 8 bytes
                    if (script.DataChunks[9].Length != 8)
                        throw new ArgumentException("Invalid InitialTokenMintQuantity! Expected 8 bytes but received " +
                                                    script.DataChunks[8].Length + " bytes.");

                    // validation passed, so decode and return
                    return new TokenType1Message.GenesisMessage(script.DataChunks);

                case TransactionType.SEND:
                    // minimum 2 fields, maximum 20 (19 outputs)
                    if (script.DataChunks.Count < 5 || script.DataChunks.Count > 23)
                        throw new InvalidSLPScriptException(
                            "Script contains incorrect number of data chunks for SEND operations. Expected 5-23 inclusive, but received " +
                            script.DataChunks.Count + ".");
                    // token_id must be 32 bytes
                    if (script.DataChunks[3].Length != 32)
                        throw new InvalidSLPScriptException("Invalid tokenId! Expected 32 bytes, but received " +
                                                            script.DataChunks[4].Length + " bytes.");
                    // there can be anywhere from 1 to 19 outputs, each must be 8 bytes
                    for (var i = 4; i < script.DataChunks.Count; i++)
                        if (script.DataChunks[i].Length != 8)
                            throw new InvalidSLPScriptException(
                                "Invalid TokenOutputQuantity" + i + "! Expected 8 bytes, but received " +
                                script.DataChunks[i].Length + " bytes.");

                    // validation passed, so decode and return
                    return new TokenType1Message.SendMessage(script.DataChunks);

                case TransactionType.MINT:
                    if (script.DataChunks.Count != 6)
                        throw new InvalidSLPScriptException(
                            "Script contains incorrect number of data chunks for SEND operations. Expected 6, but received " +
                            script.DataChunks.Count + ".");

                    // token_id must be 32 bytes
                    if (script.DataChunks[3].Length != 32)
                        throw new InvalidSLPScriptException("Invalid tokenId! Expected 32 bytes, received " +
                                                            script.DataChunks[4].Length + " bytes.");
                    // mint_baton_vout can be 0 or 1 bytes, but must not equal 1
                    if (script.DataChunks[4].Length > 1)
                        throw new ArgumentException("Invalid MintBatonVOut! Expected 0 or 1 bytes but received " +
                                                    script.DataChunks[5].Length + " bytes.");
                    if (script.DataChunks[4].Length == 1 &&
                        (script.DataChunks[4][0] == 1 || script.DataChunks[4][0] == 0))
                        throw new ArgumentException("Invalid MintBatonVOut! Expected 0x02-0xff inclusive received " +
                                                    (int)script.DataChunks[4][0] + ".");
                    // additional_token_quantity must be 8 bytes
                    if (script.DataChunks[5].Length != 8)
                        throw new ArgumentException("Invalid AdditionalTokenQuantity! Expected 8 bytes but received " +
                                                    script.DataChunks[5].Length + " bytes.");

                    // validation passed, so decode and return
                    return new TokenType1Message.MintMessage(script.DataChunks);

                case TransactionType.COMMIT:
                    if (script.DataChunks.Count != 8)
                        throw new InvalidSLPScriptException(
                            "Script contains incorrect number of data chunks for COMMIT operations. Expected 8, but received " +
                            script.DataChunks.Count + ".");

                    // token_id must be 32 bytes
                    if (script.DataChunks[3].Length != 32)
                        throw new InvalidSLPScriptException("Invalid tokenId! Expected 32 bytes, received " +
                                                            script.DataChunks[4].Length + " bytes.");
                    // for_bitcoin_block_hash must be 32 bytes
                    if (script.DataChunks[4].Length != 32)
                        throw new InvalidSLPScriptException("Invalid forBitcoinBlockHash! Expected 32 bytes, received " +
                                                            script.DataChunks[4].Length + " bytes.");
                    // block_height must be 8 bytes
                    if (script.DataChunks[5].Length != 8)
                        throw new ArgumentException("Invalid BlockHeight! Expected 8 bytes but received " +
                                                    script.DataChunks[5].Length + " bytes.");
                    // token_txn_set_hash must be 32 bytes
                    if (script.DataChunks[6].Length != 32)
                        throw new InvalidSLPScriptException("Invalid TokenTransactionSetHash! Expected 32 bytes, received " +
                                                            script.DataChunks[6].Length + " bytes.");
                    // txn_set_data_url requires no validation

                    // validation passed, so decode and return
                    return new TokenType1Message.CommitMessage(script.DataChunks);

                default:
                    // cannot happen (already validated)
                    throw new ArgumentOutOfRangeException(nameof(script.DataChunks), "Invalid Transaction Type");
            }
        }
    }
}