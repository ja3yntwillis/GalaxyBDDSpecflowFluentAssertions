using System.Collections.Generic;
using TestRunner.Utilities;

namespace TestRunner.UI.Services.Interfaces
{
    public interface IAnalyzeTests
    {
        void BuildTestList();

        List<string> GetAssemblieList();

        List<string> GetTestSuiteList();

        List<string> GetAttributeList();

        List<TestFixtureDto> GetTestListFiltered(string testSuiteName, string attributeName, string assemblyName);
    }
}
