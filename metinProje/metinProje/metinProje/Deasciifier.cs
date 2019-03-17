using System;
using System.Collections;

namespace metinProje
{
    public class Deasciifier
    {
        #region Private Values

        private readonly int _contextSize;
        private readonly bool _aggressive;
        private readonly IDeasciifierPatterns _patternData;

        #endregion

        #region Constructors

        public Deasciifier(int contextSize = 20, bool aggressive = false, IDeasciifierPatterns customPatterns = null)
        {
            _contextSize = contextSize;
            _aggressive = aggressive;
            _patternData = customPatterns ?? new DeasciifierPatterns();
        }

        #endregion

        #region Public Properties

        public int ContextSize
        {
            get { return _contextSize; }
        }

        public bool IsAggressive
        {
            get { return _aggressive; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Deasciifies the input
        /// </summary>
        /// <param name="asciiString">ascii string</param>
        /// <returns>deasciified string</returns>
        public string DeAsciify(string asciiString)
        {
            return IsNullOrWhiteSpace(asciiString)
                ? asciiString
                : DeAsciify(asciiString, 0, asciiString.Length);
        }

        /// <summary>
        /// Deasciifies the specified input region
        /// </summary>
        /// <param name="asciiString">ascii string</param>
        /// <param name="startIndex">region start index</param>
        /// <param name="length">region size</param>
        /// <returns></returns>
        public string DeAsciify(string asciiString, int startIndex, int length)
        {
            if (IsNullOrWhiteSpace(asciiString))
                return asciiString;
            asciiString += " ";
            char[] buffer = asciiString.ToCharArray(startIndex, length);
            for (int i = startIndex; i < length; i++)
            {
                char ch = buffer[i], x;
                if (NeedCorrection(buffer, ch, i) &&
                    _patternData.TurkishToggleAccentsTable.TryGetValue(ch, out x))
                {
                    // Adds or removes turkish accent at the cursor.
                    SetCharAt(buffer, i, x);
                }
            }

            return new string(buffer).TrimEnd();
        }

        #endregion

        #region Private/Protected Methods

        protected static void SetCharAt(char[] buffer, int index, char ch)
        {
            buffer[index] = ch;
        }

        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        /// <summary>
        /// Determine if char at cursor needs correction.
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="ch">char</param>
        /// <param name="point">index</param>
        /// <returns>whether if needs correction</returns>
        protected bool NeedCorrection(char[] buffer, char ch, int point)
        {
            char tr;
            if (!_patternData.TurkishAsciifyTable.TryGetValue(ch, out tr))
                tr = ch;
            else if (!_aggressive)
                return false; // aslı & asli problemi

            var m = false;
            IDictionary pattern;
            if (_patternData.TryGetPattern(tr, out pattern))
            {
                m = MatchPattern(buffer, pattern, point);
            }

            if (tr == 'I')
                return ch == tr ? !m : m;
            return ch == tr ? m : !m;
        }

        protected virtual bool MatchPattern(char[] buffer, IDictionary pattern, int point)
        {
            char[] s = GetContext(buffer, _contextSize, point);
            short rank = Convert.ToInt16(pattern.Count*2);
            var start = 0;
            while (start <= _contextSize)
            {
                int end = _contextSize + 1;
                while (end <= s.Length)
                {
                    var str = new string(s, start, end - start);
                    short r;
                    if (pattern.Contains(str) && Math.Abs(r = (short) pattern[str]) < Math.Abs(rank))
                    {
                        rank = r;
                    }
                    end++;
                }
                start++;
            }
            return rank > 0;
        }

        protected char[] GetContext(char[] buffer, int size, int point)
        {
            char[] s = new string(' ', 1 + 2*size).ToCharArray();
            SetCharAt(s, size, 'X');
            int i = size + 1;
            int index = point + 1;
            var space = false;

            while (!space && (i < s.Length) && (index < buffer.Length))
            {
                char cc = buffer[index];
                char x;
                if (_patternData.TurkishDowncaseAsciifyTable.TryGetValue(cc, out x) == false)
                    space = true;
                else
                    SetCharAt(s, i, x);
                i++;
                index++;
            }

            Array.Resize(ref s, i);
            index = point;
            i = size - 1;
            space = false;
            index--;

            while (i >= 0 && index >= 0)
            {
                char cc = buffer[index];
                char x;
                if (!_patternData.TurkishUpcaseAccentsTable.TryGetValue(cc, out x))
                {
                    if (!space)
                    {
                        i--;
                        space = true;
                    }
                }
                else
                {
                    SetCharAt(s, i, x);
                    i--;
                    space = false;
                }
                //i--;
                index--;
            }
            return s;
        }

        #endregion
    }
}