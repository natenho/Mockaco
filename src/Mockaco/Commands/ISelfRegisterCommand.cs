using McMaster.Extensions.CommandLineUtils;

namespace Mockaco.Commands
{
    public interface ISelfRegisterCommand
    {
        void SelfRegister(CommandLineApplication root);
    }
}
