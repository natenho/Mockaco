using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Mockaco.Commands
{
    public class GenerateCommand : ISelfRegisterCommand
    {
        [Required]
        [Option("-t|--type", Description = "Sets source type")]
        public string Type { get; set; }

        [Argument(1, Description = "Source URI", Name = "source_uri")]
        public string SourceUri { get; set; }

        public void SelfRegister(CommandLineApplication root)
        {
            root.Command<GenerateCommand>("generate", Configure);
        }

        private void Configure(CommandLineApplication self)
        {
            self.Description = "Templates generation";
            self.OnExecuteAsync(Execute);
        }

        private Task<int> Execute(CancellationToken arg)
        {
            return Task.FromResult(1);
        }
    }
}
