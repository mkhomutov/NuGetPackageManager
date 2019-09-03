using Catel.Logging;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NuGetPackageManager.Management
{
    public class ProjectContext : INuGetProjectContext
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public ProjectContext(FileConflictAction fileConflictAction)
        {
            FileConflictAction = fileConflictAction;
        }

        public PackageExtractionContext PackageExtractionContext { get; set; }

        public ISourceControlManagerProvider SourceControlManagerProvider { get; }

        public ExecutionContext ExecutionContext { get; }

        public FileConflictAction FileConflictAction { get; private set; }

        public XDocument OriginalPackagesConfig { get; set; }
        public NuGetActionType ActionType { get; set; }
        public Guid OperationId { get; set; }

        void INuGetProjectContext.Log(MessageLevel level, string message, params object[] args)
        {
            //todo
            throw new NotImplementedException();
        }

        public void ReportError(string message)
        {
            Log.Error(message);
        }

        public FileConflictAction ResolveFileConflict(string message)
        {
            if (FileConflictAction == FileConflictAction.PromptUser)
            {
                //todo conflict resolution window
                var resolution = ShowConflictPrompt();


                if (resolution == FileConflictAction.IgnoreAll
                    ||
                    resolution == FileConflictAction.OverwriteAll)
                {
                    FileConflictAction = resolution;
                }
                return resolution;
            }

            return FileConflictAction;
        }

        private FileConflictAction ShowConflictPrompt()
        {
            //Todo show form for user, to provide him a way to tell how conflicts should be resolved
            return FileConflictAction;
        }
    }
}
