using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpBCH;
using SharpBCH.CashAddress;
using SharpBCH.Util;

namespace SharpBCHTest
{
    [TestClass]
    public class CashAddressTests
    {
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private readonly Dictionary<string, DecodedBitcoinAddress> _cashAddressTestCases =
            new Dictionary<string, DecodedBitcoinAddress>
            {
                ["bitcoincash:qp2vpt9tesq77nu4d0hpuy7hjyy87mq0xscgy22kjl"] = new DecodedBitcoinAddress("bitcoincash",
                    ScriptType.P2PKH, ByteHexConverter.StringToByteArray("54C0ACABCC01EF4F956BEE1E13D791087F6C0F34")),
                ["bitcoincash:qq4m0c73rdyv4t4gl60pas2merqzskheqqsahwxfzx"] = new DecodedBitcoinAddress("bitcoincash",
                    ScriptType.P2PKH, ByteHexConverter.StringToByteArray("2BB7E3D11B48CAAEA8FE9E1EC15BC8C0285AF900")),
                ["bitcoincash:qqj4gfkt5c043jy347wnf2v9rrq77fq4xv999n8l8j"] = new DecodedBitcoinAddress("bitcoincash",
                    ScriptType.P2PKH, ByteHexConverter.StringToByteArray("255426CBA61F58C891AF9D34A98518C1EF241533")),
                ["bitcoincash:qp4ucz08r35uqddhkch474ccd48a7cfjlv9cvyqyk4"] = new DecodedBitcoinAddress("bitcoincash",
                    ScriptType.P2PKH, ByteHexConverter.StringToByteArray("6BCC09E71C69C035B7B62F5F57186D4FDF6132FB")),
                ["bitcoincash:ppm2qsznhks23z7629mms6s4cwef74vcwvn0h829pq"] = new DecodedBitcoinAddress("bitcoincash",
                    ScriptType.P2SH, ByteHexConverter.StringToByteArray("76A04053BDA0A88BDA5177B86A15C3B29F559873")),
                ["bitcoincash:pr95sy3j9xwd2ap32xkykttr4cvcu7as4yc93ky28e"] = new DecodedBitcoinAddress("bitcoincash",
                    ScriptType.P2SH, ByteHexConverter.StringToByteArray("CB481232299CD5743151AC4B2D63AE198E7BB0A9")),
                ["bitcoincash:pqq3728yw0y47sqn6l2na30mcw6zm78dzq5ucqzc37"] = new DecodedBitcoinAddress("bitcoincash",
                    ScriptType.P2SH, ByteHexConverter.StringToByteArray("011F28E473C95F4013D7D53EC5FBC3B42DF8ED10")),
                ["bchtest:pr6m7j9njldwwzlg9v7v53unlr4jkmx6eyvwc0uz5t"] = new DecodedBitcoinAddress("bchtest",
                    ScriptType.P2SH, ByteHexConverter.StringToByteArray("F5BF48B397DAE70BE82B3CCA4793F8EB2B6CDAC9"))
            };

        [TestMethod]
        public void CashAddressTest1()
        {
            foreach (var testCase in _cashAddressTestCases)
            {
                // decode the cash address
                var decoded = CashAddress.DecodeCashAddress(testCase.Key);
                // ensure all bytes of the hash output are correct
                CollectionAssert.AreEqual(decoded.Hash, testCase.Value.Hash);
                // ensure type output is correct
                Assert.AreEqual(decoded.Type, testCase.Value.Type);
                // ensure prefix output is correct
                Assert.AreEqual(decoded.Prefix, testCase.Value.Prefix);
            }
        }

        [TestMethod]
        public void CashAddressTest2()
        {
            foreach (var testCase in _cashAddressTestCases)
            {
                // ensure the prefix is valid
                Assert.IsTrue(Enum.IsDefined(typeof(AddressPrefix), testCase.Value.Prefix));
                // create the cash address
                var encoded = CashAddress.EncodeCashAddress(Enum.Parse<AddressPrefix>(testCase.Value.Prefix), testCase.Value.Type, testCase.Value.Hash);
                // ensure cash address output is correct
                Assert.AreEqual(encoded, testCase.Key);
            }
        }
    }
}
