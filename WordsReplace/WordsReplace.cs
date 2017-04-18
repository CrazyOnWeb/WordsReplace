﻿using System;
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
        private List<string> m_KeywordsReg_Regroup;
        private ConcurrentDictionary<string, int> m_IncrementNumberByKeyword;
        private ConcurrentDictionary<string, string> m_KeywordsRelations;
        private readonly Dictionary<string, List<string>> m_KeywordsValues = new Dictionary<string, List<string>>();
        #endregion

        #region c_
        private const int c_Content_MaxLengthPer = 300;
        private const int c_Regex_MaxLengthPer = 1000;
        private const string c_Format = @"#「{0}」";
        #endregion

        #region Singleton
        private static WordsReplace m_Singleton;
        private WordsReplace() {
        }

        public static WordsReplace Instance() {
            if (m_Singleton == null) {
                m_Singleton = new WordsReplace();
            }
            return m_Singleton;
        }
        #endregion

        #region public method
        public void SetKeywordsData(IGetKeywordsData getKeywordsData) {
            if (m_KeywordsValues != null) {
                m_KeywordsValues.Clear();
            }

            getKeywordsData.SetKeywordsData(m_KeywordsValues);
            KeyWordsRegroup();
        }

        public string Repalce(string content) {
            if (m_KeywordsValues == null || m_KeywordsValues.Count <= 0)
                return content;

            var result = string.Empty;
            if (string.IsNullOrEmpty(content)) return result;
            Reset();
            Dictionary<int, string> splitedContent = new Dictionary<int, string>();
            Dictionary<int, string> splitedContentClone = new Dictionary<int, string>();
            var splited = content.SplitItems(c_Content_MaxLengthPer);
            for (int i = 0; i < splited.Length; i++) {
                splitedContent[i] = splited[i];
                splitedContentClone[i] = splited[i];
            }
            var task = Parallel.ForEach(splitedContent, (kv) => {
                m_KeywordsReg_Regroup.ForEach(reg => {
                    splitedContentClone[kv.Key] = ReplaceByReg(splitedContentClone[kv.Key], reg);
                });
            });

            while (!task.IsCompleted) {
            }
            var lastContent = string.Join("", splitedContentClone.Values);
            m_KeywordsRelations.ToList().ForEach(formatKV => {
                lastContent = Regex.Replace(lastContent, formatKV.Key, (matched) => {
                    if (m_KeywordsRelations.ContainsKey(matched.Value)) {
                        var key = m_KeywordsRelations[matched.Value];
                        return RandomSelect(m_KeywordsValues[key]);
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
        #region private method
        private void Reset() {
            m_formatIndex = 0;
            m_IncrementNumberByKeyword = new ConcurrentDictionary<string, int>();
            m_KeywordsRelations = new ConcurrentDictionary<string, string>();
        }

        private void KeyWordsRegroup() {
            if (m_KeywordsValues == null || m_KeywordsValues.Count <= 0)
                return;
            var sorted = m_KeywordsValues.Keys.OrderByDescending(_ => _.Length).ToList();
            var spilted = sorted.SplitItems(c_Regex_MaxLengthPer);
            m_KeywordsReg_Regroup = new List<string>();
            spilted.ForEach(keyword => m_KeywordsReg_Regroup.Add(string.Join("|", keyword).TrimEnd('|')));
        }

        private string ReplaceByReg(string content, string pattern) {
            return Regex.Replace(content, pattern, (matched) => {
                if (m_KeywordsValues.ContainsKey(matched.Value)) {
                    if (!m_IncrementNumberByKeyword.ContainsKey(matched.Value)) {
                        Interlocked.Increment(ref m_formatIndex);
                        m_IncrementNumberByKeyword.GetOrAdd(matched.Value, m_formatIndex);
                    }

                    var key = string.Format(c_Format, m_IncrementNumberByKeyword[matched.Value]);
                    if (!m_KeywordsRelations.ContainsKey(key)) {
                        m_KeywordsRelations.GetOrAdd(key, matched.Value);
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
                result.Add(source.Skip(totalLength - lastItemLength).Take(lastItemLength).ToList());
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
