using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Pattern
{
    public class Scanner
    {
        public List<string> matches = new List<string>(); 

        public int LoadPatterns(List<string> patterns)
        {
            if (patterns == null)
            {
                patterns = new List<string>();
                ConfigXmlDocument 

                var PostSetting = ConfigurationManager.GetSection("BlogGroup/PostSetting") as NameValueCollection;
                if (PostSetting.Count == 0)
                {
                    Console.WriteLine("Post Settings are not defined");
                }
                else
                {
                    foreach (var key in PostSetting.AllKeys)
                    {
                        Console.WriteLine(key + " = " + PostSetting[key]);
                    }
                }
            }
            return patterns.Count;
        }
        public bool Scan(string data)
        {
            Regex regex = new Regex();
            return true;
        }
    }
}
