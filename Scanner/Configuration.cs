using System.Configuration;

namespace Scanner
{
	public class PatternGroup
	{
		public static PatternSection _config = ConfigurationManager.GetSection("Patterns") as PatternSection;
		public static PatternCollection GetPattern()
		{
			return _config.Patterns;
		}
	}
	public class PatternSection : ConfigurationSection
	{
		[ConfigurationProperty("Patterns")]
		[ConfigurationCollection(typeof(PatternCollection))]
		public PatternCollection Patterns
		{
			get { return ((PatternCollection)(this["Patterns"])); }
			set { this["Patterns"] = value; }
		}
	}

	public class PatternCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new Pattern();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((Pattern)(element)).name;
		}

		public Pattern this[int idx]
		{
			get
			{
				return (Pattern)BaseGet(idx);
			}
			set
			{
				if (BaseGet(idx) != null)
					BaseRemoveAt(idx);

				BaseAdd(idx, value);
			}
		}
		public new Pattern this[string idx]
		{
			get
			{
				return (Pattern)BaseGet(idx);
			}
			set
			{
				if (BaseGet(idx) != null)
					BaseRemoveAt(BaseIndexOf(BaseGet(idx)));

				BaseAdd(value);
			}
		}
		public override bool IsReadOnly()
		{
			return false;
		}
	}

	public class Pattern : ConfigurationElement
	{
		public override string ToString()
		{
			return (string)this["name"];
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		[ConfigurationProperty("name", IsKey=true, IsRequired = true)]
		public string name
		{
			get{return (string)this["name"];}
			set { this["name"] = value;  }
		}
		[ConfigurationProperty("pattern", IsRequired = true)]
		public string pattern
		{
			get{return (string)this["pattern"];}
			set { this["pattern"] = value; }
		}
		[ConfigurationProperty("enabled", DefaultValue="true", IsRequired = false)]
		public bool enabled
		{
			get { return (bool)this["enabled"]; }
			set { this["enabled"] = value; }
		}
	}
}