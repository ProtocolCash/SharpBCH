﻿/*
 * Copyright (c) 2019 ProtocolCash
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */

using System;
using System.Text;
using SharpBCH;
using SharpBCH.CashAddress;
using SharpBCH.Script;
using SharpBCH.Transaction;
using SharpBCH.Util;

namespace SharpBCHDemo
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Creating output script for 'bitcoincash:qpm2qsznhks23z7629mms6s4cwef74vcwvy22gdx6a'...");
            // input cash address and produce an output script
            var decoded = CashAddress.DecodeCashAddress("bitcoincash:qpm2qsznhks23z7629mms6s4cwef74vcwvy22gdx6a");

            // so far, cash addresses have two defined types
            // let's print the readable output script for the address 
            switch (decoded.Type)
            {
                // our demo happens to be a P2PKH cash address... 
                case ScriptType.P2PKH:
                    // use ByteHexConverter to convert raw byte data for output script to readable hex
                    Console.WriteLine("Output script: " +
                                      "OP_DUP OP_HASH160 " + ByteHexConverter.ByteArrayToHex(decoded.Hash) + " OP_EQUALVERIFY OP_CHECKSIG");
                    break;
                // if it was a P2SH cash address...
                case ScriptType.P2SH:
                    // use ByteHexConverter to convert raw byte data for output script to readable hex
                    Console.WriteLine("Output script: " +
                                      "OP_HASH160 " + ByteHexConverter.ByteArrayToHex(decoded.Hash) + " OP_CHECKSIG");
                    break;
                // whoa... another type?
                default:
                    Console.WriteLine("This shouldn't happen! That's an unknown cash address type!");
                    break;
            }

            // let's use script builder now
            var outputScript = ScriptBuilder.CreateOutputScript(decoded.Type, decoded.Hash);
            // same thing if we go straight from a Cash Address
            outputScript = ScriptBuilder.CreateOutputScript("bitcoincash:qpm2qsznhks23z7629mms6s4cwef74vcwvy22gdx6a");
            Console.WriteLine("Output script from ScriptBuilder: " + outputScript);
            Console.WriteLine("Output script RAW from ScriptBuilder: " + ByteHexConverter.ByteArrayToHex(outputScript.ScriptBytes));

            // What about if we want to create an OP_RETURN output?
            // ASCII encode "My Bitcoin OP_RETURN!" and create an output script
            var opReturn = ScriptBuilder.CreateOpReturn(Encoding.ASCII.GetBytes("My Bitcoin OP_RETURN!"));
            Console.WriteLine("OP_RETURN script from ScriptBuilder: " + opReturn);
            Console.WriteLine("OP_RETURN script RAW from ScriptBuilder: " + ByteHexConverter.ByteArrayToHex(opReturn.ScriptBytes));


            // encode a hash160 from an output script as a cash address (demo script is P2PKH)
            Console.WriteLine("Encoding output script 'OP_DUP OP_HASH160 76a04053bda0a88bda5177b86a15c3b29f559873 OP_EQUALVERIFY OP_CHECKSIG'...");
            // use ByteHexConverter to convert the readable hex to raw byte data (as it would actually be encoded in an output script)
            var encoded = CashAddress.EncodeCashAddress(AddressPrefix.bitcoincash, ScriptType.P2PKH,
                ByteHexConverter.StringToByteArray("76a04053bda0a88bda5177b86a15c3b29f559873"));

            Console.WriteLine("Cash Address: " + encoded);


            // let's try decoding a raw transaction!
            var txHex =
                "020000000113b15104613103365466d9c1773a2c60c3dec7ab6ea41f7f2824f6b00556bd98370000006b483045022100bda8b53dcffbcbf3c005b7c55a923cd04eb3d3abd7632dd260f97d15cc2982ed02202dc15d4a9ad826f4b3a0781693050fe8c1cdeb919903ba11385f0b5e83c1ea5641210384dd3ad997f2e10980e755236b474f986c519599946027876cdeb4eb5a30a09fffffffff0110270000000000001976a91476a04053bda0a88bda5177b86a15c3b29f55987388ac00000000";

            var tx = new Transaction(ByteHexConverter.StringToByteArray(txHex));
            Console.WriteLine("Decoded transaction. TXID: " + tx.TXIDHex + ". Inputs: " + tx.Inputs.Length + ". Outputs: " + tx.Outputs.Length+ ". Output Scripts:");
            foreach (var output in tx.Outputs)
                Console.WriteLine(output);

            // wait
            Console.ReadKey();
        }
    }
}
