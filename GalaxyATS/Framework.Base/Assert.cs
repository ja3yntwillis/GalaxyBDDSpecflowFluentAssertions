using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LZAuto.Framework
{
    /// <summary>
    /// Assertion class
    /// </summary>
    public static class Assert
    {
        public const string NOT_EQUAL = "Expected value <{0}> not equal to Actual Value <{1}>";
        private const string LIST_NOT_EQUAL = "Expected List <{0}> not equal to Actual List <{1}>";
        private const string DICTIONARY_NOT_EQUAL = "Expected Dictionary Key-value pair <{0}> not equal to Actual Key-value pair <{1}>";
        private const string NOT_EMPTY = "Value is Null, Empty or Exclusively consists of white-space characters";
        private const string IS_NULL = "Value is Null";
        private const string IS_NOTNULL = "Value is Not Null";
        private const string NOT_GREATER = "Expected Value <{0}> not greater than Value <{1}>";
        private const string EQUAL = "Expected value <{0}> is not supposed to be equal to Actual Value <{1}>";
        private const string LIST_EQUAL = "Expected List <{0}> is not supposed to be equal to Actual List <{1}>";
        private const string DICTIONARY_EQUAL = "Expected Dictionary key-value pair <{0}> is not supposed to be equal to be Actual key-value pair <{1}>";
        private const string CONTAINS = "Main String <{0}> doesnot contain Sub-String <{1}>";
        private const string DOESNOTCONTAIN = "Main String <{0}> is not supposed to contain Sub-String <{1}>";
        private const string LISTCONTAINS = "Value(s) <{1}> is not supposed to in List <{0}>";
        private const string LISTDOESNOTCONTAIN = "List <{0}> is supposed contain atleast one value from Sub-List <{1}>";
        private const string DICTIONARY_KEY_NOT_PRESENT = "The given key <{0}> is not present in this dictionary";

        /// <summary>
        /// Assertion to check if two string values are equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        /// <param name="additionalErrorMessage">Additional Error Message</param>
        public static void AreEqual(string expectedValue, string actualValue, string additionalErrorMessage = "")
        {
            ManageAssert(((expectedValue ?? string.Empty) == (actualValue ?? string.Empty).Replace("\u202A", "").Replace("\u202B", "").Replace("\u202C", "")), string.Format(NOT_EQUAL, expectedValue, actualValue), additionalErrorMessage);
        }

        /// <summary>
        /// Assertion to check if two string values are not equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void NotEqual(string expectedValue, string actualValue)
        {
            ManageAssert((expectedValue != actualValue.Replace("\u202A", "").Replace("\u202B", "").Replace("\u202C", "")), string.Format(EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if two integer values are equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        /// <param name= "additionalErrorMessage">Additional Error Message</param>
        public static void AreEqual(int expectedValue, int actualValue, string additionalErrorMessage = "")
        {
            ManageAssert((expectedValue == actualValue), string.Format(NOT_EQUAL, expectedValue, actualValue), additionalErrorMessage);
        }

        /// <summary>
        /// Assertion to check if two integer values are not equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void NotEqual(int expectedValue, int actualValue)
        {
            ManageAssert((expectedValue != actualValue), string.Format(EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if two Double values are equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void AreEqual(double expectedValue, double actualValue, string additionalErrorMessage = "")
        {
            ManageAssert(expectedValue == actualValue, string.Format(NOT_EQUAL, expectedValue, actualValue), additionalErrorMessage);
        }

        /// <summary>
        /// Assertion to check if two Double values are not equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void NotEqual(double expectedValue, double actualValue)
        {
            ManageAssert(expectedValue != actualValue, string.Format(NOT_EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if two decimal values are equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void AreEqual(decimal expectedValue, decimal actualValue)
        {
            ManageAssert(expectedValue == actualValue, string.Format(NOT_EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if two decimal values are not equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void NotEqual(decimal expectedValue, decimal actualValue)
        {
            ManageAssert(expectedValue != actualValue, string.Format(NOT_EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if two decimal values are equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        /// <param name= "additionalErrorMessage">Additional Error Message</param>
        public static void AreEqual(decimal? expectedValue, decimal? actualValue, string additionalErrorMessage = "")
        {
            ManageAssert(expectedValue == actualValue, string.Format(NOT_EQUAL, expectedValue, actualValue), additionalErrorMessage);
        }

        /// <summary>
        /// Assertion to check if two decimal values are not equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void NotEqual(decimal? expectedValue, decimal? actualValue)
        {
            ManageAssert(expectedValue != actualValue, string.Format(NOT_EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if two boolean values are equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void AreEqual(bool expectedValue, bool actualValue)
        {
            ManageAssert((expectedValue == actualValue), string.Format(NOT_EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if two boolean values are not equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        public static void NotEqual(bool expectedValue, bool actualValue)
        {
            ManageAssert((expectedValue != actualValue), string.Format(NOT_EQUAL, expectedValue, actualValue));
        }

        /// <summary>
        /// Assertion to check if the subString is present within the main string.
        /// </summary>
        /// <param name = "mainString">Main string to be searched</param>
        /// <param name = "subString">Sub-string to be searched</param>
        public static void Contains(string mainString, string subString)
        {
            ManageAssert(mainString.Contains(subString), string.Format(CONTAINS, mainString, subString));
        }

        /// <summary>
        /// Assertion to check if the subString is not present within the main string.
        /// </summary>
        /// <param name = "mainString">Main string to be searched</param>
        /// <param name = "subString">Sub-string to be searched</param>
        public static void DoesNotContain(string mainString, string subString)
        {
            ManageAssert(!mainString.Contains(subString), string.Format(DOESNOTCONTAIN, mainString, subString));
        }

        /// <summary>
        /// Assertion to check if a value is not empty
        /// </summary>
        /// <param name = "actualValue">Value to be checked</param>
        public static void NotEmpty(string actualValue)
        {
            ManageAssert((!String.IsNullOrWhiteSpace(actualValue)), string.Format(NOT_EMPTY, actualValue));
        }

        /// <summary>
        /// Assertion to check if a value is not null
        /// </summary>
        /// <param name = "actualValue">Value to be checked</param>
        public static void NotNull(string actualValue)
        {
            ManageAssert((actualValue != null), string.Format(IS_NULL, actualValue));
        }

        /// <summary>
        /// Assertion to check if a object is not null
        /// </summary>
        /// <param name = "actualObject">Object to be checked</param>
        public static void NotNull(object actualObject)
        {
            ManageAssert((actualObject != null), string.Format(IS_NULL, actualObject));
        }

        /// <summary>
        /// Assertion to check if a value is not null
        /// </summary>
        /// <param name = "actualValue">Value to be checked</param>
        public static void IsNull(string actualValue)
        {
            ManageAssert((actualValue == null), string.Format(IS_NOTNULL, actualValue));
        }

        /// <summary>
        /// Assertion to check if a object is not null
        /// </summary>
        /// <param name = "actualObject">Object to be checked</param>
        public static void IsNull(object actualObject)
        {
            ManageAssert((actualObject == null), string.Format(IS_NOTNULL, actualObject));
        }

        /// <summary>
        /// Check the First value is greater than the second.
        /// </summary>
        /// <param name = "expectedGreater">Greater value</param>
        /// <param name = "expectedLess">Lesser Value</param>
        public static void GreaterThan(int expectedGreater, int expectedLess)
        {
            ManageAssert((expectedLess < expectedGreater), string.Format(NOT_GREATER, expectedGreater, expectedLess));
        }
        /// <summary>
        /// Check the First value is greater than the second when the type is 'double'.
        /// </summary>
        /// <param name = "expectedGreater">Greater value</param>
        /// <param name = "expectedLess">Lesser Value</param>
        public static void GreaterThan(double? expectedGreater, double expectedLess)
        {
            ManageAssert((expectedLess < expectedGreater), string.Format(NOT_GREATER, expectedGreater, expectedLess));
        }

        /// <summary>
        /// Assertion to check if two lists are equal.
        /// </summary>
        /// <param name = "expectedList">Expected List</param>
        /// <param name = "actualList">Actual List</param>
        /// <param name = "ignoreOrder">ignore insertion order</param>
        public static void AreEqual(List<string> expectedList, List<string> actualList, bool ignoreOrder = false)
        {
            if (ignoreOrder)
            {
                expectedList.Sort();
                actualList.Sort();
            }

            bool listEqual = ListEqual(expectedList, actualList);
            var errorMessage = convertListToStringMessage(expectedList, actualList, LIST_NOT_EQUAL);
            ManageAssert(listEqual, errorMessage);
        }

        /// <summary>
        /// Assertion to check if two lists are not equal.
        /// </summary>
        /// <param name = "expectedList">Expected List</param>
        /// <param name = "actualList">Actual List</param>
        public static void NotEqual(List<string> expectedList, List<string> actualList)
        {
            bool listNotEqual = !ListEqual(expectedList, actualList);
            var errorMessage = convertListToStringMessage(expectedList, actualList, LIST_EQUAL);
            ManageAssert(listNotEqual, errorMessage);
        }

        /// <summary>
        /// Assertion to check if the Main list contains atleast one value from the sub list.
        /// </summary>
        /// <param name = "mainList">Main List</param>
        /// <param name = "subList">Sub List</param>
        public static void ListContainsAtleastOneValue(List<string> mainList, List<string> subList)
        {
            var matchList = GetMatchingValues(mainList, subList);
            bool listContains = false;
            if (matchList.Count != 0)
                listContains = true;
            var errorMessage = convertListToStringMessage(mainList, subList, LISTDOESNOTCONTAIN);
            ManageAssert(listContains, errorMessage);
        }


        /// <summary>
        /// Assertion to check if the Main list does not contains no value from the sub list.
        /// </summary>
        /// <param name = "mainList">Main List</param>
        /// <param name = "subList">Sub List</param>
        public static void ListContainsNoValue(List<string> mainList, List<string> subList)
        {
            var matchList = GetMatchingValues(mainList, subList);
            bool listContains = true;
            if (matchList.Count != 0)
            {
                listContains = false;
                subList = matchList;
            }
            var errorMessage = convertListToStringMessage(mainList, subList, LISTCONTAINS);
            ManageAssert(listContains, errorMessage);
        }

        /// <summary>
        /// Assertion to check if two dictionary (Key Value pair) are equal.
        /// </summary>
        /// <param name = "expectedDictionary">Expected Dictionary</param>
        /// <param name = "actualDictionary">Actual Dictionary</param>
        public static void AreEqual(Dictionary<string, string> expectedDictionary, Dictionary<string, string> actualDictionary)
        {
            bool dictionaryEqual = DictionaryEqual(expectedDictionary, actualDictionary);
            var errorMessage = convertDictionaryToStringMessage(expectedDictionary, actualDictionary, DICTIONARY_NOT_EQUAL);
            ManageAssert(dictionaryEqual, errorMessage);
        }

        /// <summary>
        /// Assert to check if two DateTime values are equal.
        /// </summary>
        /// <param name = "expectedValue">Expected Value</param>
        /// <param name = "actualValue">Actual Value</param>
        /// <param name="additionalErrorMessage">Additional Error Message</param>
        public static void AreEqual(DateTime expectedValue, DateTime actualValue, string additionalErrorMessage = "")
        {
            ManageAssert(expectedValue.Equals(actualValue), string.Format(NOT_EQUAL, expectedValue, actualValue), additionalErrorMessage);
        }

        /// <summary>
        /// Assertion to check if two dictionary (Key Value pair) are not equal.
        /// </summary>
        /// <param name = "expectedDictionary">Expected Dictionary</param>
        /// <param name = "actualDictionary">Actual Dictionary</param>
        public static void NotEqual(Dictionary<string, string> expectedDictionary, Dictionary<string, string> actualDictionary)
        {
            bool dictionaryNotEqual = !DictionaryEqual(expectedDictionary, actualDictionary);
            var errorMessage = convertDictionaryToStringMessage(expectedDictionary, actualDictionary, DICTIONARY_EQUAL);
            ManageAssert(dictionaryNotEqual, errorMessage);
        }

        /// <summary>
        /// Evaluates whether the supplied boolean value is true.
        /// </summary>
        /// <param name="predicate">The boolean value to evalute for true.</param>
        /// <param name="falseMessage">The message to supply to the runner if the condition is false.</param>
        public static void IsTrue(bool predicate, string falseMessage)
        {
            ManageAssert(predicate, falseMessage);
        }

        public static void Fail(string errorMessage)
        {
            throw new Exception(errorMessage);
        }

        /// <summary>
        /// Asserts to make sure the expected element exists inside the source collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="expected"></param>
        public static void Exists<T>(ICollection<T> source, T expected)
        {
            var exists = source.Contains(expected);

            if (!exists)
            {
                Fail($"Element '{expected.ToString()}' did not exist inside source collection.");
            }
        }

        /// <summary>
        /// Asserts to make sure the expected element does not exist inside the source collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="notExpected"></param>
        public static void NotExists<T>(ICollection<T> source, T notExpected)
            where T : struct
        {
            var exists = source.Contains(notExpected);

            if (exists)
            {
                Fail($"Element '{notExpected.ToString()}' exist inside source collection.");
            }
        }

        /// <summary>
        /// Asserts to make sure the expected element does not exist inside the source collection
        /// </summary>
        /// <param name="source"></param>
        /// <param name="notExpected"></param>
        public static void NotExists(ICollection<string> source, string notExpected)
        {
            var exists = source.Contains(notExpected);

            if (exists)
            {
                Fail($"Element '{notExpected}' exist inside source collection.");
            }
        }

        /// <summary>
        /// Convert Dictionary of Key Value pairs to a string using JSON.
        /// </summary>
        /// <param name = "expectedDictionary">Expected Dictionary</param>
        /// <param name = "actualDictionary">Actual Dictionary</param>
        /// <param name = "errorMessage">Error Message to be displayed</param>
        private static string convertDictionaryToStringMessage(Dictionary<string, string> expectedDictionary, Dictionary<string, string> actualDictionary, string errorMessage)
        {
            var expectedListString = JsonSerializer.Serialize(expectedDictionary).Replace("\":\"", ":").Replace("{", "").Replace("}", "");
            var actualListString = JsonSerializer.Serialize(actualDictionary).Replace("\":\"", ":").Replace("{", "").Replace("}", "");
            var errorOutput = string.Format(errorMessage, expectedListString, actualListString);
            return errorOutput;
        }

        /// <summary>
        /// Convert List of values to a string using JSON.
        /// </summary>
        /// <param name = "expectedList">Expected List</param>
        /// <param name = "actualList">Actual List</param>
        /// <param name = "errorMessage">Error Message to be displayed</param>
        private static string convertListToStringMessage(List<string> expectedList, List<string> actualList, string errorMessage)
        {
            var expectedListString = JsonSerializer.Serialize(expectedList).Replace("[", "").Replace("]", "");
            var actualListString = JsonSerializer.Serialize(actualList).Replace("[", "").Replace("]", "");
            var errorOutput = string.Format(errorMessage, expectedListString, actualListString);
            return errorOutput;
        }

        /// <summary>
        /// Checking if two dictionary (key value pair) are equal.
        /// </summary>
        /// <param name = "expected">Expected Dictionary</param>
        /// <param name = "actual">Actual Dictionary</param>
        private static bool DictionaryEqual(Dictionary<string, string> expected, Dictionary<string, string> actual)
        {
            if (expected == actual)
            {
                return true;
            }
            if (expected == null || actual == null)
            {
                return false;
            }
            if (expected.Count != actual.Count)
            {
                return false;
            }
            foreach (KeyValuePair<string, string> kvp in expected)
            {
                string actualValue;
                if (!actual.TryGetValue(kvp.Key, out actualValue))
                {
                    return false;
                }
                if (kvp.Value != actualValue)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checking if two lists are equal.
        /// </summary>
        /// <param name = "expected">Expected List</param>
        /// <param name = "actual">Actual List</param>
        private static bool ListEqual(List<string> expected, List<string> actual)
        {            
            if (expected == actual)
            {
                return true;
            }
            if (expected == null || actual == null)
            {
                return false;
            }
            if (expected.Count != actual.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < expected.Count; i++)
                {
                    if (expected[i] != actual[i].Replace("\u202A", "").Replace("\u202B", "").Replace("\u202C", ""))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Check if atleast one value from the sub List is present in the main List.
        /// </summary>
        /// <param name = "mainList">Main  List</param>
        /// <param name = "subList">Sub List</param>
        private static List<string> GetMatchingValues(List<string> mainList, List<string> subList)
        {
            List<string> foundList = new List<string>();
            for (int i = 0; i < mainList.Count; i++)
            {
                for (int j = 0; j < subList.Count; j++)
                {
                    if (mainList[i] == subList[j])
                    {
                        foundList.Add(subList[j]);
                    }
                }
            }
            return foundList;

        }

        /// <summary>
        /// Manage the Assert. If continue assert feature is enabled, collect the failed asserts else throw the exception
        /// </summary>
        /// <param name="assertState">Boolean indicating assert state</param>
        /// <param name="message">Message to set when the assert is failed</param>
        /// <param name="additionalErrorMessage">Additional Error Message</param>
        private static void ManageAssert(bool assertState, string message, string additionalErrorMessage = "")
        {
            if (!assertState)
            {
                if (additionalErrorMessage != "")
                    throw new Exception(additionalErrorMessage + ": " + message);

                else
                    throw new Exception(message);
            }
        }

        public static void DictionaryContainsKey(Dictionary<string, string> dictionary,
                             string expectedKey, string additionalErrorMessage)
        {
            ManageAssert(dictionary.ContainsKey(expectedKey), string.Format(DICTIONARY_KEY_NOT_PRESENT, expectedKey), additionalErrorMessage);
        }
    }
}