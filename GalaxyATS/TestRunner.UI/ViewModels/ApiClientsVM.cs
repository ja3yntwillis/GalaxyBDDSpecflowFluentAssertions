using System.Collections.Generic;
using TestRunner.DTOs;

namespace TestRunner.UI.ViewModels
{
    public class ApiClientsVM
    {
        public List<ReferenceDto> UiReferences { get; set; }
        public List<ReferenceDto> ApiReferences { get; set; }
    }
}
