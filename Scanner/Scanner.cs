using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace Scanner
{
	/// <summary>
	/// Items found in the text that match the pattern
	/// </summary>
	public class Found
	{
		public string Name { get; private set; }
		public string Value { get; private set; }
		public int Index { get; private set; }

		public Found(string name, string val, int idx)
		{
			Name = name;
			Value = val;
			Index = idx;
		}
	}

	/// <summary>
	/// Engine to scan memory
	/// </summary>
	public class ScanEngine
	{
		public List<Found> Matches { get; private set; }

		private NameValueCollection patterns;

		/// <summary>
		/// Load the patterns to scan
		/// </summary>
		/// <returns>Number of patterns</returns>
		public int LoadPatterns()
		{
			if (patterns == null)
				patterns = ConfigurationManager.GetSection("PatternGroup/Patterns") as NameValueCollection;
			return patterns.Count;
		}

		/// <summary>
		/// Get the pattern name for a given item
		/// </summary>
		/// <param name="data">data to evaluate</param>
		/// <returns>name of pattern</returns>
		private string GetPatternName(string data)
		{
			for (int i = 0; i < patterns.Count; i++)
				if (Regex.IsMatch(data, patterns[i]))
					return patterns.GetKey(i);
			return null;
		}

		/// <summary>
		/// All patterns combined separated by an '|' 
		/// </summary>
		private string Patterns
		{
			get
			{
				StringBuilder retval = new StringBuilder();
				for (int i = 0; i < patterns.Count; i++)
				{
					if (i == 0)
						retval.Append(patterns[i]);
					else
						retval.AppendFormat("|{0}", patterns[i]);
				}
				return retval.ToString();
			}
		}

		/// <summary>
		/// Scan memory for matches to the patterns provided
		/// </summary>
		/// <param name="data">memory to scan</param>
		/// <returns>True = Found Match; False = No Matches</returns>
		public bool Scan(string data)
		{
			if (patterns == null || patterns.Count == 0)
				throw new ApplicationException("No patterns defined");
			Regex rgx = new Regex(Patterns);
			if (Matches == null)
				Matches = new List<Found>();
			Matches.Clear();
			foreach (Match match in rgx.Matches(data))
			{
				Matches.Add(new Found(GetPatternName(match.Value), match.Value, match.Index));
			}	
			return Matches.Count > 0;
		}
	}
}
