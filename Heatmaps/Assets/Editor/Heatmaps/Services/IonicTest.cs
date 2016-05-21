using System;
using UnityEngine;
using System.IO;
using System.Text;
using Ionic.Zlib;
using System.Collections.Generic;


public class IonicTest
{

    public static string[] DecompressFiles(string[] args)
    {
        var results = new List<string>();
        foreach(var fileName in args)
        {
            byte[] file = File.ReadAllBytes(fileName);
            byte[] decompressed = Decompress(file);
            string responsebody = Encoding.UTF8.GetString(decompressed);

            results.Add(responsebody);
        }
        return results.ToArray();
    }

    static byte[] Decompress(byte[] gzip)
    {
        // Create a GZIP stream with decompression mode.
        // ... Then create a buffer and write into while reading from the GZIP stream.
        using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
        {
            const int size = 4096;
            byte[] buffer = new byte[size];
            using (MemoryStream memory = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);
                return memory.ToArray();
            }
        }
    }
}