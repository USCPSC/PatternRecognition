using ExcelDataReader;
using FileManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Text;

namespace XlsxReader
{
	[Export(typeof(IFileManager))]
	public class Reader : IFileManager
	{
		public string[] FileExtention => new string[] { ".xlsx", ".xls" };

		private static IList<string> GetTablenames(DataTableCollection tables)
		{
			var tableList = new List<string>();
			foreach (var table in tables)
			{
				tableList.Add(table.ToString());
			}

			return tableList;
		}

		private string GetValues(DataSet dataset, string sheetName)
		{
			StringBuilder sb = new StringBuilder();
			foreach (DataRow row in dataset.Tables[sheetName].Rows)
			{
				foreach (var value in row.ItemArray)
				{
					sb.AppendFormat(@"{value}");
				}
			}
			return sb.ToString();
		}

		public FileContents ReadAllText(string filename, bool imageScan)
		{
			using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				IExcelDataReader reader = null;
				
				switch (Path.GetExtension(filename))
				{
					case ".xls":
						reader = ExcelReaderFactory.CreateBinaryReader(stream);
						break;
					case ".xlsx":
						reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
						break;
				}
				if (reader != null)
				{
					using (reader)
					{
						using (DataSet ds = reader.AsDataSet())
						{
							return new FileContents(JsonConvert.SerializeObject(ds), imageScan ? FileContents.None : FileContents.NotApplicable);
						}
					}
				}
			}
			return new FileContents("", FileContents.None);
		}
	}
}
