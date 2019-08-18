namespace NuGetPackageManager.Management
{
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Services;
    using System;
    using System.Collections.Generic;

    public class SourceContext : IDisposable
    {
        private static Stack<SourceContext> _activeContext = new Stack<SourceContext>();

        public static SourceContext CurrentContext
        {
            get
            {
                return _activeContext.Peek();
            }
        }

        public SourceContext(IReadOnlyList<SourceRepository> sourceRepositories, IRepositoryService repositoryService)
        {
            SourceRepositories = sourceRepositories;
            _activeContext.Push(this);
        }


        public bool IsMultipleRepository => SourceRepositories?.Count > 1;

        public IReadOnlyList<SourceRepository> SourceRepositories { get; private set; }

        public void Dispose()
        {
            //todo release this context
            _activeContext.Pop();
        }
    }
}
