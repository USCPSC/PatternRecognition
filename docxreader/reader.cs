﻿using FileManager;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Xceed.Words.NET;

namespace DocxFileReader
{
	[Export(typeof(IFileReader))]
	public class Reader : IFileReader
	{
		public string[] FileExtention => new string[] { ".docx" };

		public FileContents ReadAllText(string filename, bool imageScan)
		{
			using (DocX document = DocX.Load(filename))
			{
				StringBuilder sb = new StringBuilder(FileContents.NotApplicable);
				if (imageScan)
				{
					Dictionary<string, int> docimages = new Dictionary<string, int>();
					foreach (var i in document.Images)
					{
						string fileType = Path.GetExtension(i.FileName).TrimStart('.');
						if (docimages.ContainsKey(fileType))
							++docimages[fileType];
						else
							docimages.Add(fileType, 1);
					}
					sb.Clear();
					if (docimages.Count == 0)
						sb.Append(FileContents.None);
					else
					{
						foreach (var k in docimages.Keys)
							sb.AppendFormat($"{k}: {docimages[k]} ");
					}
				}
				return new FileContents(document.Text, sb.ToString());
			}
		}
	}
}
