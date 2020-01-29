using System;
using System.Diagnostics;
using System.Windows.Forms;
using Videfix.Tray.Properties;

namespace Videfix.Tray
{
	internal class ProcessIcon : IDisposable
	{
		protected Program Program { get; }

		/// <summary>
		/// The NotifyIcon object.
		/// </summary>
		private readonly NotifyIcon _notifyIcon;

		private readonly Timer _backupTimer = new Timer();

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessIcon"/> class.
		/// </summary>
		public ProcessIcon(Program program)
		{
			Program = program ?? throw new ArgumentNullException(nameof(program));

			// Instantiate the NotifyIcon object.
			_notifyIcon = new NotifyIcon();

			// Put the icon in the system tray and allow it react to mouse clicks.			
			_notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
			_notifyIcon.Icon = Resources.AppIcon;
			_notifyIcon.Text = Strings.SystemTray_AppTitle;
			
			// Attach a context menu.
			_notifyIcon.ContextMenuStrip = new ContextMenu(Program).Create();


			_backupTimer.Interval = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
			_backupTimer.Tick += (_, __) => Program.BackupArrangement();
		}

		/// <summary>
		/// Displays the icon in the system tray.
		/// </summary>
		public bool Display()
		{
			_backupTimer.Enabled = true;
			_notifyIcon.Visible = true;



			if (Program.TryGetLatestArrangement(out var latestArrangement) && VidefixCore.CheckNeedsRestore())
			{
				switch (new OfferRestoreDialog(latestArrangement.Key).Show())
				{
					case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Yes:
						Program.RestoreArrangement(latestArrangement.Value);
						return true;
					case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.No:
						break;
					case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Cancel:
						Application.Exit();
						return false;
					default:
						throw new ArgumentOutOfRangeException("NotElevatedWarningDialog.Show");
				}
			}

			Program.BackupArrangement();
			return true;
		}

		/// <inheritdoc />
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		public void Dispose()
		{
			// When the application closes, this will remove the icon from the system tray immediately.
			_notifyIcon.Dispose();
		}

		/// <summary>
		/// Handles the MouseClick event of the ni control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			// Handle mouse button clicks.
			if (e.Button == MouseButtons.Left)
			{
				Program.ShowAbout();
			}
		}
	}
}