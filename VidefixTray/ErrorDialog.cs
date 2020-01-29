using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Videfix.Tray.Properties;
using HelperLib;
using HelperLib.MicrosoftHelpers.WindowsAPICodePack.Dialogs;
using HelperLib.SystemHelpers.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Videfix.Tray
{
	internal class TaskDialog : CustomTaskDialog
	{
		public TaskDialog()
		{
			Caption = Strings.Dialog_AppTitle;
		}
	}

	internal abstract class ErrorDialog : TaskDialog
	{
		protected ErrorDialog()
		{
			Icon = TaskDialogStandardIcon.Error;

			(DetailsCollapsedLabel, DetailsExpandedLabel) = (Strings.ErrorDialog_ShowDetails, Strings.ErrorDialog_HideDetails);
		}
	}

	internal class BusyErrorDialog : ErrorDialog
	{
		public BusyErrorDialog()
		{
			InstructionText = Strings.BusyErrorDialog_Instruction;
			Text = Strings.BusyErrorDialog_Details;
		}
	}

	internal abstract class SpecificErrorDialog : ErrorDialog
	{
		protected SpecificErrorDialog(Exception exception)
		{
			DetailsExpandedText = exception.ToString();
		}
	}

	internal class GeneralErrorDialog : SpecificErrorDialog
	{
		public GeneralErrorDialog(Exception exception) : base(exception)
		{
			InstructionText = Strings.GeneralErrorDialog_Instruction;
			Text = Strings.GeneralErrorDialog_Details;
		}
	}

	internal class RestoreErrorDialog : SpecificErrorDialog
	{
		public RestoreErrorDialog(Exception exception) : base(exception)
		{
			InstructionText = Strings.RestoreErrorDialog_Instruction;
			Text = exception.Message /*+ Environment.NewLine.Repeat(2) + Strings.RestoreErrorDialog_Details*/;
		}
	}

	internal class SaveErrorDialog : SpecificErrorDialog
	{
		public SaveErrorDialog(Exception exception) : base(exception)
		{
			InstructionText = Strings.SaveErrorDialog_Instruction;
			Text = exception.Message;
		}
	}

	internal class BackupErrorDialog : SpecificErrorDialog
	{
		public BackupErrorDialog(Exception exception) : base(exception)
		{
			InstructionText = Strings.BackupErrorDialog_Instruction;
			Text = exception.Message;
		}
	}

	internal class ConfigureStartWithWindowsErrorDialog : SpecificErrorDialog
	{
		public ConfigureStartWithWindowsErrorDialog(Exception exception) : base(exception)
		{
			InstructionText = Strings.ConfigureStartWithWindowsErrorDialog_Instruction;
			Text = exception.Message;
		}
	}

	internal class OfferRestoreDialog : TaskDialog
	{
		public OfferRestoreDialog(DateTime lastBackedUp)
		{
			InstructionText = Strings.OfferRestoreDialog_Instruction;

			AcceptButton = new TaskDialogCommandLink(nameof(AcceptButton), Strings.OfferRestoreDialog_Accept + '\n' + string.Format(Strings.OfferRestoreDialog_Accept_Details, lastBackedUp));
			SetClickResult(AcceptButton, TaskDialogResult.Yes);

			DeclineButton = new TaskDialogCommandLink(nameof(DeclineButton), Strings.OfferRestoreDialog_Decline.Replace("\r\n", "\n"));
			SetClickResult(DeclineButton, TaskDialogResult.No);

			Controls.AddRange(AcceptButton, DeclineButton);
			StandardButtons = TaskDialogStandardButtons.Cancel;
		}

		protected TaskDialogCommandLink AcceptButton { get; }
		protected TaskDialogCommandLink DeclineButton { get; }
	}

	internal class RestoreArrangementDialog : TaskDialog
	{
		public DateTime Latest { set {
				RestoreBackedUpArrangementButton.Text = Strings.RestoreArrangementDialog_RestoreLastSaved 
				                                        + '\n' + string.Format(Strings.RestoreArrangementDialog_RestoreLastSaved_Details, value);
		} }

		public RestoreArrangementDialog()
		{
			InstructionText = Strings.RestoreArrangementDialog_Instruction;

			RestoreBackedUpArrangementButton = new TaskDialogCommandLink(nameof(RestoreBackedUpArrangementButton), Strings.RestoreArrangementDialog_RestoreLastSaved);
			SetClickResult(RestoreBackedUpArrangementButton, TaskDialogResult.Yes);

			BrowseArrangementButton = new TaskDialogCommandLink(nameof(BrowseArrangementButton), Strings.RestoreArrangementDialog_Browse);
			SetClickResult(BrowseArrangementButton, TaskDialogResult.No);

			Controls.AddRange(RestoreBackedUpArrangementButton, BrowseArrangementButton);
			StandardButtons = TaskDialogStandardButtons.Cancel;
		}

		protected TaskDialogCommandLink RestoreBackedUpArrangementButton { get; }
		protected TaskDialogCommandLink BrowseArrangementButton { get; }
	}

	internal class BrowseArrangementsDialog
	{
		public BrowseArrangementsDialog(string backupFolder)
		{
			Dialog = new CommonOpenFileDialog
			{
				Title = Strings.BrowseArrangementsDialog_Title,
				DefaultDirectory = backupFolder, InitialDirectory = backupFolder,
				DefaultExtension = ".xml", Filters = { new CommonFileDialogFilter(Strings.BrowseArrangementsDialog_ArrangementFilter, ".xml") },
				EnsureFileExists = true
			};
		}

		protected CommonOpenFileDialog Dialog { get; }

		public bool TryBrowse(out string backupFile)
		{
			switch (Dialog.ShowDialog()) {
				case CommonFileDialogResult.Ok:
					backupFile = Dialog.FileName;
					return true;
				case CommonFileDialogResult.Cancel:
					backupFile = default;
					return false;
				default:
					throw new ArgumentOutOfRangeException(nameof(Dialog.ShowDialog));
			}
		}
	}
}
