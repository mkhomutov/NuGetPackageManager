namespace NuGetPackageManager.Management
{
    using Catel.Logging;
    using NuGet.Packaging;
    using NuGet.ProjectManagement;
    using NuGetPackageManager.Services;
    using NuGetPackageManager.Windows;
    using NuGetPackageManager.Windows.Dialogs;
    using System;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class ProjectContext : INuGetProjectContext
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        
        private readonly IMessageDialogService _messageDialogService;

        public ProjectContext(FileConflictAction fileConflictAction, IMessageDialogService messageDialogService)
        {
            FileConflictAction = fileConflictAction;

            _messageDialogService = messageDialogService;
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

                FileConflictAction = resolution;
            }

            return FileConflictAction;
        }

        private FileConflictAction ShowConflictPrompt()
        {

            _messageDialogService.ShowDialog<FileConflictAction>(NuGetPackageManager.Constants.PackageInstallationConflictMessage,
                "content of dialog",
                false,
                FileConflictDialogOption.OverWrite,
                FileConflictDialogOption.OverWriteAll,
                FileConflictDialogOption.Ignore,
                FileConflictDialogOption.IgnoreAll
            );

            return FileConflictAction;
        }
    }
}
