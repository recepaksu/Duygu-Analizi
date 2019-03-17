using System.Collections;
using System.Collections.Generic;

namespace metinProje
{
    public interface IDeasciifierPatterns
    {
        Dictionary<char, char> TurkishAsciifyTable { get; }
        Dictionary<char, char> TurkishDowncaseAsciifyTable { get; }
        Dictionary<char, char> TurkishUpcaseAccentsTable { get; }
        Dictionary<char, char> TurkishToggleAccentsTable { get; }
        bool TryGetPattern(char ch, out IDictionary patternData);
    }
}