using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WordsReplace {
    public class WordsReplace {
        #region m_
        private int m_formatIndex = 0;
        private List<string> m_Reg_Keywords;
        private ConcurrentDictionary<string, int> m_map = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, List<string>> m_Format_KeywordsValues = new ConcurrentDictionary<string, List<string>>();
        #endregion

        #region c_
        private const int c_Content_MaxLengthPer = 300;
        private const int c_Regex_MaxLengthPer = 1000;
        private const string c_Format = @"#「{0}」";
        #endregion

        #region protected m_
        protected Dictionary<string, List<string>> m_KeywordsValues = new Dictionary<string, List<string>>();
        #endregion

        #region Singleton
        private static WordsReplace m_Singleton;
        private WordsReplace() {
            Reload();
        }

        public static WordsReplace Instance() {
            if (m_Singleton == null) {
                m_Singleton = new WordsReplace();
            }
            return m_Singleton;
        }
        #endregion

        #region public method
        public void Reload() {
            Load();
            KeyWordsRegroup();
        }

        public string Repalce(string content) {
            var result = string.Empty;
            if (string.IsNullOrEmpty(content)) return result;

            Dictionary<int, string> splitedContent = new Dictionary<int, string>();
            Dictionary<int, string> splitedContentClone = new Dictionary<int, string>();
            var splited = content.SplitItems(c_Content_MaxLengthPer);
            for (int i = 0; i < splited.Length; i++) {
                splitedContent[i] = splited[i];
                splitedContentClone[i] = splited[i];
            }
            var task = Parallel.ForEach(splitedContent, (kv) => {
                m_Reg_Keywords.ForEach(reg => {
                    splitedContentClone[kv.Key] = ReplaceByReg(splitedContentClone[kv.Key], reg);
                });
            });

            while (!task.IsCompleted) {
            }
            var lastContent = string.Join("", splitedContentClone.Values);
            m_Format_KeywordsValues.ToList().ForEach(formatKV => {
                lastContent = Regex.Replace(lastContent, formatKV.Key, (matched) => {
                    if (m_Format_KeywordsValues.ContainsKey(matched.Value)) {
                        return RandomSelect(m_Format_KeywordsValues[matched.Value]);
                    }
                    return matched.Value;
                });
            });

            return lastContent;
        }

        public List<string> Repalce(IEnumerable<string> contents) {
            var result = new List<string>();
            if (contents == null || contents.Count() <= 0) return result;
            contents.ToList().ForEach(content => {
                result.Add(Repalce(content));
            });
            return result;
        }
        #endregion

        #region protected method
        protected virtual void Load() {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"keywords.txt")) {
                while (sr.Peek() >= 0) {
                    var item = sr.ReadLine().Split('/');
                    if (!m_KeywordsValues.ContainsKey(item[0])) {
                        m_KeywordsValues.Add(item[0], new List<string> { item[1] });
                    } else {
                        m_KeywordsValues[item[0]].Add(item[1]);
                    }
                }
            }
        }
        #endregion

        #region private method
        private void KeyWordsRegroup() {
            var sorted = m_KeywordsValues.Keys.OrderByDescending(_ => _.Length).ToList();
            var spilted = sorted.SplitItems(c_Regex_MaxLengthPer);
            m_Reg_Keywords = new List<string>();
            spilted.ForEach(keyword => m_Reg_Keywords.Add(string.Join("|", keyword).TrimEnd('|')));
        }

        private string ReplaceByReg(string content, string pattern) {
            return Regex.Replace(content, pattern, (matched) => {
                if (m_KeywordsValues.ContainsKey(matched.Value)) {
                    if (!m_map.ContainsKey(matched.Value)) {
                        Interlocked.Increment(ref m_formatIndex);
                        m_map.GetOrAdd(matched.Value, m_formatIndex);
                    }

                    var key = string.Format(c_Format, m_map[matched.Value]);
                    if (!m_Format_KeywordsValues.ContainsKey(key)) {
                        m_Format_KeywordsValues.GetOrAdd(key, m_KeywordsValues[matched.Value]);
                    }
                    return key;
                }
                return matched.Value;
            });
        }

        private string RandomSelect(List<string> items) {
            var itemCount = items.Count();
            if (itemCount == 1) {
                return items[0];
            }
            Random rd = new Random();
            return items[rd.Next(0, itemCount)];
        }
        #endregion
    }

    public static class ItemsSplitExtension {
        public static List<IEnumerable<TSource>> SplitItems<TSource>(this IEnumerable<TSource> source, int maxPerCount) {
            var result = new List<IEnumerable<TSource>>();
            if (source == null) {
                return null;
            }
            var totalLength = source.Count();

            int splitCount, perCount, lastItemLength;
            SplitCalc(out splitCount, out perCount, out lastItemLength, totalLength, maxPerCount);

            for (int i = 0; i < splitCount; i++) {
                result.Add(source.Skip(i * perCount).Take(perCount).ToList());
            }
            if (lastItemLength != 0) {
                result.Add(source.Skip(source.Count() - lastItemLength).Take(lastItemLength).ToList());
            }

            return result;
        }

        public static string[] SplitItems(this string source, int maxPerCount) {
            if (string.IsNullOrEmpty(source)) {
                return null;
            }
            var totalLength = source.Length;
            int splitCount, perCount, lastItemLength, itemCount;
            SplitCalc(out splitCount, out perCount, out lastItemLength, out itemCount, totalLength, maxPerCount);

            string[] splited = new string[itemCount];
            for (int i = 0; i < splitCount; i++) {
                splited[i] = source.Substring(i * perCount, perCount);
            }
            if (lastItemLength != 0) {
                splited[itemCount - 1] = source.Substring(source.Length - lastItemLength, lastItemLength);
            }
            return splited;
        }

        private static void SplitCalc(out int splitCount, out int perCount, out int lastItemLength, int totalLength, int maxPerCount) {
            int itemCount;
            SplitCalc(out splitCount, out perCount, out lastItemLength, out itemCount, totalLength, maxPerCount);
        }

        private static void SplitCalc(out int splitCount, out int perCount, out int lastItemLength, out int itemCount, int totalLength, int maxPerCount) {
            splitCount = 1;

            lastItemLength = 0;
            if (maxPerCount < totalLength) {
                splitCount = totalLength / maxPerCount;
                if (totalLength % maxPerCount != 0) {
                    splitCount = splitCount + 1;
                }

            }
            itemCount = splitCount;
            perCount = totalLength / splitCount;

            var mod = totalLength % splitCount;
            if (mod != 0) {
                itemCount = splitCount + 1;
                lastItemLength = mod;
            }
        }
    }
}
