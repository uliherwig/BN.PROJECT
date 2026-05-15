using BN.PROJECT.Core;
using System.Text.Json;

namespace BN.PROJECT.StrategyService.Tests
{
    public static class TestData
    {
        private static string GetPath(params string[] path)
        {
            var execDir = GetExecDir();
            var parent1 = execDir.Parent;
            var parent2 = parent1?.Parent;
            var parent3 = parent2?.Parent;
            if (parent3 == null)
                throw new InvalidOperationException("Cannot traverse up 3 parent directories from current directory.");
            return GetPath(parent3, path);
        }

        private static DirectoryInfo GetExecDir() => new(Directory.GetCurrentDirectory());

        private static string GetPath(DirectoryInfo di, params string[] path)
        {
            var pathParts = new List<string> { di.FullName };
            pathParts.AddRange(path.SelectMany(x => x.Split(new char[] { '\\', '/' })));
            return Path.Combine(pathParts.ToArray());
        }

        public static List<PriceQuote> GetTestData()
        {
            try
            {
                var result = new List<PriceQuote>();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };


                // Get the base directory of the application safely
                var dir1 = Directory.GetParent(Directory.GetCurrentDirectory());
                var dir2 = dir1?.Parent;
                var dir3 = dir2?.Parent;
                if (dir3 == null)
                    throw new InvalidOperationException("Cannot traverse up 3 parent directories from current directory.");
                string baseDirectory = dir3.FullName;

                // Combine the base directory with the relative path to the JSON file
                string filePath = Path.Combine(baseDirectory, "TestData/SPY-2024-11-27.json");

                // Read the JSON file content
                string jsonData = File.ReadAllText(filePath);

                var bars = JsonSerializer.Deserialize<List<Bar>>(jsonData, options);
                if (bars == null)
                {
                    Console.WriteLine("Warning: Deserialized bars list is null.");
                    return result;
                }

                foreach (var bar in bars)
                {
                    Console.WriteLine($"Symbol: {bar.Symbol}, T: {bar.T}, C: {bar.C}");
                    result.Add(new PriceQuote
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