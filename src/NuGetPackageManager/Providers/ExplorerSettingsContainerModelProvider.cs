using Catel.IoC;
using NuGetPackageManager.Models;

namespace NuGetPackageManager.Providers
{
    public class ExplorerSettingsContainerModelProvider : ModelProvider<ExplorerSettingsContainer>
    {
        private readonly ITypeFactory _typeFactory;

        public ExplorerSettingsContainerModelProvider(ITypeFactory typeFactory)
        {
            _typeFactory = typeFactory;

            //create instance of Model
            Model = _typeFactory.CreateInstance<ExplorerSettingsContainer>();
        }
    }
}
