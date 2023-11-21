using System;

namespace TestRunner.Utilities
{
    public class ConfigurationBaseParser
    {
        private IParseRunnerConfiguration RunnerConfigurationParser;

        public ConfigurationBaseParser(IParseRunnerConfiguration runnerConfigurationParser)
        {
            RunnerConfigurationParser = runnerConfigurationParser;
        }

        public void GetConfigurationArguments(string[] args)
        {
            int n = 0;

            RunnerConfigurationParser.LoadDefaultOptions();

            while (n < args.Length)
            {
                int pos = IsOption(args[n]);
                if (pos > 0)
                {
                    var value = "";
                    if (args.GetUpperBound(0) > n && IsOption(args[n + 1]) == 0)
                    {
                        value = args[n + 1];
                    }
                    if (!GetOption(args[n], value, pos) && !RunnerConfigurationParser.GetOption(args[n], value, pos))
                    {
                        Console.WriteLine("***ERROR::Invalid argument :: {0}", args[n]);
                    }
                }
                n++;
            }
            ValidateStorageArgs();
        }

        private bool GetOption(string arg, string value, int pos)
        {
            string option = arg.Substring(pos, arg.Length - pos);

            if (option == "st" || option == "storagetype")
            {
                RunnerConfiguration.StorageType = value.ToLower();
            }
            else if (option == "fsp" || option == "filestoragepath")
            {
                RunnerConfiguration.FileStoragePath = value.ToLower();
            }
            else if (option == "at" || option == "applicationtype")
            {
                RunnerConfiguration.ApplicationType = value.ToLower();
            }
            else if (option == "azsa" || option == "azstorageaccount")
            {
                RunnerConfiguration.AZStorageAccount = value;
            }
            else if (option == "azsk" || option == "azstoragekey")
            {
                RunnerConfiguration.AZStorageKey = value;
            }
            else if (option == "azsbc" || option == "azstoragescreenshot")
            {
                RunnerConfiguration.AZStorageScreenShotBlobContainer = value;
            }
            else if (option == "azst" || option == "azstoragetable")
            {
                RunnerConfiguration.AZStorageTable = value;
            }
            else if (option == "ltd" || option == "logtestdata")
            {
                RunnerConfiguration.LogTestData = true;
            }
            else if (option == "azstbc" || option == "azstoragetestdata")
            {
                RunnerConfiguration.AZStorageTestDataBlobContainer = value;
            }
            else if (option == "lta" || option == "logtestaction")
            {
                RunnerConfiguration.LogTestAction = true;
            }
            else if (option == "azsta" || option == "azstoragetestaction")
            {
                RunnerConfiguration.AZStorageTestDataBlobContainer = value;
            }
            else if (option == "lurl" || option == "loggingapi")
            {
                RunnerConfiguration.LoggingAPIUrl = value;
            }
            else
            {
                return false;
            }

            return true;
        }

        private int IsOption(string opt)
        {
            char[] c = null;
            if (opt.Length < 2)
            {
                return 0;
            }
            else if (opt.Length > 2)
            {
                c = opt.ToCharArray(0, 3);
                if (c[0] == '-' && c[1] == '-' && IsOptionNameChar(c[2]))
                {
                    return 2;
                }
            }
            else
            {
                c = opt.ToCharArray(0, 2);
            }
            if ((c[0] == '-' || c[0] == '/' && IsOptionNameChar(c[1])))
            {
                return 1;
            }
            return 0;
        }

        private bool IsOptionNameChar(char c)
        {
            return Char.IsLetterOrDigit(c) || c == '?';
        }

        private void ValidateStorageArgs()
        {
            switch (RunnerConfiguration.StorageType)
            {
                case "file":
                    if (RunnerConfiguration.FileStoragePath == null)
                        throw new ArgumentException($"Missing arguments for given storage type. File storage requires FileStoragePath to be set.");
                    break;
                case "azure":
                    if (RunnerConfiguration.AZStorageAccount == null || RunnerConfiguration.AZStorageKey == null || RunnerConfiguration.AZStorageScreenShotBlobContainer == null || RunnerConfiguration.AZStorageTable == null)
                    {
                        throw new ArgumentException($"Missing arguments for given storage type. Azure storage requires AZStorageAccount, AZStorageKey, AZStorageBlobContainer, and AZStorageTable to be set.");
                    }

                    if (RunnerConfiguration.LogTestData == true && (RunnerConfiguration.AZStorageTestDataBlobContainer == null || RunnerConfiguration.AZStorageAccount == null))
                    {
                        throw new ArgumentException($"Missing arguments for given storage type and account. Azure Test Data Storage requires AZStorageTestDataBlobContainer to be set and AzStorageAccount.");
                    }

                    if (RunnerConfiguration.LogTestAction == true && (RunnerConfiguration.AZStorageTestActionBlobContainer == null || RunnerConfiguration.AZStorageAccount == null))
                    {
                        throw new ArgumentException($"Missing arguments for given storage type and account. Azure Test Action Storage requires AZStorageTestActionBlobContainer to be set and AzStorageAccount.");
                    }
                    break;
                case "dashboard":
                    break;
                default:
                    throw new ArgumentException($"Unknown storage type of: {RunnerConfiguration.StorageType}");
            }
        }
    }
}
