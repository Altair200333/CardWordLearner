using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordLearner
{
    class TranslatedWord
    {
        public string word;
        public List<string> translations;

        public TranslatedWord()
        {
            translations = new List<string>();
        }
    }
}
