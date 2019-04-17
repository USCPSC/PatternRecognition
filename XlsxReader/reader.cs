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
		private string ConvertToCSV(DataSet objDataSet)
		{
			StringBuilder content = new StringBuilder();

			if (objDataSet.Tables.Count >= 1)
			{
				DataTable table = objDataSet.Tables[0];

				if (table.Rows.Count > 0)
				{
					DataRow dr1 = (DataRow)table.Rows[0];
					int intColumnCount = dr1.Table.Columns.Count;
					int index = 1;

					//add column names
					foreach (DataColumn item in dr1.Table.Columns)
					{
						content.Append(string.Format("\"{0}\"", item.ColumnName));
						if (index < intColumnCount)
							content.Append(",");
						else
							content.Append("\r\n");
						index++;
					}

					//add column data
					foreach (DataRow currentRow in table.Rows)
					{
						string strRow = string.Empty;
						for (int y = 0; y <= intColumnCount - 1; y++)
						{
							strRow += "\"" + currentRow[y].ToString() + "\"";

							if (y < intColumnCount - 1 && y >= 0)
								strRow += ",";
						}
						content.Append(strRow + "\r\n");
					}
				}
			}

			return content.ToString();
		}
		public FileContents ReadAllText(string filename)
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
						DataSet ds = reader.AsDataSet();
						return new FileContents(JsonConvert.SerializeObject(ds), false);
					}
				}
			}
			return new FileContents("", false);
		}
	}
}
