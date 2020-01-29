using Videfix.Tray.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Videfix.Tray
{
	internal class SavedDialog : TaskDialog
	{
		public SavedDialog()
		{
			Icon = TaskDialogStandardIcon.Information;
			Text = Strings.SavedDialog_Message;
		}
	}
}