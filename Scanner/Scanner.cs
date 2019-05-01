using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Scanner
{
	/// <summary>
	/// Items found in the text that match the pattern
	/// </summary>
	public class PatternFound
	{
		public string Name { get; private set; }
		public string Value { get; private set; }
		public int Index { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="val"></param>
		/// <param name="idx"></param>
		public PatternFound(string name, string val, int idx)
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
		public List<PatternFound> PatternsFound { get; private set; }

		/// <summary>
		/// Get a comma separated string of matches 
		/// </summary>
		/// <returns>comma separated string</returns>
		public string GetPatternsFound()
		{
			var sb = new StringBuilder();
			if (PatternsFound != null)
			{
				foreach(var f in PatternsFound.Select(x => x.Name).Distinct())
				{
					if (sb.Length == 0)
						sb.Append(f);
					else
						sb.AppendFormat($" & {f}");
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Get a comma separated string of pattern names
		/// </summary>
		/// <returns>comma separated string</returns>
		public string GetPatternNames()
		{
			var sb = new StringBuilder();
			if (patterns != null)
			{
				for (int i = 0; i < patterns.Count; i++)
				{
					if (i == 0)
						sb.Append(patterns.GetKey(i));
					else
						sb.AppendFormat($", {patterns.GetKey(i)}");
				}
			}
			return sb.ToString();
		}

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
			for (var i = 0; i < patterns.Count; i++)
			{
				if (Regex.IsMatch(data, patterns[i]))
					return patterns.GetKey(i);
			}

			return null;
		}

		/// <summary>
		/// All patterns combined separated by an '|' 
		/// </summary>
		private string Patterns
		{
			get
			{
				var retval = new StringBuilder();
				for (var i = 0; i < patterns.Count; i++)
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
			if (PatternsFound == null)
				PatternsFound = new List<PatternFound>();
			PatternsFound.Clear();
			foreach (string p in patterns)
			{
				foreach (Match m in Regex.Matches(data, patterns[p]))
				{
					string name = GetPatternName(m.Value);
					if (FilterMatch(name, m.Value) == false)
						PatternsFound.Add(new PatternFound(name, m.Value, m.Index));
				}
			}
			return PatternsFound.Count > 0;
		}
		private bool FilterMatch(string name, string value) => (ConfigurationManager.AppSettings[name] != null) ? Regex.IsMatch(value, ConfigurationManager.AppSettings[name]) : false;
	}
}
