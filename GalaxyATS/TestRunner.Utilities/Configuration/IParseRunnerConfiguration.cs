namespace TestRunner.Utilities
{
    public interface IParseRunnerConfiguration
    {
        void LoadDefaultOptions();

        bool GetOption(string arg, string value, int pos);
    }
}
