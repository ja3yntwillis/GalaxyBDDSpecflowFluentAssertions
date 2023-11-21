using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestRunner.UI.Services.Interfaces;
using TestRunner.Utilities;

namespace TestRunner.UI.Services
{
    public class TestAnalyzerService : IAnalyzeTests
    {
        TestListDto TestList { get; set; }
        public TestAnalyzerService()
        {
            
        }

        public void BuildTestList()
        {
            var testAnalyzer = new TestAnalyzer();
            TestList = testAnalyzer.BuildTestList();
        }

        public List<string> GetAssemblieList()
        {
            return TestList.Assemblies;
        }

        public List<string> GetTestSuiteList()
        {
            return TestList.TestSuites.Select(x => x.Name.ToUpper()).ToList();
        }

        public List<string> GetAttributeList()
        {
            return TestList.Attributes.Select(x => x.GetType().Name).ToList();
        }

        public List<TestFixtureDto> GetTestListFiltered(string testSuitName, string attributeName, string assemblyName)
        {
            TestSuiteDto testSuite = null;
            List<TestFixtureDto> testFixtures = new List<TestFixtureDto>();
            List<TestFixtureDto> result = new List<TestFixtureDto>();
            List<Attribute> attributes = null;

            if (!string.IsNullOrEmpty(testSuitName))
            {
                testSuite = TestList.TestSuites.Where(x => x.Name.ToLower() == testSuitName.ToLower()).Single();
            }

            if (!string.IsNullOrEmpty(assemblyName))
            {
               testFixtures = testSuite.TestFixtures.Where(x => x.Assembly.ToLower() == assemblyName.ToLower()).ToList();
            }
            else
            {
                testFixtures = testSuite.TestFixtures.ToList();
            }

            if (!string.IsNullOrEmpty(attributeName))
            {
                var attrStrList = attributeName.ToLower().Split(",");
                attributes = TestList.Attributes.Where(x => attrStrList.Contains(x.GetType().Name.ToLower())).ToList();
            }

            foreach (var testFixture in testFixtures)
            {
                var tempMethodList = testFixture.TestMethods.Where(x => x.Attributes.Intersect(attributes).Count() == attributes.Count()).ToList();

                if (tempMethodList.Count > 0)
                {
                    result.Add(new TestFixtureDto()
                    {
                        Assembly = testFixture.Assembly,
                        Description = testFixture.Description,
                        Name = testFixture.Name,
                        TestMethods = tempMethodList,
                    });
                }
            }
            return result;
        }
    }
}
