using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsReplace {
    public interface IGetKeywordsData {
        Dictionary<string, List<string>> SetKeywordsData();
    }

    public class DefaultGetKeywordsData : IGetKeywordsData {
        public Dictionary<string, List<string>> SetKeywordsData() {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"keywords.txt")) {
                while (sr.Peek() >= 0) {
                    var item = sr.ReadLine().Split('/');
                    if (!result.ContainsKey(item[0])) {
                        result.Add(item[0], new List<string> { item[1] });
                    } else {
                        result[item[0]].Add(item[1]);
                    }
                }
            }
            return result;
        }
    }
}
