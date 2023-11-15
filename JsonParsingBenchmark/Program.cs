using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using JsonParsingBenchmark.Converters.Stj;
using JsonParsingBenchmark.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonParsingBenchmark
{
    class Program
    {
        static async Task Main()
        {
            var path = Startup.GetTestDataPath();
            Directory.CreateDirectory(path);
            Startup.EnsureTestData(path);
            //var _inputFiles = Startup.LoadTestData();
            //var inputFilePath = _inputFiles["dotnet-core.json"];
            ////Utf8JsonStreamingReader.Read(inputFilePath);

            //_inputFiles = Startup.LoadTestData();

            //var StjOptions = new JsonSerializerOptions();
            //StjOptions.Converters.Add(new Converters.Stj.SearchResultsConverter());
            //StjOptions.Converters.Add(new Converters.Stj.SearchResultConverter());
            //StjOptions.Converters.Add(new Converters.Stj.SearchResultVersionConverter());
            //using (var stream = File.OpenRead(inputFilePath))
            //{
            //    await JsonSerializer.DeserializeAsync<SearchResults>(stream, options: StjOptions).ConfigureAwait(false);
            //}

            BenchmarkRunner.Run<FullResultBenchmarks>();
        }
    }
}
