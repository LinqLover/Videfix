using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Videfix.Tray.Properties;
using HelperLib.SystemHelpers.Collections.Generic;
using HelperLib.SystemHelpers.Linq;
using HelperLib.SystemHelpers.Windows;
using HelperLib.SystemHelpers.Text.RegularExpression;

namespace Videfix.Tray
{
	public class Program
	{
		public Program()
		{
			SettingsFilePath = !string.IsNullOrWhiteSpace(Settings.Default.SettingsFilePathOverride)
				? Settings.Default.SettingsFilePathOverride
				: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.CompanyName, Application.ProductName);

			_browseArrangementDialog = new Lazy<BrowseArrangementsDialog>(() => new BrowseArrangementsDialog(SettingsFilePath));

			_arrangementFiles = ArrangementDictionary.Create(SettingsFilePath, _backupFileNamePattern);
			_backupFiles = BackupDictionary.Create(SettingsFilePath, _backupFileNamePattern);
		}

		protected string SettingsFilePath { get; }

		private readonly Lazy<AboutDialog> _aboutDialog = new Lazy<AboutDialog>();
		private readonly Lazy<BrowseArrangementsDialog> _browseArrangementDialog;
		private readonly Lazy<BusyErrorDialog> _busyErrorDialog = new Lazy<BusyErrorDialog>();
		private readonly Lazy<RestoredDialog> _restoredDialog = new Lazy<RestoredDialog>();
		private readonly Lazy<RestoreArrangementDialog> _restoreArrangementDialog = new Lazy<RestoreArrangementDialog>();
		private readonly Lazy<SavedDialog> _savedDialog = new Lazy<SavedDialog>();

		private readonly ArrangementDictionary _arrangementFiles;
		private readonly BackupDictionary _backupFiles;

		private bool _isGuiActive = false;
		private bool _isBusy = false;

		[STAThread]
		public static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				if (!AdministratorHelper.IsAdmin)
					switch (new NotElevatedWarningDialog().Show())
					{
						case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Yes:
							AdministratorHelper.RestartAsAdmin();
							break;
						case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.No:
							break;
						case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Cancel:
							return;
						default:
							throw new ArgumentOutOfRangeException("NotElevatedWarningDialog.Show");
					}

				using (var pi = new ProcessIcon(new Program()))
				{
					if (pi.Display())
						Application.Run();
				}
			}
			catch (Exception exception)
			{
				new GeneralErrorDialog(exception).Show();
			}
		}

		public bool TryGetLatestArrangement(out KeyValuePair<DateTime, string> item)
		{
			return _arrangementFiles.TryGetLatest(out item);
		}

		public void RestoreArrangement()
		{
			try
			{
				if (!TryActivateGui())
					return;
				
				string backupFilePath;
				if (TryGetLatestArrangement(out var latestArrangement))
				{
					_restoreArrangementDialog.Value.Latest = latestArrangement.Key;
					_isGuiActive = true;
					switch (_restoreArrangementDialog.Value.Show())
					{
						case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Yes:
							backupFilePath = latestArrangement.Value;
							break;
						case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.No:
							_browseArrangementDialog.Value.TryBrowse(out backupFilePath);
							break;
						case Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Cancel:
							backupFilePath = default;
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(_restoreArrangementDialog.Value.Show));
					}
				}
				else
					_browseArrangementDialog.Value.TryBrowse(out backupFilePath);
				if (backupFilePath != default)
				{
					RestoreArrangement(backupFilePath);
				}
			}
			catch (Exception exception)
			{
				new GeneralErrorDialog(exception).Show();
			}
			finally
			{
				DeactivateGui();
			}
		}

		private void DeactivateGui()
		{
			_isGuiActive = false;
			_isBusy = false;
		}

		private bool TryActivateGui()
		{
			if (_isBusy)
			{
				if (_isGuiActive)
					return false;

				_isGuiActive = true;
				_busyErrorDialog.Value.Show();
				_isGuiActive = false;
				return false;
			}

			_isBusy = true;

			return true;
		}

		public void RestoreArrangement(string backupFilePath)
		{
			try
			{
				VidefixCore.Apply(backupFilePath);
				_restoredDialog.Value.Show();
			}
			catch (Exception exception)
			{
				new RestoreErrorDialog(exception).Show();
			}
		}


		public void SaveArrangement()
		{
			try
			{
				if (!TryActivateGui())
					return;

				try
				{
					var dateTime = DateTime.Now;
					var backupFile = GetArrangementFilePath(dateTime);
					VidefixCore.Save(backupFile);
					_arrangementFiles.Add(dateTime, backupFile);
					_isGuiActive = true;
					_savedDialog.Value.Show();
				}
				catch (Exception exception)
				{
					new SaveErrorDialog(exception).Show();
				}
			}
			catch (Exception exception)
			{
				new GeneralErrorDialog(exception).Show();
			}
			finally
			{
				DeactivateGui();
			}
		}

		public void BackupArrangement()
		{
			try
			{
				if (_isBusy)
					return;

				_isBusy = true;
				try
				{
					var dateTime = DateTime.Now;
					var backupFile = GetBackupFilePath(dateTime);
					VidefixCore.Save(backupFile);
					_backupFiles.Add(dateTime, backupFile);
				}
				catch (Exception exception)
				{
					new BackupErrorDialog(exception).Show();
				}
			}
			catch (Exception exception)
			{
				// TODO: Activate gui here?
				new GeneralErrorDialog(exception).Show();
			}
			finally
			{
				_isBusy = false;
			}
		}

		public void ShowAbout()
		{
			if (_isGuiActive)
				return;

			_isGuiActive = true;
			_aboutDialog.Value.LatestBackup = _backupFiles.TryGetLatest(out var latestBackup) ? latestBackup.Key : default;
			_aboutDialog.Value.Show();
			_isGuiActive = false;
		}

		protected string GetArrangementFilePath(DateTime dateTime)
		 => Path.Combine(SettingsFilePath, $"arrangement_{dateTime:yyyMMdd-HHmmss}.xml");

		protected string GetBackupFilePath(DateTime dateTime)
		 => Path.Combine(SettingsFilePath, $"backup_{dateTime:yyyMMdd-HHmmss}.xml");

		private readonly Regex _backupFileNamePattern = new Regex(@"^backup_(?'year'\d{4})(?'month'\d{2})(?'day'\d{2})-(?'hour'\d{2})(?'minute'\d{2})(?'second'\d{2})$");
	}

	internal class ArrangementDictionary
	{
		protected ArrangementDictionary(IEnumerable<KeyValuePair<DateTime, string>> dictionary)
		{
			Dictionary = new OrderedDictionary<DateTime, string>(dictionary, Comparer<DateTime>.Default.Invert());
		}

		protected OrderedDictionary<DateTime, string> Dictionary { get; }

		public static ArrangementDictionary Create(string backupDirectory, Regex backupFileNamePattern)
		{
			return new ArrangementDictionary(
				Directory.GetFiles(backupDirectory, "*")
				.SelectWhere(file => (
					isSelected: RegexHelper.TryGetDateTime(backupFileNamePattern.Match(Path.GetFileNameWithoutExtension(file)), out var dateTime),
					result: new KeyValuePair<DateTime, string>(key: dateTime, value: file))));
		}

		public virtual void Add(DateTime key, string backupFile)
		{
			Dictionary.Add(key, backupFile);
		}

		public void Remove(KeyValuePair<DateTime, string> backup)
		{
			Dictionary.Remove(backup);
			File.Delete(backup.Value);
		}
		public bool TryGetLatest(out KeyValuePair<DateTime, string> latestBackup)
		{
			return Dictionary.TryGetFirst(out latestBackup);
		}
	}

	internal class BackupDictionary : ArrangementDictionary
	{
		protected BackupDictionary(IEnumerable<KeyValuePair<DateTime, string>> dictionary) : base(dictionary)
		{
		}

		protected int BackupFilesLimit { get; } = 50;

		public new static BackupDictionary Create(string backupDirectory, Regex backupFileNamePattern)
		{
			return new BackupDictionary(
				Directory.GetFiles(backupDirectory, "*")
					.SelectWhere(file => (
						isSelected: RegexHelper.TryGetDateTime(backupFileNamePattern.Match(Path.GetFileNameWithoutExtension(file)), out var dateTime),
						result: new KeyValuePair<DateTime, string>(key: dateTime, value: file))));
		}

		public override void Add(DateTime key, string backupFile)
		{
			base.Add(key, backupFile);
			if (Dictionary.Count > BackupFilesLimit)
				CleanBackupFiles(DateTime.Now);
		}

		private void CleanBackupFiles(DateTime dateTime)
		{
			int latestCount = 5;
			int hourCount = 24;
			int dayCount = 7;
			foreach (var backup in Dictionary.ToArray())
				if (dateTime - backup.Key <= TimeSpan.FromHours(1))
				{
					if (latestCount-- <= 0)
						Remove(backup);
				}
				else if (dateTime - backup.Key <= TimeSpan.FromDays(1))
				{
					if (hourCount-- <= 0)
					{
						Remove(backup);
						dateTime = backup.Key;
					}
				}
				else
				if (dayCount-- <= 0)
					Remove(backup);
		}
	}
}