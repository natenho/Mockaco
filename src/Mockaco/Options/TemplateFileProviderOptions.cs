using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mockaco
{
    public class TemplateFileProviderOptions
    {
        public string Path { get; set; }

        public TemplateFileProviderOptions()
        {
            Path = string.Empty;
        }
    }
}
