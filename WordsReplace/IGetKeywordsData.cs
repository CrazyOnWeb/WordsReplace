using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordsReplace {
    public interface IGetKeywordsData {
        void GetKeywordsData(Dictionary<string, List<string>> container);
    }

    public class DefaultKeywordsDataProvider : IGetKeywordsData {
        public void GetKeywordsData(Dictionary<string, List<string>> container) {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"keywords.txt")) {
                while (sr.Peek() >= 0) {
                    var item = sr.ReadLine().Split('/');
                    var key = item[0];
                    if (!container.ContainsKey(key)) {
                        container.Add(key, new List<string> { item[1] });
                    } else {
                        container[key].Add(item[1]);
                    }
                }
            }
        }
    }
}
