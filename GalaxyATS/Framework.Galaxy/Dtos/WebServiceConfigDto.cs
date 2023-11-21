using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Galaxy.Dtos
{
    public class WebServiceConfigDto
    {
        public List<WebServiceConfig> WebServiceConfig { get; set; }
    }

    public class WebServiceConfig
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string SoapAction { get; set; }
        public List<Platform> Platform { get; set; }
    }

    public class Platform
    {
        public string Env { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
