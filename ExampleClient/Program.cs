﻿using System;
using System.IO;
using System.Threading.Tasks;
using DHaven.Faux;

namespace ExampleClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var wrapper = new Faux<IValuesService>();
            var blobWrapper = new Faux<IBlobService>();

            try
            {
                Task.Run(() => RunBlob(blobWrapper.Service)).Wait();
                Task.Run(() => RunExample(wrapper.Service)).Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            Console.In.Read();
        }

        private static async Task RunExample(IValuesService valueService)
        {
            Console.Out.WriteLine($"Get All Values: {string.Join(",", await valueService.Get())}");
            Console.Out.WriteLine($"Get 1: {await valueService.Get(1)}");
        }

        private static async Task RunBlob(IBlobService blobService)
        {
            using (var content = typeof(Program).Assembly.GetManifestResourceStream("ExampleClient.coffee.jpg"))
            {
                var etag = await blobService.UploadAsync(content, 82825, "image/jpeg");

                using (var returnContent = await blobService.GetAsync(etag, "attachment; filename=my-coffee.jpg"))
                {
                    content.Seek(0, SeekOrigin.Begin);

                    int len;
                    var readBuf = new byte[8192];
                    var compBuf = new byte[readBuf.Length];
                    while ((len = content.Read(readBuf, 0, readBuf.Length)) > 0)
                    {
                        if (len != returnContent.Read(compBuf, 0, len))
                        {
                            throw new Exception("Invalid content, lengths are different");
                        }

                        for (var i = 0; i < len; i++)
                        {
                            if (readBuf[i] != compBuf[i])
                            {
                                throw new Exception("Invalid content, not same results");
                            }
                        }
                    }
                }
            }
        }
    }
}