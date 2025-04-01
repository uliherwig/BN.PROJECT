// <copyright company="BSH Hausgeräte GmbH"></copyright>

using Newtonsoft.Json;

namespace BN.PROJECT.Core;

public class BaseTest
{
    /// <summary>
    /// When set will be used as a base folder in \TestData\{TestCaseFolder}
    /// Just for convenience where over and over the same folder needs to be specified
    /// </summary>
    protected string TestCaseFolder { get; set; }

    protected static string GetPath(params string[] path) => GetPath(GetExecDir().Parent.Parent.Parent, path);

    private static DirectoryInfo GetExecDir() => new(Directory.GetCurrentDirectory());

    private static string GetPath(DirectoryInfo di, params string[] path)
    {
        var pathParts = new List<string> { di.FullName };
        pathParts.AddRange(path.SelectMany(x => x.Split(new char[] { '\\', '/' })));
        return Path.Combine(pathParts.ToArray());
    }

    protected string GetTestPath(params string[] path)
    {
        var parts = new List<string> { "TestData" };

        if (TestCaseFolder == null)
        {
            parts.Add(TestCaseFolder);
        }

        parts.AddRange(path);
        return GetPath(parts.ToArray());
    }

    protected string ReadTestData(params string[] relativePath)
    {
        var path = GetTestPath(relativePath);
        return File.ReadAllText(path);
    }

    protected T ReadTestData<T>(params string[] relativePath) => JsonConvert.DeserializeObject<T>(ReadTestData(relativePath));
}