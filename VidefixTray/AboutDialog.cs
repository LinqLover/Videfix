using System;
using System.Diagnostics;
using HelperLib.MicrosoftHelpers.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Videfix.Tray.Properties;
using System.Windows.Forms;
using HelperLib;
using Microsoft.Win32;

namespace Videfix.Tray
{
	internal class AboutDialog : TaskDialog
	{
		public AboutDialog()
		{
			//Icon = TaskDialogStandardIcon.Information;

			InstructionText = Strings.Dialog_AppTitle;
			Text = Strings.AboutDialog_Description
				+ Environment.NewLine + Environment.NewLine
				+ Strings.AboutDialog_Copyright;

			LatestBackup = default;

			try
			{
				_startWithWindowsRegistry = (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true), Process.GetCurrentProcess().MainModule.FileName);
				FooterCheckBoxText = Strings.AboutDialog_StartWithWindows;
			}
			catch (System.Security.SecurityException)
			{
			}
		}

		public DateTime? LatestBackup
		{
			get => _latestBackup;
			set
			{
				_latestBackup = value;
				DetailsExpandedText = string.Format(Strings.AboutDialog_LatestBackupCreated,
					_latestBackup?.ToString() ?? Strings.AboutDialog_NoBackupCreated);
			}
		}

		private DateTime? _latestBackup;
		private readonly (RegistryKey key, string value) _startWithWindowsRegistry;

		public new void Show()
		{
			var startWithWindows = _startWithWindowsRegistry != default ? (_startWithWindowsRegistry.key.GetValue(Strings.Registry_ApplicationName) != null) : default;
			FooterCheckBoxChecked = startWithWindows;
			base.Show();
			if (_startWithWindowsRegistry != default && FooterCheckBoxChecked != startWithWindows)
			{
				try
				{
					// SecurityException, IOException, UnauthorizedAccessException
					if ((bool)FooterCheckBoxChecked)
						_startWithWindowsRegistry.key.SetValue(Strings.Registry_ApplicationName, _startWithWindowsRegistry.value);
					else
						_startWithWindowsRegistry.key.DeleteValue(Strings.Registry_ApplicationName);
				}
				catch (Exception exception)
				{
					new ConfigureStartWithWindowsErrorDialog(exception).Show();
				}
			}
		}
	}
}