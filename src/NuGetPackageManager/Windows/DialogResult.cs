using Catel;
using Catel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Windows
{
    public class DialogResult<T> : DialogResult
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public T Result { get; private set; }

        public override void SetResult(IDialogOption result)
        {
            try
            {
                Argument.IsNotNull(() => result);

                if (result is IDialogOption<T>)
                {
                    Result = (result as IDialogOption<T>).DialogCallback();
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            catch(InvalidCastException e)
            {
                Log.Error(e, $"Cannot set result value, expected type {typeof(T)} was {result.GetType()}");
            }
        }
    }

    public class DialogResult
    {
        public virtual void SetResult(IDialogOption result)
        {

        }
    }
}
