using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DecryptCocos2dAsset
{
    class Program
    {
        // original
        // https://github.com/cocos2d/cocos2d-x/blob/v3/cocos/base/ZipUtils.h
        // https://github.com/cocos2d/cocos2d-x/blob/v3/cocos/base/ZipUtils.cpp
        static uint[] s_uEncryptedPvrKeyParts = new uint[4];
        static uint[] s_uEncryptionKey = new uint[1024];

        static void SetKey(uint key1, uint key2, uint key3, uint key4)
        {
            s_uEncryptedPvrKeyParts[0] = key1;
            s_uEncryptedPvrKeyParts[1] = key2;
            s_uEncryptedPvrKeyParts[2] = key3;
            s_uEncryptedPvrKeyParts[3] = key4;
        }

        static uint MX(uint z, uint y, uint p, uint e, uint sum) =>
            (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (s_uEncryptedPvrKeyParts[(p & 3) ^ e] ^ z)));

        static void InitializeKey()
        {
            const int enclen = 1024;

            uint y, p, e;
            uint rounds = 6;
            uint sum = 0;
            uint z = s_uEncryptionKey[enclen - 1];

            do
            {
                uint DELTA = 0x9e3779b9;

                sum += DELTA;
                e = (sum >> 2) & 3;

                for (p = 0; p < enclen - 1; p++)
                {
                    y = s_uEncryptionKey[p + 1];
                    z = s_uEncryptionKey[p] += MX(z, y, p, e, sum);
                }

                y = s_uEncryptionKey[0];
                z = s_uEncryptionKey[enclen - 1] += MX(z, y, p, e, sum);

            } while (--rounds > 0);
        }

        static void ExportKey()
        {
            var bytes = new List<byte>();
            foreach (var x in s_uEncryptionKey)
            {
                bytes.AddRange(BitConverter.GetBytes(x));
            }
            File.WriteAllBytes("key.bin", bytes.ToArray());
        }

        static void Decrypt(ref byte[] bytes)
        {
            const int enclen = 1024;
            const int securelen = 512 * 4;
            const int distance = 64 * 4;

            var b = 0;
            var i = 0;
            var len = (bytes.Length - 12);

            for (; i < len && i < securelen; i += 4)
            {
                var key = BitConverter.GetBytes(s_uEncryptionKey[b++]);
                bytes[i + 0] ^= key[0];
                bytes[i + 1] ^= key[1];
                bytes[i + 2] ^= key[2];
                bytes[i + 3] ^= key[3];

                if (b >= enclen)
                {
                    b = 0;
                }
            }

            for (; i < len; i += distance)
            {
                var key = BitConverter.GetBytes(s_uEncryptionKey[b++]);
                bytes[i + 0] ^= key[0];
                bytes[i + 1] ^= key[1];
                bytes[i + 2] ^= key[2];
                bytes[i + 3] ^= key[3];

                if (b >= enclen)
                {
                    b = 0;
                }
            }
        }

        static void Main(string[] args)
        {
            SetKey(0xF68C6273, 0x07C32116, 0x4AF4F1AC, 0xBF0988A6);
            InitializeKey();
            //ExportKey();

            var bytes = File.ReadAllBytes("gacha_cash_2017_0701.png");
            // skip header(12byte)
            bytes = bytes.Skip(12).ToArray();
            // decrypt
            Decrypt(ref bytes);
            // skip length header(4byte)
            bytes = ZlibStream.UncompressBuffer(bytes.Skip(4).ToArray());
            File.WriteAllBytes("out.png", bytes);
        }
    }
}
