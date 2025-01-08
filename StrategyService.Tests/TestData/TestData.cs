using BN.PROJECT.Core;
using System.Text.Json;

namespace BN.PROJECT.StrategyService.Tests
{
    public static class TestData
    {
        private static string GetPath(params string[] path) => GetPath(GetExecDir().Parent.Parent.Parent, path);

        private static DirectoryInfo GetExecDir() => new(Directory.GetCurrentDirectory());

        private static string GetPath(DirectoryInfo di, params string[] path)
        {
            var pathParts = new List<string> { di.FullName };
            pathParts.AddRange(path.SelectMany(x => x.Split(new char[] { '\\', '/' })));
            return Path.Combine(pathParts.ToArray());
        }

        public static List<Quote> GetTestData()
        {
            try
            {
                var result = new List<Quote>();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Get the base directory of the application
                string baseDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;

                // Combine the base directory with the relative path to the JSON file
                string filePath = Path.Combine(baseDirectory, "TestData/SPY-2024-11-27.json");

                // Read the JSON file content
                string jsonData = File.ReadAllText(filePath);

                var bars = JsonSerializer.Deserialize<List<Bar>>(jsonData, options);

                foreach (var bar in bars)
                {
                    Console.WriteLine($"Symbol: {bar.Symbol}, T: {bar.T}, C: {bar.C}");
                    result.Add(new Quote
                    {
                        Symbol = bar.Symbol,
                        BidPrice = bar.C - 0.1m,
                        AskPrice = bar.C + 0.1m,
                        TimestampUtc = bar.T
                    });
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}