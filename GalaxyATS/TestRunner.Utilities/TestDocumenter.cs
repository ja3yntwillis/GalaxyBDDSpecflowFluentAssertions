using System.Text.Json;
using TestRunner.Utilities.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TestRunner.DTOs;

namespace TestRunner.Utilities
{
    public class TestDocumenter
    {
        private string documentsPath = RunnerConfiguration.TestInventoryFileStoragePath;
        private Dictionary<string, List<string>> TestPageObjectDictionary;
        private Dictionary<string, List<string>> ActionTestDictionary;
        private Dictionary<string, List<string>> PageObjectActionDictionary;
        private Dictionary<string, List<string>> PageObjectTestDictionary;
        private XDocument XmlDocTests;
        private XDocument XmlDocFramework;

        public List<TestDocTestDto> GetAllTests()
        {
            var directorySize = GetDirectorySize("tests");

            if (directorySize > 0)
            {
                var jsonFilesPath = Directory.GetFiles(string.Format("{0}{1}tests", documentsPath, Path.DirectorySeparatorChar), "*.json", SearchOption.AllDirectories);
                var result = new List<TestDocTestDto>();

                foreach (var jsonFilePath in jsonFilesPath)
                {
                    var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));
                    var testDoc = new TestDocTestDto()
                    {
                        ClassName = Path.GetDirectoryName(jsonFilePath).Split(Path.DirectorySeparatorChar).Last(),
                        Method = Path.GetFileNameWithoutExtension(jsonFilePath),
                        Actions = testDocObject["actions"],
                        PageObjects = testDocObject["pageobjects"],
                        Description = testDocObject["description"].First(),
                    };
                    result.Add(testDoc);
                }

                return result;
            }
            else
            {
                return new List<TestDocTestDto> { };
            }
        }

        public List<TestDocApiClientDto> GetAllApiClients()
        {
            var directorySize = GetDirectorySize("clients");

            if (directorySize > 0)
            {
                var jsonFilesPath = Directory.GetFiles(string.Format("{0}{1}clients", documentsPath, Path.DirectorySeparatorChar), "*.json", SearchOption.AllDirectories);
                var result = new List<TestDocApiClientDto>();

                foreach (var jsonFilePath in jsonFilesPath)
                {
                    var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));
                    var testDoc = new TestDocApiClientDto()
                    {
                        ClassName = Path.GetDirectoryName(jsonFilePath).Split(Path.DirectorySeparatorChar).Last(),
                        Method = Path.GetFileNameWithoutExtension(jsonFilePath),
                        Actions = testDocObject["actions"],
                    };
                    result.Add(testDoc);
                }

                return result;
            }
            else
            {
                return new List<TestDocApiClientDto> { };
            }
        }

        public List<TestDocActionDto> GetAllActions()
        {
            var directorySize = GetDirectorySize("actions");

            if (directorySize > 0)
            {
                var jsonFilesPath = Directory.GetFiles(string.Format("{0}{1}actions", documentsPath, Path.DirectorySeparatorChar), "*.json", SearchOption.AllDirectories);
                var result = new List<TestDocActionDto>();

                foreach (var jsonFilePath in jsonFilesPath)
                {
                    var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));
                    var testDoc = new TestDocActionDto()
                    {
                        ClassName = Path.GetDirectoryName(jsonFilePath).Split(Path.DirectorySeparatorChar).Last(),
                        Method = Path.GetFileNameWithoutExtension(jsonFilePath),
                        PageObjects = testDocObject["pageobjects"],
                        Tests = testDocObject["tests"],
                        Description = "no summary",
                    };

                    if (testDocObject.ContainsKey("description"))
                    {
                        testDoc.Description = testDocObject["description"].First();
                    }

                    result.Add(testDoc);
                }

                return result;
            }
            else
            {
                return new List<TestDocActionDto> { };
            }
        }

        public List<TestDocPageObjectDto> GetAllPageObjects()
        {
            var directorySize = GetDirectorySize("pageobjects");

            if (directorySize > 0)
            {
                var jsonFilesPath = Directory.GetFiles(string.Format("{0}{1}pageobjects", documentsPath, Path.DirectorySeparatorChar), "*.json", SearchOption.AllDirectories);
                var result = new List<TestDocPageObjectDto>();

                foreach (var jsonFilePath in jsonFilesPath)
                {
                    var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));
                    var testDoc = new TestDocPageObjectDto()
                    {
                        ClassName = Path.GetFileNameWithoutExtension(jsonFilePath),
                        Actions = testDocObject["actions"],
                        Tests = testDocObject["tests"],
                        Description = "no summary",
                    };

                    if (testDocObject.ContainsKey("description"))
                    {
                        testDoc.Description = testDocObject["description"].First();
                    }
                    result.Add(testDoc);
                }

                return result;
            }
            else
            {
                return new List<TestDocPageObjectDto> { };
            }
        }

        public TestDocTestDto GetTest(string testName)
        {
            var testNameParsed = testName.Split(".");
            var fixture = testNameParsed[0];
            var method = testNameParsed[1];
            var jsonFilePath = string.Format("{0}{1}tests{1}{2}{1}{3}.json", documentsPath, Path.DirectorySeparatorChar, fixture, method);
            var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));

            string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string docPath = string.Format("{0}{1}UIAutomation.xml", path, Path.DirectorySeparatorChar);
            XDocument xmlDoc = null;

            if (File.Exists(docPath))
            {
                xmlDoc = XDocument.Load(docPath);
            }

            var actions = testDocObject["actions"];
            var actionDtoList = new List<TestDocActionDto>();

            foreach (var action in actions)
            {
                var splitName = action.Split(".");
                var actionDto = new TestDocActionDto()
                {
                    ClassName = splitName.First(),
                    Description = "No documentation available",
                    Method = splitName.Last(),
                };

                if (xmlDoc != null)
                {
                    actionDto.Description = xmlDoc.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("M:LZAuto.Framework.Roles." + action)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";
                }
                actionDtoList.Add(actionDto);
            }

            var pageObjects = testDocObject["pageobjects"];
            var pageObjectDtoList = new List<TestDocPageObjectDto>();

            foreach (var pageObject in pageObjects)
            {
                var pageObjectDto = new TestDocPageObjectDto()
                {
                    ClassName = pageObject,
                    Description = "No documentation available",
                };

                if (xmlDoc != null)
                {
                    pageObjectDto.Description = xmlDoc.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("T:LZAuto.Framework.PageObject." + pageObject)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";
                }
                pageObjectDtoList.Add(pageObjectDto);
            }

            var result = new TestDocTestDto()
            {
                ClassName = fixture,
                Method = method,
                ActionDtos = actionDtoList,
                PageObjectDtos = pageObjectDtoList,
            };

            return result;
        }

        public TestDocActionDto GetAction(string actionName)
        {
            var testNameParsed = actionName.Split(".");
            var fixture = testNameParsed[0];
            var method = testNameParsed[1];
            var jsonFilePath = string.Format("{0}{1}actions{1}{2}{1}{3}.json", documentsPath, Path.DirectorySeparatorChar, fixture, method);
            var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));

            string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string docPath = string.Format("{0}{1}UIAutomation.xml", path, Path.DirectorySeparatorChar);
            XDocument xmlDoc = null;

            if (File.Exists(docPath))
            {
                xmlDoc = XDocument.Load(docPath);
            }

            var tests = testDocObject["tests"];
            var testDtoList = new List<TestDocTestDto>();

            foreach (var test in tests)
            {
                var splitName = test.Split(".");
                var testDto = new TestDocTestDto()
                {
                    ClassName = splitName.First(),
                    Description = "No documentation available",
                    Method = splitName.Last(),
                };

                if (xmlDoc != null)
                {
                    testDto.Description = xmlDoc.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("M:LZAuto.Tests." + test)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";
                }
                testDtoList.Add(testDto);
            }

            var pageObjects = testDocObject["pageobjects"];
            var pageObjectDtoList = new List<TestDocPageObjectDto>();

            foreach (var pageObject in pageObjects)
            {
                var pageObjectDto = new TestDocPageObjectDto()
                {
                    ClassName = pageObject,
                    Description = "No documentation available",
                };

                if (xmlDoc != null)
                {
                    pageObjectDto.Description = xmlDoc.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("T:LZAuto.Framework.PageObject." + pageObject)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";
                }
                pageObjectDtoList.Add(pageObjectDto);
            }

            var result = new TestDocActionDto()
            {
                ClassName = fixture,
                Method = method,
                PageObjectDtos = pageObjectDtoList,
                TestDtos = testDtoList,
            };

            return result;
        }

        public TestDocPageObjectDto GetPageObject(string pageObjectName)
        {
            var fixture = pageObjectName;
            var jsonFilePath = string.Format("{0}{1}pageobjects{1}{2}.json", documentsPath, Path.DirectorySeparatorChar, fixture);
            var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));

            string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string docPath = string.Format("{0}{1}UIAutomation.xml", path, Path.DirectorySeparatorChar);
            XDocument xmlDoc = null;

            if (File.Exists(docPath))
            {
                xmlDoc = XDocument.Load(docPath);
            }

            var tests = testDocObject["tests"];
            var testDtoList = new List<TestDocTestDto>();

            foreach (var test in tests)
            {
                var splitName = test.Split(".");
                var testDto = new TestDocTestDto()
                {
                    ClassName = splitName.First(),
                    Description = "No documentation available",
                    Method = splitName.Last(),
                };

                if (xmlDoc != null)
                {
                    testDto.Description = xmlDoc.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("M:LZAuto.Tests." + test)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";
                }
                testDtoList.Add(testDto);
            }

            var actions = testDocObject["actions"];
            var actionDtoList = new List<TestDocActionDto>();

            foreach (var action in actions)
            {
                var splitName = action.Split(".");
                var actionDto = new TestDocActionDto()
                {
                    ClassName = splitName.First(),
                    Description = "No documentation available",
                    Method = splitName.Last(),
                };

                if (xmlDoc != null)
                {
                    actionDto.Description = xmlDoc.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("M:LZAuto.Framework.Roles." + action)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";
                }
                actionDtoList.Add(actionDto);
            }

            var result = new TestDocPageObjectDto()
            {
                ClassName = fixture,
                ActionDtos = actionDtoList,
                TestDtos = testDtoList,
            };

            return result;
        }

        public void SearchTests()
        {
            var files = Directory.GetFiles(string.Format("{0}{1}tests", documentsPath, Path.DirectorySeparatorChar), "*.json", SearchOption.AllDirectories);
            var regex = new Regex("Login");
            var result = new List<string>();

            foreach (var fName in files)
            {
                var testFile = File.ReadAllText(fName);

                if (regex.IsMatch(testFile))
                {
                    result.Add(fName);
                }
            }
        }

        public bool DocumentFrameworkMethods(string type)
        {
            //Globals.LoadOpCodes();
            //string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            ////string assemblyPath = string.Format("{0}{1}UIAutomation.dll", path, Path.DirectorySeparatorChar);
            //string assemblyPathTests = string.Format("{0}{1}Tests.UI.dll", path, Path.DirectorySeparatorChar);
            //string assemblyPathFramework = string.Format("{0}{1}Framework.UI.dll", path, Path.DirectorySeparatorChar);
            ////string docPath = string.Format("{0}{1}UIAutomation.xml", path, Path.DirectorySeparatorChar);
            //string docPathTests = string.Format("{0}{1}Tests.UI.xml", path, Path.DirectorySeparatorChar);
            //string docPathFramework = string.Format("{0}{1}Framework.UI.xml", path, Path.DirectorySeparatorChar);
            ////XmlDoc = XDocument.Load(docPath);
            //XmlDocTests = XDocument.Load(docPathTests);
            //XmlDocFramework = XDocument.Load(docPathFramework);
            ////Assembly testAssembly = Assembly.LoadFrom(assemblyPath);
            //Assembly assemblyTests = Assembly.LoadFrom(assemblyPathTests);
            //Assembly assemblyFramework = Assembly.LoadFrom(assemblyPathFramework);

            //IEnumerable<Type> classList = new Type[0];
            //Type attributeFilter = null;
            //string namespaceFilter = "";

            //if (type.ToLower() == "tests")
            //{
            //    ActionTestDictionary = new Dictionary<string, List<string>>();
            //    //classList = typeof(BaseTest).Assembly.GetTypes().Where(t => t.Namespace == "LZAuto.Tests").OrderBy(t => t.Name).ToArray();

            //    classList = assemblyTests.GetTypes().Where(t => t.Namespace == "LZAuto.Tests" && Attribute.IsDefined(t, typeof(LZAuto.Attributes.TestFixtureAttribute), false)).OrderBy(t => t.Name);
            //    attributeFilter = typeof(LZAuto.Attributes.TestAttribute);
            //    namespaceFilter = "LZAuto.Framework.Roles";
            //}
            //else if (type.ToLower() == "actions")
            //{
            //    TestPageObjectDictionary = new Dictionary<string, List<string>>();
            //    PageObjectActionDictionary = new Dictionary<string, List<string>>();
            //    PageObjectTestDictionary = new Dictionary<string, List<string>>();
            //    classList = assemblyFramework.GetTypes().Where(t => t.Namespace == "LZAuto.Framework.Roles" && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute), false)).OrderBy(t => t.Name);
            //    attributeFilter = typeof(LZAuto.Attributes.ParentActionAttribute);
            //    namespaceFilter = "LZAuto.Framework.PageObject";
            //}
            //else if (type.ToLower() == "pageobjects")
            //{
            //    classList = assemblyFramework.GetTypes().Where(t => t.Namespace == "LZAuto.Framework.PageObject" && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute), false)).OrderBy(t => t.Name);
            //    attributeFilter = typeof(LZAuto.Attributes.TestAttribute);
            //    namespaceFilter = "DummyNamespace";
            //}
            //else if (type.ToLower() == "clients")
            //{
            //    ActionTestDictionary = new Dictionary<string, List<string>>();

            //    classList = assemblyTests.GetTypes().Where(t => t.Namespace == "LZAuto.Tests" && Attribute.IsDefined(t, typeof(LZAuto.Attributes.TestFixtureAttribute), false)).OrderBy(t => t.Name);
            //    attributeFilter = typeof(LZAuto.Attributes.TestAttribute);
            //    namespaceFilter = "LZAuto.Utility.RESTAPI";
            //}

            //foreach (Type classType in classList)
            //{
            //    IEnumerable<MethodInfo> methods = new MethodInfo[0];

            //    if (type.ToLower() == "pageobjects")
            //    {
            //        var a = classType.GetMethods().First().DeclaringType;
            //        var b = classType.FullName;
            //        methods = classType.GetMethods().Where(t => t.DeclaringType == classType).OrderBy(t => t.Name);
            //    }
            //    else
            //    {
            //        methods = classType.GetMethods().Where(t => (Attribute.IsDefined(t, attributeFilter, false))).OrderBy(t => t.Name);
            //    }

            //    foreach (MethodInfo testMethod in methods)
            //    {
            //        if (type.ToLower() == "clients")
            //        {
            //            DocumentApi(classType.Name, testMethod, namespaceFilter, type);
            //        }
            //        else if (type.ToLower() == "tests")
            //        {
            //            DocumentTest(classType.Name, testMethod, namespaceFilter, type);
            //        }
            //        else if (type.ToLower() == "actions")
            //        {
            //            DocumentAction(classType.Name, testMethod, namespaceFilter, type);
            //        }
            //        else
            //        {
            //            DocumentMethod(classType.Name, testMethod, namespaceFilter, type);
            //        }
            //    }
            //}

            //if (type.ToLower() == "tests")
            //{
            //    foreach (var actionTests in ActionTestDictionary)
            //    {
            //        var splitActionName = actionTests.Key.Split(".");
            //        var className = splitActionName[0];
            //        var methodName = splitActionName[1];
            //        string testDocPath = string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, "actions");

            //        if (!Directory.Exists(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, className)))
            //        {
            //            Directory.CreateDirectory(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, className));
            //        }
            //        var testDocActionObject = new Dictionary<string, List<string>>()
            //            {
            //                {"tests", actionTests.Value},
            //                {"pageobjects", new List<string>() },
            //            };
            //        var testDocJson = JsonSerializer.Serialize(testDocActionObject);
            //        File.WriteAllText(string.Format("{0}{1}{2}{1}{3}.json", testDocPath, Path.DirectorySeparatorChar, className, methodName), testDocJson);
            //    }
            //}
            //else if (type.ToLower() == "actions")
            //{
            //    string testDocPath = string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, "pageobjects");

            //    if (!Directory.Exists(string.Format(testDocPath)))
            //    {
            //        Directory.CreateDirectory(testDocPath);
            //    }

            //    foreach (var pageObjectActions in PageObjectActionDictionary)
            //    {
            //        var pageObjectTests = new List<string>();
            //        PageObjectTestDictionary.TryGetValue(pageObjectActions.Key, out pageObjectTests);
            //        var className = pageObjectActions.Key;
            //        var summary = "";
            //        summary = XmlDocFramework.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("T:LZAuto.Framework.PageObject." + pageObjectActions.Key)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";
            //        var testDocActionObject = new Dictionary<string, List<string>>()
            //            {
            //                {"actions", pageObjectActions.Value},
            //                {"tests", pageObjectTests.Distinct().ToList<string>() },
            //                {"description", new List<string>(){summary } },
            //            };
            //        var testDocJson = JsonSerializer.Serialize(testDocActionObject);
            //        File.WriteAllText(string.Format("{0}{1}{2}.json", testDocPath, Path.DirectorySeparatorChar, className), testDocJson);
            //    }

            //    foreach (var testPageObject in TestPageObjectDictionary)
            //    {
            //        var pageObjectTests = new List<string>();
            //        var splitTestName = testPageObject.Key.Split(".");
            //        var className = splitTestName[0];
            //        var methodName = splitTestName[1];
            //        testDocPath = string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, "tests");
            //        var jsonFilePath = string.Format("{0}{1}{2}{1}{3}.json", testDocPath, Path.DirectorySeparatorChar, className, methodName);

            //        if (File.Exists(jsonFilePath))
            //        {
            //            var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));
            //            testDocObject["pageobjects"] = testPageObject.Value.Distinct().ToList<string>();
            //            var testDocJson = JsonSerializer.Serialize(testDocObject);

            //            if (testDocJson != "null")
            //            {
            //                File.WriteAllText(jsonFilePath, testDocJson);
            //            }
            //        }
            //    }
            //}

            return true;
        }

        public void DocumentMethod(string fixtureName, MethodInfo mi, string namespaceFilter, string type)
        {
            string testDocPath = string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, type);
            var testDocList = new List<Dictionary<string, string>>();
            var ActionList = new List<string>();
            MethodBodyReader mr = new MethodBodyReader(mi);
            string msil = mr.GetBodyCode();

            var codet = mr.instructions.Where(x => x.Operand is MethodInfo && ((MethodInfo)x.Operand).ReflectedType.Namespace == namespaceFilter).Select(x => ((MethodInfo)x.Operand).DeclaringType.Name + "." + ((MethodInfo)x.Operand).Name).ToList<string>();
            var testDocJson = JsonSerializer.Serialize(codet);

            if (testDocJson != "null")
            {
                if (!Directory.Exists(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, fixtureName)))
                {
                    Directory.CreateDirectory(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, fixtureName));
                }
                File.WriteAllText(string.Format("{0}{1}{2}{1}{3}.json", testDocPath, Path.DirectorySeparatorChar, fixtureName, mi.Name), testDocJson);
            }
        }

        public void DocumentTest(string fixtureName, MethodInfo mi, string namespaceFilter, string type)
        {
            string testDocPath = string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, type);
            MethodBodyReader mr = new MethodBodyReader(mi);
            string msil = mr.GetBodyCode();
            var summary = "";
            summary = XmlDocTests.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("M:LZAuto.Tests." + fixtureName + "." + mi.Name)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";

            var codet = mr.instructions.Where(x => x.Operand is MethodInfo && ((MethodInfo)x.Operand).ReflectedType.Namespace.Contains(namespaceFilter)).Select(x => ((MethodInfo)x.Operand).DeclaringType.Name + "." + ((MethodInfo)x.Operand).Name).Distinct().ToList<string>();
            var testDocObject = new Dictionary<string, List<string>>()
            {
                {"actions", codet },
                {"pageobjects", new List<string>() },
                {"description", new List<string>(){summary } },
            };
            var testDocJson = JsonSerializer.Serialize(testDocObject);

            if (testDocJson != "null")
            {
                if (!Directory.Exists(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, fixtureName)))
                {
                    Directory.CreateDirectory(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, fixtureName));
                }
                File.WriteAllText(string.Format("{0}{1}{2}{1}{3}.json", testDocPath, Path.DirectorySeparatorChar, fixtureName, mi.Name), testDocJson);

                foreach (var action in codet)
                {
                    if (!ActionTestDictionary.ContainsKey(action))
                    {
                        ActionTestDictionary.Add(action, new List<string>());
                    }
                    ActionTestDictionary[action].Add(string.Format("{0}.{1}", fixtureName, mi.Name));
                }
            }
        }

        public void DocumentApi(string fixtureName, MethodInfo mi, string namespaceFilter, string type)
        {
            string testDocPath = string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, type);
            MethodBodyReader mr = new MethodBodyReader(mi);
            string msil = mr.GetBodyCode();
            var summary = "";
            summary = XmlDocTests.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("M:LZAuto.Tests." + fixtureName + "." + mi.Name)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";

            var codet = mr.instructions.Where(x => x.Operand is MethodInfo && ((MethodInfo)x.Operand).ReflectedType.Namespace.Contains(namespaceFilter)).Select(x => ((MethodInfo)x.Operand).DeclaringType.Name + "." + ((MethodInfo)x.Operand).Name).Distinct().ToList<string>();
            var testDocObject = new Dictionary<string, List<string>>()
            {
                {"actions", codet },
                {"description", new List<string>(){summary } },
            };
            var testDocJson = JsonSerializer.Serialize(testDocObject);

            if (testDocJson != "null")
            {
                if (!Directory.Exists(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, fixtureName)))
                {
                    Directory.CreateDirectory(string.Format("{0}{1}{2}", testDocPath, Path.DirectorySeparatorChar, fixtureName));
                }
                File.WriteAllText(string.Format("{0}{1}{2}{1}{3}.json", testDocPath, Path.DirectorySeparatorChar, fixtureName, mi.Name), testDocJson);

                foreach (var action in codet)
                {
                    if (!ActionTestDictionary.ContainsKey(action))
                    {
                        ActionTestDictionary.Add(action, new List<string>());
                    }
                    ActionTestDictionary[action].Add(string.Format("{0}.{1}", fixtureName, mi.Name));
                }
            }
        }

        public void DocumentAction(string fixtureName, MethodInfo mi, string namespaceFilter, string type)
        {
            string testDocPath = string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, type);
            MethodBodyReader mr = new MethodBodyReader(mi);
            string msil = mr.GetBodyCode();

            var codet = mr.instructions.Where(x => x.Operand is MethodInfo && ((MethodInfo)x.Operand).ReflectedType.Namespace == namespaceFilter).Select(x => ((MethodInfo)x.Operand).DeclaringType.Name).Distinct().ToList<string>();

            var jsonFilePath = string.Format("{0}{1}{2}{1}{3}.json", testDocPath, Path.DirectorySeparatorChar, fixtureName, mi.Name);

            if (File.Exists(jsonFilePath))
            {
                var testDocObject = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(jsonFilePath));
                testDocObject["pageobjects"] = codet;
                var summary = "";
                summary = XmlDocFramework.Descendants("members").Descendants("member").Where(x => x.Attribute("name").Value.Contains("M:LZAuto.Framework.Roles." + fixtureName + "." + mi.Name)).Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? "no summary";

                testDocObject["description"] = new List<string>() { summary };
                var testDocJson = JsonSerializer.Serialize(testDocObject);

                if (testDocJson != "null")
                {
                    File.WriteAllText(jsonFilePath, testDocJson);

                    foreach (var pageObject in codet)
                    {
                        if (!PageObjectActionDictionary.ContainsKey(pageObject))
                        {
                            PageObjectActionDictionary.Add(pageObject, new List<string>());
                        }
                        PageObjectActionDictionary[pageObject].Add(string.Format("{0}.{1}", fixtureName, mi.Name));

                        if (!PageObjectTestDictionary.ContainsKey(pageObject))
                        {
                            PageObjectTestDictionary.Add(pageObject, new List<string>());
                        }
                        PageObjectTestDictionary[pageObject].AddRange(testDocObject["tests"]);
                    }

                    foreach (var test in testDocObject["tests"])
                    {
                        if (!TestPageObjectDictionary.ContainsKey(test))
                        {
                            TestPageObjectDictionary.Add(test, new List<string>());
                        }
                        TestPageObjectDictionary[test].AddRange(codet);
                    }
                }
            }
        }

        private int GetDirectorySize(string type)
        {
            int directorySize;

            try
            {
                directorySize = Directory.GetFiles(string.Format("{0}{1}{2}", documentsPath, Path.DirectorySeparatorChar, type), "*.json", SearchOption.AllDirectories).Length;
            }
            catch
            {
                directorySize = 0;
            }

            return directorySize;
        }
    }
}
