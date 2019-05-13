using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;
using FileManager;

namespace XlsxReader
{
	[Export(typeof(IFileReader))]
	public class Reader : IFileReader
	{
		public string[] FileExtention => new string[] { ".xlsx", ".xls" };

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
							return new FileContents(ConvertToCSV(ds), imageScan ? FileContents.None : FileContents.NotApplicable);
						}
					}
				}
			}
			return new FileContents("", FileContents.None);
		}
		private string ConvertToCSV(DataSet objDataSet)
		{
			StringBuilder content = new StringBuilder();
			string colFilter = ConfigurationManager.AppSettings["Columns2Search"];
			List<int> cols = new List<int>();
			if (objDataSet.Tables.Count >= 1)
			{
				foreach (DataTable table in objDataSet.Tables)
				{
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
							if (colFilter?.Length > 0)
							{
								if (Regex.IsMatch(item.ColumnName, colFilter))
									cols.Add(item.Ordinal);
							}
						}

						//add column data
						foreach (DataRow currentRow in table.Rows)
						{
							string strRow = string.Empty;
							for (int y = 0; y <= intColumnCount - 1; y++)
							{
								bool addCol = true;
								if (cols.Contains(y) == true)
									addCol = false;

								if (addCol)
									strRow += "\"" + currentRow[y].ToString() + "\"";

								if (y < intColumnCount - 1 && y >= 0)
									strRow += ",";
							}
							content.Append(strRow + "\r\n");
						}
					}
				}
			}
			return content.ToString();
		}
	}
}
