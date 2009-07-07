using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Gibbed.Valve.FileFormats;

namespace Gibbed.Valve.ExtractPackage
{
	public partial class Extractor : Form
	{
		public Extractor()
		{
			this.InitializeComponent();
		}

		delegate void SetProgressDelegate(long percent);
		private void SetProgress(long percent)
		{
			if (this.progressBar.InvokeRequired)
			{
				SetProgressDelegate callback = new SetProgressDelegate(SetProgress);
				this.Invoke(callback, new object[] { percent });
				return;
			}
			
			this.progressBar.Value = (int)percent;
		}

		delegate void LogDelegate(string message);
		private void Log(string message)
		{
			if (this.logText.InvokeRequired)
			{
				LogDelegate callback = new LogDelegate(Log);
				this.Invoke(callback, new object[] { message });
				return;
			}

			if (this.logText.Text.Length == 0)
			{
				this.logText.AppendText(message);
			}
			else
			{
				this.logText.AppendText(Environment.NewLine + message);
			}
		}

		delegate void EnableButtonsDelegate(bool extract);
		private void EnableButtons(bool extract)
		{
			if (this.extractButton.InvokeRequired || this.cancelButton.InvokeRequired)
			{
				EnableButtonsDelegate callback = new EnableButtonsDelegate(EnableButtons);
				this.Invoke(callback, new object[] { extract });
				return;
			}

			this.extractButton.Enabled = extract ? true : false;
			this.cancelButton.Enabled = extract ? false : true;
		}

		private void OnOpen(object sender, EventArgs e)
		{
			if (this.openFileDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			string indexName = this.openFileDialog.FileName;
			if (indexName.EndsWith("_dir.vpk") == false)
			{
				MessageBox.Show(String.Format("{0} does not end in _dir.vpk.", Path.GetFileName(indexName)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.savePathDialog.SelectedPath = Path.GetDirectoryName(indexName);
			if (this.savePathDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			string basePath = indexName.Substring(0, indexName.Length - 8);
			string savePath = this.savePathDialog.SelectedPath;

            PackageFile package = new PackageFile();
			Stream indexStream = File.OpenRead(indexName);
			package.Deserialize(indexStream);
			indexStream.Close();

			this.progressBar.Minimum = 0;
			this.progressBar.Maximum = package.Entries.Count;
			this.progressBar.Value = 0;

			ExtractThreadInfo info = new ExtractThreadInfo();
			info.BasePath = basePath;
			info.SavePath = savePath;
			info.Package = package;

			this.ExtractThread = new Thread(new ParameterizedThreadStart(ExtractFiles));
			this.ExtractThread.Start(info);
			this.EnableButtons(false);
		}

		private Thread ExtractThread;
		private class ExtractThreadInfo
		{
			public string BasePath;
			public string SavePath;
			public PackageFile Package;
		}

		public void ExtractFiles(object oinfo)
		{
			long succeeded, failed, current;
			ExtractThreadInfo info = (ExtractThreadInfo)oinfo;
			Dictionary<string, Stream> archiveStreams = new Dictionary<string, Stream>();

			succeeded = failed = current = 0;

			this.Log(String.Format("{0} files in package.", info.Package.Entries.Count));

			foreach (PackageEntry entry in info.Package.Entries)
			{
				this.SetProgress(++current);

				if (entry.Size == 0 && entry.SmallData.Length > 0)
				{
					Directory.CreateDirectory(Path.Combine(info.SavePath, entry.DirectoryName));
					string smallDataName = Path.Combine(entry.DirectoryName, Path.ChangeExtension(entry.FileName, entry.TypeName));
					this.Log(smallDataName);
					Stream smallDataStream = File.OpenWrite(Path.Combine(info.SavePath, smallDataName));
					smallDataStream.Write(entry.SmallData, 0, entry.SmallData.Length);
					smallDataStream.Close();
					succeeded++;
				}
				else
				{
					string archiveName = string.Format("{0}_{1:D3}.vpk", info.BasePath, entry.ArchiveIndex);
					Stream archiveStream;

					if (archiveStreams.ContainsKey(archiveName) == true)
					{
						if (archiveStreams[archiveName] == null)
						{
							failed++;
							continue;
						}

						archiveStream = archiveStreams[archiveName];
					}
					else
					{
						if (File.Exists(archiveName) == false)
						{
							this.Log(String.Format("Missing package {0}", Path.GetFileName(archiveName)));
							archiveStreams[archiveName] = null;
							failed++;
							continue;
						}

						archiveStream = File.OpenRead(archiveName);
						archiveStreams[archiveName] = archiveStream;
					}

					archiveStream.Seek(entry.Offset, SeekOrigin.Begin);

					string outputName = Path.Combine(entry.DirectoryName, Path.ChangeExtension(entry.FileName, entry.TypeName));
					this.Log(outputName);

					Directory.CreateDirectory(Path.Combine(info.SavePath, entry.DirectoryName));
					Stream outputStream = File.OpenWrite(Path.Combine(info.SavePath, outputName));

					long left = entry.Size;
					byte[] data = new byte[4096];
					while (left > 0)
					{
						int block = (int)(Math.Min(left, 4096));
						archiveStream.Read(data, 0, block);
						outputStream.Write(data, 0, block);
						left -= block;
					}

					outputStream.Close();

					if (entry.SmallData.Length > 0)
					{
						string smallDataName = Path.Combine(info.SavePath, Path.ChangeExtension(outputName, entry.TypeName + ".smalldata"));
						Stream smallDataStream = File.OpenWrite(smallDataName);
						smallDataStream.Write(entry.SmallData, 0, entry.SmallData.Length);
						smallDataStream.Close();
					}

					succeeded++;
				}
			}

			foreach (Stream stream in archiveStreams.Values)
			{
				stream.Close();
			}

			this.Log(String.Format("Done, {0} succeeded, {1} failed, {2} total.", succeeded, failed, info.Package.Entries.Count));
			this.EnableButtons(true);
		}

		private void OnCancel(object sender, EventArgs e)
		{
			if (this.ExtractThread != null)
			{
				this.ExtractThread.Abort();
			}

			this.Close();
		}
	}
}
