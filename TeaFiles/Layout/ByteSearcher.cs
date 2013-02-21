// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
namespace TeaTime
{
    class ByteSearcher
    {
        /// <summary>
        /// Finds the position of <paramref name="searchPattern"/> inside <paramref name="searchSpace"/>.
        /// </summary>
        /// <param name="searchSpace">The search space.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchSpaceLength">Length of the search space.</param>
        /// <param name="patternLength">Length of the pattern.</param>
        /// <returns>THe byte position of the pattern.</returns>
        /// <exception cref="InternalErrorException">The pattern is not included in the search space.</exception>
        /// <remarks></remarks>
        public static unsafe int GetPosition(byte* searchSpace, byte* searchPattern, int searchSpaceLength, int patternLength)
        {
            int n = searchSpaceLength - patternLength + 1;
            for (int i = 0; i < n; i++)
            {
                if (StartsWith(searchSpace++, searchPattern, patternLength)) return i;
            }
            throw new InternalErrorException("Pattern not found: The search pattern was not found inside search space.");
        }

        /// <summary>
        /// Tests whether <paramref name="searchSpace"/> starts with the byte sequence <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchSpace">The search space.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="patternLength">Length of the pattern.</param>
        /// <returns><c>true</c> if the pattern matches the begin of the search space</returns>
        /// <remarks>The length of the search space is not checked. The caller must assure that the search space is at least as long as the pattern length.</remarks>
        internal static unsafe bool StartsWith(byte* searchSpace, byte* searchPattern, int patternLength)
        {
            while (patternLength-- > 0)
            {
                if (*searchSpace++ != *searchPattern++) return false;
            }
            return true;
        }
    }
}
