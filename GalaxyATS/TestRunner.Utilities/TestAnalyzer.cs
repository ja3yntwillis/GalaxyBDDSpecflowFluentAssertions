using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace TestRunner.Utilities
{
    public class TestAnalyzer
    {
        private Dictionary<string, string> tooltipCollection = new Dictionary<string, string>();
        private const string NO_SUMMARY = "NO DOCUMENTATION FOR THIS TEST!";
        private string testFixtureKey = "T:{0}";
        private string testKey = "M:LZAuto.Tests.{0}";
        Regex regex_ProjectName = new Regex(@"(.*?), Version");

        public TestListDto BuildTestList()
        {
            GetTestDocumentation();
            var testList = new List<TestSuiteDto>();

            Assembly testBaseAssemblyUI = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + "Tests.UI.BaseTest.dll");
            Type baseTestTypeUI = testBaseAssemblyUI.GetType("LZAuto.Tests.BaseTest");

            Assembly testBaseAssemblyNUT = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + "Tests.NUT.BaseTest.dll");
            Type baseTestTypeNUT = testBaseAssemblyNUT.GetType("LZAuto.Tests.BaseTestNUT");

            Assembly testBaseAssemblyDB = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + "Tests.DB.BaseTest.dll");
            Type baseTestTypeDB = testBaseAssemblyDB.GetType("LZAuto.Tests.BaseTestDB");

            Assembly testBaseAssemblyMobile = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + "Tests.Mobile.BaseTest.dll");
            Type baseTestTypeMobile = testBaseAssemblyMobile.GetType("LZAuto.Tests.BaseTestMobile");

            var testAssemblies = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Tests.*.dll").ToList<string>();
            testAssemblies = testAssemblies.Where(x => !x.Contains("BaseTest")).ToList<string>();

            List<Type> uiTypes = new List<Type>();
            List<Type> nutTypes = new List<Type>();
            List<Type> dbTypes = new List<Type>();
            List<Type> MobileTypes = new List<Type>();
            foreach (string testAssemblyPath in testAssemblies)
            {
                Assembly testAssembly = Assembly.LoadFrom(testAssemblyPath);
                var allTypes = testAssembly.GetTypes();
                uiTypes.AddRange(allTypes.Where(t => t.BaseType == baseTestTypeUI).OrderBy(t => t.Name).ToList());
                nutTypes.AddRange(allTypes.Where(t => t.BaseType == baseTestTypeNUT).OrderBy(t => t.Name).ToArray());
                dbTypes.AddRange(allTypes.Where(t => t.BaseType == baseTestTypeDB).OrderBy(t => t.Name).ToArray());
                MobileTypes.AddRange(allTypes.Where(t => t.BaseType == baseTestTypeMobile).OrderBy(t => t.Name).ToArray());

            }

            var testSuite = new TestSuiteDto()
            {
                Name = "nut",
                TestFixtures = AnalyzeTestFixtures(nutTypes),
            };
            testList.Add(testSuite);

            var dbTestSuite = new TestSuiteDto()
            {
                Name = "db",
                TestFixtures = AnalyzeTestFixtures(dbTypes),
            };
            testList.Add(dbTestSuite);

            var uiTestSuite = new TestSuiteDto()
            {
                Name = "ui",
                TestFixtures = AnalyzeTestFixtures(uiTypes),
            };
            testList.Add(uiTestSuite);

            var MobileTestSuite = new TestSuiteDto()
            {
                Name = "mobile",
                TestFixtures = AnalyzeTestFixtures(MobileTypes),
            };
            testList.Add(MobileTestSuite);

            var filteredTestFixtures = testList.SelectMany(x => x.TestFixtures);
            var filteredTestAssemblies = filteredTestFixtures.Select(x => x.Assembly).Distinct().ToList();
            var filteredTestAttributes = filteredTestFixtures.SelectMany(x => x.TestMethods.SelectMany(y => y.Attributes)).Distinct().ToList();

            var testListDto = new TestListDto()
            {
                Assemblies = filteredTestAssemblies,
                Attributes = filteredTestAttributes,
                TestSuites = testList,
            };
            return testListDto;
        }

        public List<TestFixtureDto> AnalyzeTestFixtures(List<Type> testFixtureTypes)
        {
            var testFixtureList = new List<TestFixtureDto>();
            foreach (Type fixtureType in testFixtureTypes)
            {
                var regexedString = regex_ProjectName.Match(fixtureType.Assembly.FullName).Groups[1].Value;
                var description = tooltipCollection.ContainsKey(string.Format(testFixtureKey, fixtureType.FullName)) ? tooltipCollection[string.Format(testFixtureKey, fixtureType.FullName)] : NO_SUMMARY;

                var fixture= new TestFixtureDto()
                {
                    Assembly = regexedString,
                    Name = fixtureType.Name,
                    Description = description,
                    TestMethods = new List<TestMethodDto>(),
                };

                var isTextFixture = Attribute.IsDefined(fixtureType, typeof(LZAuto.Attributes.TestFixtureAttribute), false);
                if (isTextFixture)
                {
                    var methods = fixtureType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    var tests = methods.Where(x => Attribute.IsDefined(x, typeof(LZAuto.Attributes.TestAttribute), false)).OrderBy(x => x.Name);
                    if (tests.Count() > 0)
                    {
                        foreach (MethodInfo meth in tests)
                        {
                            var method = new TestMethodDto()
                            {
                                Name = meth.Name,
                                Description = tooltipCollection.ContainsKey(string.Format(testKey, string.Format("{0}.{1}", fixtureType.Name, meth.Name))) ? tooltipCollection[string.Format(testKey, string.Format("{0}.{1}", fixtureType.Name, meth.Name))] : NO_SUMMARY,
                                Attributes = meth.GetCustomAttributes().Where(x => x.GetType() != typeof(LZAuto.Attributes.TestAttribute) && x.GetType().Namespace == "LZAuto.Attributes").ToList(),
                            };
                            fixture.TestMethods.Add(method);
                        }

                        if (fixture.TestMethods.Count > 0)
                        {
                            testFixtureList.Add(fixture);
                        }
                    }
                }
            }
            return testFixtureList;

        }

        public void GetTestDocumentation()
        {
            Assembly testAssembly = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + "Tests.Acclaris.dll");
            string docPath = testAssembly.Location.Replace(".dll", ".xml");
            if (File.Exists(docPath))
            {
                var xmlDoc = XDocument.Load(docPath);
                tooltipCollection = xmlDoc.Descendants("members").FirstOrDefault().Descendants("member")
                .ToDictionary(el => el.Attribute("name").Value,
                              el => el.Descendants("summary")?.FirstOrDefault()?.Value?.Trim() ?? NO_SUMMARY);
            }
        }
    }
    public class TestFixtureDto
    {
        public string Assembly { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TestMethodDto> TestMethods { get; set; }
    }

    public class TestMethodDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Attribute> Attributes { get; set; }
    }

    public class TestSuiteDto
    {
        public string Name { get; set; }
        public List<TestFixtureDto> TestFixtures { get; set; }
    }

    public class TestListDto
    {
        public List<string> Assemblies { get; set; }
        public List<Attribute> Attributes { get; set; }
        public List<TestSuiteDto> TestSuites { get; set; }

    }
}
