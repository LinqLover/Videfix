using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Videfix.Tray.Properties;

namespace Videfix.Tray
{
	internal class ContextMenu
	{
		protected Program Program { get; }

		public ContextMenu(Program program)
		{
			Program = program ?? throw new ArgumentNullException(nameof(program));
		}

		/// <summary>
		/// Creates this instance.
		/// </summary>
		/// <returns>ContextMenuStrip</returns>
		public ContextMenuStrip Create()
		{
			var menu = new ContextMenuStrip();

			// TODO: Icons!
			{
				var aboutItem = new ToolStripMenuItem {
					Text = Strings.ContextMenu_About_String
				};
				aboutItem.Click += About_Click;
				//aboutItem.Image = Resources.ContextMenu_About_Icon;
				menu.Items.Add(aboutItem);
			}

			menu.Items.Add(new ToolStripSeparator());

			{
				var saveItem = new ToolStripMenuItem {
					Text = Strings.ContextMenu_Save_String
				};
				saveItem.Click += Save_Click;
				//saveItem.Image = Resources.ContextMenu_Save_Icon;
				menu.Items.Add(saveItem);
			}

			{
				var restoreItem = new ToolStripMenuItem {
					Text = Strings.ContextMenu_Restore_String
				};
				restoreItem.Click += Restore_Click;
				//restoreItem.Image = Resources.ContextMenu_Restore_Icon;
				menu.Items.Add(restoreItem);
			}

			menu.Items.Add(new ToolStripSeparator());

			{
				var exitItem = new ToolStripMenuItem {
					Text = Strings.ContextMenu_Exit_String
				};
				exitItem.Click += Exit_Click;
				//exitItem.Image = Resources.ContextMenu_Exit_Icon;
				menu.Items.Add(exitItem);
			}

			return menu;
		}

		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void About_Click(object sender, EventArgs eventArgs)
		{
			Program.ShowAbout();
		}

		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		public void Restore_Click(object sender, EventArgs eventArgs)
		{
			Program.RestoreArrangement();
		}

		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Save_Click(object sender, EventArgs eventArgs)
		{
			Program.SaveArrangement();
		}

		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Exit_Click(object sender, EventArgs eventArgs)
		{
			Program.BackupArrangement();
			Application.Exit();
		}
	}
}