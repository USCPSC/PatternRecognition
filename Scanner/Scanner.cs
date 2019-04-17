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
	public class Found
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

		/// <summary>
		/// Get a comma separated string of matches 
		/// </summary>
		/// <returns>comma separated string</returns>
		public string GetMatchNames()
		{
			var sb = new StringBuilder();
			if (Matches != null)
			{
				foreach(var f in Matches.Select(x => x.Name).Distinct())
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
			for (int i = 0; i < patterns.Count; i++)
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
			var rgx = new Regex(Patterns);
			if (Matches == null)
				Matches = new List<Found>();
			Matches.Clear();
			Matches.AddRange(from Match match in rgx.Matches(data)
								  select new Found(GetPatternName(match.Value), match.Value, match.Index));
			return Matches.Count > 0;
		}
	}
}
