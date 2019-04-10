using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagement
{
	public interface IFileManager
	{
		string FileExtention { get; }
		string ReadAllText(string filename);
    }
}
