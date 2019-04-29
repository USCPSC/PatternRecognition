﻿using ExcelDataReader;
using FileManager;
using Newtonsoft.Json;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;

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
							return new FileContents(JsonConvert.SerializeObject(ds), imageScan ? FileContents.None : FileContents.NotApplicable);
						}
					}
				}
			}
			return new FileContents("", FileContents.None);
		}
	}
}
