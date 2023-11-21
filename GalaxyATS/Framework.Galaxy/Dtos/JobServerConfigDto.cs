using System.Collections.Generic;

namespace Framework.Galaxy.Dtos
{
    public class JobServerConfigDto
    {
        public List<JobServerConfig> JobServerConfig { get; set; }
        public string MailsacApiKey { get; set; }
    }

    public class JobServerConfig
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string SudoUser { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FtpOutFilepath { get; set; }
    }
}
