using Chimp.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Net.Chdk.Detectors.Software;
using Net.Chdk.Detectors.Software.Sdm;
using Net.Chdk.Encoders.Binary;
using Net.Chdk.Meta.Providers.Sdm;
using Net.Chdk.Meta.Providers.Software.Zip;
using Net.Chdk.Meta.Writers.Software;
using Net.Chdk.Meta.Writers.Software.Json;
using Net.Chdk.Model.Software;
using Net.Chdk.Providers.Boot;
using Net.Chdk.Providers.Camera;
using Net.Chdk.Providers.Category;
using Net.Chdk.Providers.Crypto;
using Net.Chdk.Providers.Product;
using Net.Chdk.Providers.Software;
using Net.Chdk.Providers.Software.Sdm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Net.Chdk.Meta.Providers.Software.Sdm
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: hash2sw input1.zip [input2.zip [...]] hash2sw.json");
                return;
            }

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddChimpLogging()
                .AddCategoryProvider()
                .AddProductProvider()
                .AddHashProvider()
                .AddBootProvider()
                .AddBinaryDecoder()
                .AddCameraProvider()
                .AddSourceProvider()
                .AddSoftwareHashProvider()
                .AddBinarySoftwareDetector()
                .AddKnownPsBinarySoftwareDetector()
                .AddSdmSoftwareDetector()
                .AddSdmAdHocSoftwareDetector()
                .AddSdmSourceProvider()
                .AddCategoryMetaProvider()
                .AddSourceMetaProvider()
                .AddBuildMetaProvider()
                .AddCompilerMetaProvider()
                .AddEncodingMetaProvider()
                .AddSdmCameraMetaProvider()
                .AddSdmProductMetaProvider()
                .AddZipSoftwareMetaProvider()
                .AddJsonSoftwareWriter()
                .BuildServiceProvider();

            var hash2sw = GetSoftware(serviceProvider, args, "PS");

            WriteSoftware(serviceProvider, args.Last(), hash2sw);
        }

        private static Dictionary<string, SoftwareInfo> GetSoftware(IServiceProvider serviceProvider, string[] args, string categoryName)
        {
            var softwareProvider = serviceProvider.GetService<ISoftwareMetaProvider>();
            return args.Take(args.Length - 1)
                .SelectMany(p => softwareProvider.GetSoftware(p, categoryName))
                .Where(s => s != null)
                .ToDictionary(s => s.Hash.Values.Values.Single(), s => s);
        }

        private static void WriteSoftware(IServiceProvider serviceProvider, string path, Dictionary<string, SoftwareInfo> hash2sw)
        {
            serviceProvider.GetService<ISoftwareWriter>()
                .WriteSoftware(path, hash2sw);
        }
    }
}
