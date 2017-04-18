using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsReplace {
    public interface IGetKeywordsData {
        void SetKeywordsData(Dictionary<string, List<string>> container);
    }

    public class DefaultGetKeywordsData : IGetKeywordsData {
        public void SetKeywordsData(Dictionary<string, List<string>> container) {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"keywords.txt")) {
                while (sr.Peek() >= 0) {
                    var item = sr.ReadLine().Split('/');
                    if (!container.ContainsKey(item[0])) {
                        container.Add(item[0], new List<string> { item[1] });
                    } else {
                        container[item[0]].Add(item[1]);
                    }
                }
            }
        }
    }
}
