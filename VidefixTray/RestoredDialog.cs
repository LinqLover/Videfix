using Videfix.Tray.Properties;
using HelperLib.MicrosoftHelpers.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Videfix.Tray
{
	internal class RestoredDialog : TaskDialog
	{
		public RestoredDialog()
		{
			Icon = TaskDialogStandardIcon.Information;
			Text = Strings.RestoredDialog_Message;
		}
	}
}