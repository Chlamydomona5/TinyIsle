using System.Collections.Generic;

namespace UI
{
    public interface IInfoDisplay
    {
        /// <summary>
        /// Short Pair of information, Got 4 in a row
        /// </summary>
        /// <returns></returns>
        public List<(string iconId, string text, int lineNumber)> GetIconValuePair();
        
        /// <summary>
        /// Long Pair of information, Got 1 in a row
        /// </summary>
        /// <returns></returns>
        public List<(string iconId, string text)> GetIconDescPair();

    }
}