using System;
using System.Collections.Generic;
using System.Linq;
using SharpBCH.Script;
using SharpBCH.Util;

namespace SharpBCH.SLP
{
    public static class Token
    {
        public static readonly byte[] OpReturnPrefix = "SLP\0".Select(x => (byte)x).ToArray(); // 534c5000 in hex

        /// <summary>
        ///     Represents an SLP Token Message
        /// </summary>
        public class SLPMessage
        {
            public List<byte[]> DataChunks { get; protected set; }
            public byte[] LokadId => DataChunks[0];
            public byte[] TokenType => DataChunks[1];

            public byte[] ToOutputScriptBytes()
            {
                var scriptBytes = new List<byte> { (byte)OpCodeType.OP_RETURN };
                foreach (var dataChunk in DataChunks)
                {
                    scriptBytes.AddRange(ScriptBuilder.GetOpPushForLength((ulong)dataChunk.Length));
                    scriptBytes.AddRange(dataChunk);
                }

                return scriptBytes.ToArray();
            }

            public Script.Script ToOutputScript()
            {
                return new Script.Script(ToOutputScriptBytes());
            }

            protected SLPMessage(List<byte[]> dataChunks)
            {
                if (dataChunks.Count < 2)
                    throw new ArgumentException("Expected at least 2 data chunnks: header and tokentype.", "dataChunks");

                DataChunks = dataChunks;
            }

            public override string ToString()
            {
                return "[SLPMessage]";
            }
        }

        public static bool DoesScriptHaveValidHeader(Script.Script script)
        {
            try
            {
                ValidateSLPScriptHeader(script);
                return true;
            }
            catch (InvalidSLPScriptException)
            {
                return false;
            }
        }

        /// <summary>
        ///     ReadScript
        /// </summary>
        /// <param name="script"></param>
        /// <exception cref="InvalidSLPScriptException"></exception>
        public static void ValidateSLPScriptHeader(Script.Script script)
        {
            /*
                <lokad_id: 'SLP\x00'> (4 bytes, ascii)
                <token_type: 1> (1 to 2 byte integer)
             */
            if (script.OpCodes.Count < 1 || !script.OpCodes[0].Equals(OpCodeType.OP_RETURN))
                throw new InvalidSLPScriptException("Script is not an op_return");

            if (script.OpCodes.Count < 2 || !script.OpCodes[1].Equals(OpCodeType.OP_DATA) ||
                script.DataChunks.Count < 1)
                throw new InvalidSLPScriptException(
                    "Script is not an SLP op_return. Header is missing.");

            if (script.DataChunks[0].Length != 4) 
                throw new InvalidSLPScriptException(
                    "Script is not an SLP op_return. Wrong header - expected 4 bytes, but received " +
                    script.DataChunks[0].Length + ".");

            if (!script.DataChunks[0].SequenceEqual(OpReturnPrefix))
                throw new InvalidSLPScriptException(
                    "Script is not an SLP op_return. Wrong header - expected 0x504c5300, but received " +
                    ByteHexConverter.ByteArrayToHex(script.DataChunks[0]) + ".");

            if (script.DataChunks.Count == 1)
                throw new InvalidSLPScriptException("Script is missing SLP token_type.");


            if (script.DataChunks[1].Length != 1 && script.DataChunks[1].Length != 2)
                throw new InvalidSLPScriptException("SLP version violates spec - expected 1 or 2 bytes, but received " +
                                                    script.DataChunks[1].Length + ".");

            // make sure the script contains only push opcodes, data, and the op_return
            if (script.OpCodes.Where(x => x != OpCodeType.OP_DATA).Count() > 1)
                throw new InvalidSLPScriptException("Script contains invalid op_codes! Invalid codes found: " +
                                                    string.Join(", ", script.OpCodes.Where(x => x != OpCodeType.OP_DATA)));
        }
    }
}