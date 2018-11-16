/*
 * This file incorporates work covered by the following copyright and  
 * permission notice:  
 *
 * Diff Match and Patch
 * Copyright 2018 The diff-match-patch Authors.
 * https://github.com/google/diff-match-patch
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Numerics;

namespace NzbDrone.Common.Extensions
{

    public static class FuzzyContainsExtension {

        public static int FuzzyFind(this string text, string pattern, double matchProb)
        {
            return match(text, pattern, matchProb).Item1;
        }

        // return the accuracy of the best match of pattern within text
        public static double FuzzyContains(this string text, string pattern)
        {
            return match(text, pattern, 0.25).Item2;
        }
        
        /**
         * Locate the best instance of 'pattern' in 'text'.
         * Returns (-1, 1) if no match found.
         * @param text The text to search.
         * @param pattern The pattern to search for.
         * @return Best match index or -1.
         */
        private static Tuple<int, double> match(string text, string pattern, double matchThreshold = 0.5) {
            // Check for null inputs not needed since null can't be passed in C#.
            if (text.Length == 0 || pattern.Length == 0) {
                // Nothing to match.
                return new Tuple<int, double> (-1, 0);
            }

            if (pattern.Length <= text.Length)
            {
                var loc = text.IndexOf(pattern, StringComparison.Ordinal);
                if (loc != -1)
                {
                    // Perfect match!
                    return new Tuple<int, double> (loc, 1);
                }
            }

            // Do a fuzzy compare.
            return match_bitap(text, pattern, matchThreshold);
        }

        /**
         * Locate the best instance of 'pattern' in 'text' near 'loc' using the
         * Bitap algorithm.  Returns -1 if no match found.
         * @param text The text to search.
         * @param pattern The pattern to search for.
         * @return Best match index or -1.
         */
        private static Tuple<int, double> match_bitap(string text, string pattern, double matchThreshold) {

            // Initialise the alphabet.
            Dictionary<char, BigInteger> s = alphabet(pattern);
            // don't keep creating new BigInteger(1)
            var big1 = new BigInteger(1);

            // Lowest score belowe which we give up.
            var score_threshold = matchThreshold;
           
            // Initialise the bit arrays.
            var matchmask = big1 << (pattern.Length - 1);
            int best_loc = -1;

            // Empty initialization added to appease C# compiler.
            var last_rd = new BigInteger[0];
            for (int d = 0; d < pattern.Length; d++) {
                // Scan for the best match; each iteration allows for one more error.
                int start = 1;
                int finish = text.Length + pattern.Length;

                var rd = new BigInteger[finish + 2];
                rd[finish + 1] = (big1 << d) - big1;
                for (int j = finish; j >= start; j--) {
                    BigInteger charMatch;
                    if (text.Length <= j - 1 || !s.ContainsKey(text[j - 1])) {
                        // Out of range.
                        charMatch = 0;
                    } else {
                        charMatch = s[text[j - 1]];
                    }
                    if (d == 0) {
                        // First pass: exact match.
                        rd[j] = ((rd[j + 1] << 1) | big1) & charMatch;
                    } else {
                        // Subsequent passes: fuzzy match.
                        rd[j] = ((rd[j + 1] << 1) | big1) & charMatch
                            | (((last_rd[j + 1] | last_rd[j]) << 1) | big1) | last_rd[j + 1];
                    }
                    if ((rd[j] & matchmask) != 0) {
                        var score = bitapScore(d, pattern);
                        // This match will almost certainly be better than any existing
                        // match.  But check anyway.
                        if (score >= score_threshold) {
                            // Told you so.
                            score_threshold = score;
                            best_loc = j - 1;
                        }
                    }
                }
                if (bitapScore(d + 1, pattern) < score_threshold) {
                    // No hope for a (better) match at greater error levels.
                    break;
                }
                last_rd = rd;
            }
            return new Tuple<int, double> (best_loc, score_threshold);
        }

        /**
         * Compute and return the score for a match with e errors and x location.
         * @param e Number of errors in match.
         * @param pattern Pattern being sought.
         * @return Overall score for match (1.0 = good, 0.0 = bad).
         */
        private static double bitapScore(int e, string pattern) {
            return 1.0 - (double)e / pattern.Length;
        }

        /**
         * Initialise the alphabet for the Bitap algorithm.
         * @param pattern The text to encode.
         * @return Hash of character locations.
         */
        private static Dictionary<char, BigInteger> alphabet(string pattern) {
            var s = new Dictionary<char, BigInteger>();
            char[] char_pattern = pattern.ToCharArray();
            foreach (char c in char_pattern) {
                if (!s.ContainsKey(c)) {
                    s.Add(c, 0);
                }
            }
            int i = 0;
            foreach (char c in char_pattern) {
                s[c] = s[c] | (new BigInteger(1) << (pattern.Length - i - 1));
                i++;
            }
            return s;
        }
    }
}
