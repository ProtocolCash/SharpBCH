using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpBCH.Block;

namespace SharpBCHTest
{
    [TestClass]
    public class MerkleRootTests
    {
        private Dictionary<string, string[]> merkleRootTestCases =
            new Dictionary<string, string[]>
            {
                ["a17a4959eacfae4f3e06c4129c87e627ce6fe93987e78b66999a38e684c6fed5"] = new [] {
                    "fd636107ceb6de2486331ad662955d09abf0414079f2ea59f12da2cfa15c4561",
                    "088b7d88355a96633fb9586806d75d9c7e6e08b8ddaea8155f4be5ef180df3a7",
                    "dee47a1af1fbdc1ea8415ad046677234b008aac1a1f46365c5b59a33eca48065",
                    "126dbb8968504661d68adfdee5d969993e9d5262900b40ba10a92b7403e33164",
                    "9014543cdfe4f59d03f3e58d0e3cd34b1205e3173080d6252ba2c4d19977b672",
                    "ab41defef0fd2929868848dd853087e39544772b3469812d3530dc7a93604fd4",
                    "4ab90706d1162c6ef46bf7f4ab6a39cfae2f47a939a33bb9aed31e3bbe3bd86e",
                    "c6475296a18ad0423dacc3a94a231a60609f34ff068b7374880a42cbc5316307",
                    "4059bab2ec2255c0fe0c74afc774cefbbfddb1073745f6f9469c5545938f4891",
                    "e1622b99c933d518389f1793cac5fe482e5a6e8835d4803bb5deb60634fbc7bd",
                    "965fc983603545eab3571170940bb77fc301bdd02d4703504f580fdcf57abbfd",
                    "49f0f8198def669faefe2a9b30310edbd96ee685ea46e91c7a694863dcfa6c40",
                    "3751b0c8ea70985bcefbe0fd57e5977af32484a80a3bc6c96002ef94782e502b",
                    "33bb6d11961394dfa6262ca0a9e7d8ef8a090d02486be0067a0eea2462fc53b0",
                    "44195b102d6adf310530be98c9f216450bb66849030dc37a1bb832a3b1f0aa49",
                    "890756e5b2010f0a2514155450c9c1a40a5cfcd0f8f863b8820edbd93cb804ef",
                    "032a93ec78dfa141671e39bc482068d833f8ecf0c1c3daf580fcf97815a37e25",
                    "fd21f47e89e9bd3dca07e6d8274a49e7838ac8851e96228102f31fd1a7dd755f",
                    "69d184c03a2ca64a8ddfc84839f7dd71c66ec5a8ecde726e8834bfd71c3ae496",
                    "57aad7b35748c1d494240b3f4eaad3edd28edcfd645de4cb04aa430b2b870ca5",
                    "80f5f39bf798a2a13338cbe4f71aaca2c155e5fc9f97b50ca83e770e98deba90"
                },
                ["935aa0ed2e29a4b81e0c995c39e06995ecce7ddbebb26ed32d550a72e8200bf5"] = new []{
                    "5b09bbb8d3cb2f8d4edbcf30664419fb7c9deaeeb1f62cb432e7741c80dbe5ba",
                    "7fec6bd918ee43fddebc9a7d976f3c6d31a61efb4f27482810a6b63f0e4a02d5",
                    "a9300383c7b0f5fc03d495844420f25035c34c4c1abb0bdb43fed1d491bbb5e2",
                    "956365e81276bea27acc4278c90481a2c178b402ed988e976e205fb0e28c1ebc",
                    "505b42ec5e8499843ae3ad6f56f66ce52025d37205df19fb5777179d407b2978",
                    "22cdca7a187a893c4e4409ae0c287a3405e7d1e1ca6693a415e8b0c82cf09a2c",
                    "e6e792f8a1f3cf98ea4576ec430e5bef74c074056c531ab9d454a03b84791982",
                    "051986af765cd6392d3adce667986639dafbf28edeebdd3e8795724d29be00b3",
                    "fd620941d72497acf6b3294fe5a539fde302638f0dffdfa1a439cbc8f436be53",
                    "8978504ce06faf8faab517dce04038998541a6e693b997446782089bf01b6258",
                    "c4af2dfd69e7faeba1875bc2b71ab603dc1205abfc7f2a0c2b571548f1b013da",
                    "c894d9045886c600bd0330b799846684071210b98c3b68ae52c56207fcb5ff00",
                    "584efaa0b56ac226eb2c484c1ba781c9ef930492a4fed26e170b928dd4e5d85b",
                    "29d26dc14ccd8b77a2f1e8ebfa8e5f929cf8a18ec561996979f32864b60ded88",
                    "97c032354fd93437b7697fde1fad8d3b6e24e7ce226db1f96107cdb135363542",
                    "2c92fc4875abcceea6ebacea45ede9203b873cc2d9a05d3b4ca00e518e25ef60",
                    "1ff66038141233f3a01d89011d109a626a11991c1217b1ffcc0150d31f1dc372",
                    "bee7a125993e4e70fcd5fdabf6fd61dfe54213d7c8c7c486d1ff019db1aa2d38",
                    "c6b8da203b5359652049e7e585d1ed9163f61839fa3d1b1bc1aa68d9ae2b2946",
                    "834b5547cfa4557c8a94a2208c679ea0d82ff905fa8d70f054f0e428b71e8905",
                    "46f9af1f8642a2d62dff74f884a327432057cb9eb0ea638b4d06738dbd5033a4",
                    "654d9eae4b7fdd62dad57e13fed45832df8719bd82f14914c5aea006c556b16d"}
        };

        [TestMethod]
        public void TestMerkleRootBuild()
        {
            foreach (var (merkleRoot, transactions) in merkleRootTestCases)
            {
                Assert.AreEqual(merkleRoot, MerkleTree.BuildMerkleRoot(transactions));
            }
        }
    }
}