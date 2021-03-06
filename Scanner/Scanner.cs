﻿using System;
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
		public PatternCollection Patterns;
		private NameValueCollection filters;

		/// <summary>
		/// Contructor
		/// </summary>
		public ScanEngine()
		{
			LoadFilters();
		}

		/// <summary>
		/// Patterns found
		/// </summary>
		public List<PatternFound> PatternsFound { get; private set; }

		/// <summary>
		/// Get a comma separated string of matches 
		/// </summary>
		/// <returns>comma separated string</returns>
		public string PatternsFoundAsString
		{
			get
			{
				var sb = new StringBuilder();
				if (PatternsFound != null)
				{
					foreach (var f in PatternsFound.Select(x => x.Name).Distinct())
						sb = (sb.Length == 0) ? sb.Append(f) : sb.AppendFormat($" & {f}");
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// Get a comma separated string of pattern names
		/// </summary>
		/// <returns>comma separated string</returns>
		public string GetPatternNames()
		{
			var sb = new StringBuilder();
			if (Patterns != null)
			{
				foreach (Pattern p in Patterns)
				{
					if (p.enabled == true)
						sb = (sb.Length == 0)? sb.Append(p.name): sb.AppendFormat($"; {p.name}");
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Load the patterns to scan
		/// </summary>
		/// <returns>Number of patterns</returns>
		public int LoadPatterns()
		{
			if (Patterns == null)
			{
				var sec = (PatternSection)ConfigurationManager.GetSection("PatternGroup");
				Patterns = sec.Patterns;
			}
			return Patterns.Count;
		}
		public void SavePatterns()
		{
			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var sec = (PatternSection)config.GetSection("PatternGroup");
			sec.Patterns = Patterns;
			config.Save(ConfigurationSaveMode.Modified);
		}

		/// <summary>
		/// Load the filters
		/// </summary>
		/// <returns>Number of filters</returns>
		private int LoadFilters()
		{
			if (filters == null)
				filters = ConfigurationManager.GetSection("FilterGroup/Filters") as NameValueCollection;
			return filters.Count;
		}
		/// <summary>
		/// Get the pattern name for a given item
		/// </summary>
		/// <param name="data">data to evaluate</param>
		/// <returns>name of pattern</returns>
		private string GetPatternName(string data)
		{
			foreach ( Pattern p in Patterns)
			{ 
				if (p.enabled == true && Regex.IsMatch(data, p.pattern))
					return p.name;
			}

			return null;
		}

		/// <summary>
		/// Scan memory for matches to the patterns provided
		/// </summary>
		/// <param name="data">memory to scan</param>
		/// <returns>True = Found Match; False = No Matches</returns>
		public bool Scan(string data)
		{
			if (Patterns == null || Patterns.Count == 0)
				throw new ApplicationException("No patterns defined");

			if (PatternsFound == null)
				PatternsFound = new List<PatternFound>();
			else
				PatternsFound.Clear();
			foreach (Pattern p in Patterns)
			{
				if (p.enabled == true)
				{
					foreach (Match m in Regex.Matches(data, p.pattern))
					{
						string name = GetPatternName(m.Value);
						if (FilterMatch(name, m.Value) == false)
							PatternsFound.Add(new PatternFound(name, m.Value, m.Index));
					}
				}
			}
			return PatternsFound.Count > 0;
		}
		/// <summary>
		/// If there a match for a filter
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private bool FilterMatch(string name, string value) => (filters[name] != null) ? Regex.IsMatch(value, filters[name]) : false;
	}
}
