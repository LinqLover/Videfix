using Videfix.Tray.Properties;
using HelperLib.MicrosoftHelpers.WindowsAPICodePack.Dialogs;
using HelperLib.SystemHelpers.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Videfix.Tray
{
	internal class NotElevatedWarningDialog : CustomTaskDialog
	{
		public NotElevatedWarningDialog()
		{
			InstructionText = Strings.NotElevatedWarningDialog_Instruction;
			Text = Strings.NotElevatedWarningDialog_Details;
			Icon = TaskDialogStandardIcon.Warning;

			RestartButton = new TaskDialogCommandLink(nameof(RestartButton), Strings.NotElevatedWarningDialog_RestartElevated) { UseElevationIcon = true };
			SetClickResult(RestartButton, TaskDialogResult.Yes);

			IgnoreButton = new TaskDialogCommandLink(nameof(IgnoreButton), Strings.NotElevatedWarningDialog_Ignore_Title + '\n' + Strings.NotElevatedWarningDialog_Ignore_Details);
			SetClickResult(IgnoreButton, TaskDialogResult.No);

			ExitButton = new TaskDialogCommandLink(nameof(ExitButton), Strings.NotElevatedWarningDialog_Terminate);
			SetClickResult(ExitButton, TaskDialogResult.Cancel);

			Controls.AddRange(RestartButton, IgnoreButton, ExitButton);
		}

		protected TaskDialogCommandLink RestartButton { get; }
		protected TaskDialogCommandLink IgnoreButton { get; }
		protected TaskDialogCommandLink ExitButton { get; }
	}
}