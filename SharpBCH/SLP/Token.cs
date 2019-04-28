using System.Collections.Generic;
using System.Linq;
using SharpBCH.Script;
using SharpBCH.Util;

namespace SharpBCH.SLP
{
    public abstract class Token
    {
        protected readonly byte[] _opReturnPrefix = "SLP\0".Select(x => (byte)x).ToArray(); // 534c5000 in hex

        abstract public SLPMessage ReadSLPScript(Script.Script script);

        public class SLPMessage
        {
            public List<byte[]> DataChunks { get; protected set; }
            public byte[] LokadId { get; }
            public byte[] TokenType { get; }
        }

        /// <summary>
        ///     ReadScript
        /// </summary>
        /// <param name="script"></param>
        /// <exception cref="InvalidSLPScriptException"></exception>
        public void ValidateSLPScriptHeader(Script.Script script)
        {
            /*
                <lokad_id: 'SLP\x00'> (4 bytes, ascii)
                <token_type: 1> (1 to 2 byte integer)
             */
            if (script.ScriptBytes[0] != (byte) OpCodeType.OP_RETURN)
                throw new InvalidSLPScriptException("Script is not an op_return");

            if (script.DataChunks[0].Length != 4)
                throw new InvalidSLPScriptException(
                    "Script is not an SLP op_return. Wrong header - expected 4 bytes, but received " +
                    script.DataChunks[0].Length + ".");

            if (!script.DataChunks[0].SequenceEqual(_opReturnPrefix))
                throw new InvalidSLPScriptException(
                    "Script is not an SLP op_return. Wrong header - expected 0x504c5300, but received " +
                    ByteHexConverter.ByteArrayToHex(script.DataChunks[0]) + ".");

            if (script.DataChunks.Count == 1)
                throw new InvalidSLPScriptException("Script is missing SLP token_type.");


            if (script.DataChunks[1].Length != 1 && script.DataChunks[1].Length != 2)
                throw new InvalidSLPScriptException("SLP version violates spec - expected 1 or 2 bytes, but received " +
                                                    script.DataChunks[1].Length + ".");
        }
    }
}