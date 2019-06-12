using System;
using System.Configuration;
using System.Windows.Forms;

namespace PatternSearchUI
{
	public enum LastDialog {  Batch, File }

	static class Program
	{
		const string key = "LastDialog";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			string val = ConfigurationManager.AppSettings[key];
			LastDialog ld;
			if (Enum.TryParse(val, out ld) == false)
				ld = LastDialog.Batch;
			switch(ld)
			{
				case LastDialog.Batch:
					Application.Run(new BatchProcessing());
					break;
				case LastDialog.File:
					Application.Run(new FileProcessing());
					break;
			}
		}
		public static void UpdateLastDialog(LastDialog lastDiag)
		{
			try
			{
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var settings = configFile.AppSettings.Settings;
				if (settings[key] == null)
				{
					settings.Add(key, lastDiag.ToString());
				}
				else
				{
					settings[key].Value = lastDiag.ToString();
				}
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error writing app settings");
			}
		}
	}
}
