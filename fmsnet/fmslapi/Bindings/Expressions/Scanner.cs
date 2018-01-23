
using System.Text.RegularExpressions;

namespace fmslapi.Bindings.Expressions
{
    public class Scanner
    {
        private static readonly Regex _parser = new Regex(@"
                                                              (-?\d+\.\d*) 
                                                            | ([$@\#%]*?\w+[\^]?  ( (\[\d+\])+ | (\.\w+)+ )? ) 
                                                            | ((==|=|~=|!=|>=|<=)|[\+\-*/\(\)!\?:\.,\|&\<\>]) 
                                                            | ((?<p>['`]).*?\k<p>)", 
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private int _index;
        private readonly MatchCollection _mc;

        public Scanner(string Src)
        {
            _mc = _parser.Matches(Src);
        }

        public string Get()
        {
            var r = Token;

            if (r == null) 
                return null;
            
            _index++;
            return r;
        }

        public void Next()
        {
            _index++;
        }

        public string Token => _index >= _mc.Count ? null : _mc[_index].Value;
    }
}
