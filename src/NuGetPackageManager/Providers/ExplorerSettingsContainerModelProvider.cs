using Catel.IoC;
using NuGetPackageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Providers
{
    public  class ExplorerSettingsContainerModelProvider : ModelProvider<ExplorerSettingsContainer>
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
