﻿//******************************************************************************************************
//  StringParser.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/01/2017 - F. Russell Robertson
//       Generated original version of source code.
//
//******************************************************************************************************using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GSF.Parsing
{
    /// <summary>
    /// Like the Excel CSV Parser, only better.
    /// </summary>
    public class StringParser
    {
        private const char quoteDoubleChar = '\"';
        private const char quoteSingleChar = '\"';
        private const char spaceChar = ' ';
        private const char commaChar = ',';
        private const string crLf = "\r\n";
        private const int workingArraySize = 128;

        /// <summary>
        /// Since everything in the class is static or private, a public constructor
        /// is not necessary. Creating this private constructor prevents C# from automatically
        /// creating a public constructor.
        /// </summary>
        private StringParser()
        {
            // Are you happy now IntelliSense?
        }

        /// <summary>
        /// Returns the string that is between two delimiter strings beginning the first startDelimiter found.
        /// ALSO, returns the index of the payload (the index of the first char past the startDelimiter)
        /// </summary>
        /// <param name="inString">The input string</param>
        /// <param name="startToken">The beginning token or delimiter</param>
        /// <param name="endToken">The ending token or delimiter</param>
        /// <param name="startIndex">The index on which to begin searching inString</param>
        /// <param name="matchCase">set to FALSE for case insensitive test for delimiters</param>
        /// <param name="includeTokensInReturn">set to TRUE for the return string to include the opening and closing tokens.</param>
        /// <param name="payloadIndex"></param>
        /// <returns>A string, and the string starting index (payload Index)</returns>
        public static string GetBetweenDelimiters(string inString, out int payloadIndex, char startToken = ',', char endToken = ',',
                             int startIndex = 0, bool matchCase = true, bool includeTokensInReturn = false)
        {
            payloadIndex = -1;

            if (string.IsNullOrEmpty(inString))
                return (null);

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return (null);

            string testString = inString;

            if (!matchCase)
            {
                testString = inString.ToUpper();
                startToken = startToken.ToUpper();
                endToken = endToken.ToUpper();
            }

            int iStart = testString.IndexOf(startToken, startIndex);
            if (iStart > -1)
            {
                iStart++;
                int iEnd = testString.IndexOf(endToken, iStart);
                if (iEnd > -1)
                {

                    //Are there other open tokens in this range
                    int[] opentags = IndicesOfToken(testString.Substring(iStart, iEnd - iStart), startToken);
                    //If so, reach to the matching closing Token
                    if (opentags != null && opentags.Length > 0)
                    {

                        for (int i = 0; i < opentags.Length; i++)
                        {
                            iEnd++;
                            iEnd = testString.IndexOf(endToken, iEnd);
                            if (iEnd < 0) break;
                        }
                        if (iEnd < 0) return (null);

                    }

                    if (includeTokensInReturn)
                    {
                        payloadIndex = iStart - 1;
                        return inString.Substring(payloadIndex, iEnd - iStart + 2);
                    }

                    payloadIndex = iStart;
                    return inString.Substring(payloadIndex, iEnd - iStart);
                }
                else
                    // no end delimiter
                    return (null);
            }
            else
                // no start delimiter
                return (null);
        }

        /// <summary>
        /// Returns an array indices where the token char was found.  Null for no tokens found.
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <param name="token">The token string sought</param>
        /// <param name="startIndex">The index from which to begin searching inString</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns></returns>
        public static int[] IndicesOfToken(string inString, char token, int startIndex = 0, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return null;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return null;

            int[] indices = new int[workingArraySize];

            if (token == 0)
                return null;

            int count = 0;
            int k = IndexOfNextToken(inString, token, startIndex, 1, matchCase);
            while (k > -1)
            {
                indices[count++] = k;
                k = IndexOfNextToken(inString, token, k + 1, 1, matchCase);
                if (count >= indices.Length)
                    Array.Resize(ref indices, indices.Length + workingArraySize);
            }
            if (count == 0)
            {
                Array.Resize(ref indices, 1);
                return null;
            }

            Array.Resize(ref indices, count);
            return indices;
        }

        /// <summary>
        /// Returns an array of indices where the token string was found
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <param name="token">The token string sought</param>
        /// <param name="startIndex">The index from which to begin searching inString</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns>null for no tokens found</returns>
        public static int[] IndicesOfToken(string inString, string token, int startIndex = 0, bool matchCase = true)
        {

            if (string.IsNullOrEmpty(inString))
                return null;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return null;

            int[] indices = new int[workingArraySize];

            if (string.IsNullOrEmpty(token))
                return null;

            int count = 0;
            int k = IndexOfNextToken(inString, token, startIndex, 1, matchCase);

            while (k > -1)
            {
                indices[count++] = k;
                k = IndexOfNextToken(inString, token, k + token.Length, 1, matchCase);
                if (count >= indices.Length)
                    Array.Resize(ref indices, indices.Length + workingArraySize);

            }
            if (count > 0)
            {
                Array.Resize(ref indices, count);
                return indices;
            }
            return null;
        }

        /// <summary>
        /// Returns an array of the indices where the token chars were found.  Null for no tokens found.
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <param name="tokens">The char array of the tokens</param>
        /// <param name="startIndex">The index from which to begin searching inString</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns></returns>
        public static int[] IndicesOfTokens(string inString, char[] tokens, int startIndex = 0, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return null;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return null;

            int[] indices = new int[workingArraySize];

            if (tokens == null || tokens[0] == 0)
                return null;

            int count = 0;
            int k = IndexOfNextTokens(inString, tokens, startIndex, matchCase);

            while (k > -1)
            {
                indices[count++] = k;
                k = IndexOfNextTokens(inString, tokens, k + 1, matchCase);
                if (count >= indices.Length)
                    Array.Resize(ref indices, indices.Length + workingArraySize);
            }

            if (count == 0)
            {
                Array.Resize(ref indices, 1);
                return null;
            }

            Array.Resize(ref indices, count);
            return indices;
        }

        /// <summary>
        ///  Finds the index of the "n" occurrence of a character (a token) within a string
        /// </summary>
        /// <param name="inString">The string to process.</param>
        /// <param name="token">The token character sought</param>
        /// <param name="startIndex">The index from which to begin searching inString</param>
        /// <param name="occurrenceCount">The occurrence sought</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns>Returns the starting index of the nth occurrence of a character.
        ///  Returns -1 if nth occurrence does not exist.</returns>
        public static int IndexOfNextToken(string inString, char token, int startIndex = 0, int occurrenceCount = 1, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            if (token == 0)
                return -1;

            if (occurrenceCount < 1)
                return -1;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return -1;

            if (!matchCase)
            {
                inString = inString.ToUpper();
                token = token.ToUpper();
            }

            int count = 1;
            int indexPos = inString.IndexOf(token, startIndex);

            while (indexPos > -1 && count != occurrenceCount)
            {
                indexPos = inString.IndexOf(token, indexPos + 1);
                count++;
            }
            return indexPos;
        }

        /// <summary>
        ///  Finds the index of the "n" occurrence of one string (a token) within another
        /// </summary>
        /// <param name="inString">The string to process.</param>
        /// <param name="token">The string to find</param>
        /// <param name="startIndex">The index from which to begin searching inString</param>
        /// <param name="occurrenceCount">The occurrence of the token sought</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns>Returns the starting index of the nth occurrence of a string. 
        /// Returns -1 if nth occurrence does not exist.</returns>
        public static int IndexOfNextToken(string inString, string token, int startIndex = 0, int occurrenceCount = 1, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            if (string.IsNullOrEmpty(token))
                return -1;

            if (occurrenceCount < 1)
                return -1;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return -1;

            if (!matchCase)
            {
                inString = inString.ToUpper();
                token = token.ToUpper();
            }

            int count = 1;
            int indexPos = inString.IndexOf(token, startIndex);

            while (indexPos > -1 && count != occurrenceCount)
            {
                indexPos = inString.IndexOf(token, indexPos + 1);
                count++;
            }
            return indexPos;
        }


        /// <summary>
        ///  Finds the index of the "n" occurrence any one of the chars in the token array within a string
        /// </summary>
        /// <param name="inString">The string to process.</param>
        /// <param name="tokens">The token characters sought</param>
        /// <param name="startIndex">The index from which to begin searching inString</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns>Returns the starting index of the nth occurrence of a character.
        ///  Returns -1 if nth occurrence does not exist.</returns>
        public static int IndexOfNextTokens(string inString, char[] tokens, int startIndex = 0, bool matchCase = true)

        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            if (tokens == null)
                return -1;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return -1;

            if (inString.IndexOfAny(tokens, startIndex) < 0)
                return -1;

            int i = 0;

            if (!matchCase)
            {
                inString = inString.ToUpper();
                foreach (char c in tokens)
                {
                    tokens[i++] = c.ToUpper();
                }
            }

            int[] positions = new int[tokens.Length];
            i = 0;

            foreach (char c in tokens)
            {
                positions[i] = inString.IndexOf(c, startIndex);
                if (positions[i] < 0)
                    positions[i] = 32000;
                i++;
            }

            return positions.Min();

        }

        /// <summary>
        ///  Processing from RIGHT to LEFT, finds the index of the "n" occurrence of a character (a token) within a string
        /// </summary>
        /// <param name="inString">The string to process.</param>
        /// <param name="token">The token character sought</param>
        /// <param name="startIndex">Default of zero (0) begins testing end of inString, otherwise
        /// The index from with to begin processing inString from RIGHT to LEFT</param>
        /// <param name="occurrenceCount">The occurrence sought</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns>Returns the starting index of the nth occurrence of a character.
        /// Returns -1 if nth occurrence does not exist.</returns>
        public static int IndexOfPreviousToken(string inString, char token, int startIndex = 0, int occurrenceCount = 1, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            if (token == 0)
                return -1;

            if (occurrenceCount < 1)
                return -1;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return -1;

            if (startIndex == 0)
                startIndex = inString.Length - 1;


            if (!matchCase)
            {
                inString = inString.ToUpper();
                token = token.ToUpper();
            }

            int count = 1;
            int indexPos = inString.IndexOfPrevious(token, startIndex);

            while (indexPos > -1 && count != occurrenceCount)
            {
                indexPos = inString.IndexOfPrevious(token, indexPos - 1);
                count++;
            }
            return indexPos;
        }


        /// <summary>
        ///  Processing from RIGHT to LEFT, finds the index of the "n"occurrence of one string (a token) within a string
        /// </summary>
        /// <param name="inString">The string to process.</param>
        /// <param name="token">The token string sought</param>
        /// <param name="startIndex">Default of zero (0) begins testing end of inString, otherwise
        /// The index from with to begin processing inString from RIGHT to LEFT</param>
        /// <param name="occurrenceCount">The occurrence sought</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns>Returns the starting index of the nth occurrence of a string.
        /// Returns -1 if nth occurrence does not exist.</returns>
        public static int IndexOfPreviousToken(string inString, string token, int startIndex = 0, int occurrenceCount = 1, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            if (string.IsNullOrEmpty(token))
                return -1;

            if (occurrenceCount < 1)
                return -1;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return -1;

            if (startIndex == 0)
                startIndex = inString.Length - 1;


            if (!matchCase)
            {
                inString = inString.ToUpper();
                token = token.ToUpper();
            }

            int count = 1;
            int indexPos = inString.IndexOfPrevious(token, startIndex);

            while (indexPos > -1 && count != occurrenceCount)
            {
                indexPos = inString.IndexOfPrevious(token, indexPos - 1);
                count++;
            }
            return indexPos;
        }

        /// <summary>
        /// Looks to the RIGHT for the first open token and returns the matching close token
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <param name="openToken"></param>
        /// <param name="closeToken"></param>
        /// <param name="startIndex">The index from with to begin processing inString</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns></returns>
        public static int IndexOfMatchingCloseToken(string inString, string openToken, string closeToken, int startIndex = 0, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            if (string.IsNullOrEmpty(openToken))
                return -1;

            if (string.IsNullOrEmpty(closeToken))
                return -1;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return -1;

            if (!matchCase)
            {
                inString = inString.ToLower();
                openToken = openToken.ToLower();
                closeToken = closeToken.ToLower();
            }

            int openTokenIndex = inString.IndexOf(openToken, startIndex);
            if (openTokenIndex < 0)
                return -1;

            if (startIndex + openToken.Length > inString.Length)
                return -1;

            int closeTokenIndex = inString.IndexOf(closeToken, openTokenIndex + openToken.Length);
            if (closeTokenIndex < 0)
                return -1;

            int openCount = inString.Substring(openTokenIndex, closeTokenIndex - openTokenIndex).StringCount(openToken);

            if (openCount < 2)
                return closeTokenIndex;
            return IndexOfNextToken(inString, closeToken, closeTokenIndex, openCount);

        }

        /// <summary>
        /// Looks to the RIGHT for the first open token and returns the matching close token
        /// </summary>
        /// <param name="inString">The string to process</param>
        /// <param name="openToken"></param>
        /// <param name="closeToken"></param>
        /// <param name="startIndex">The index from with to begin processing inString</param>
        /// <param name="matchCase">Set to FALSE for case insensitive search</param>
        /// <returns></returns>
        public static int IndexOfMatchingCloseToken(string inString, char openToken, char closeToken, int startIndex = 0, bool matchCase = true)
        {
            if (string.IsNullOrEmpty(inString))
                return -1;

            if (openToken == 0)
                return -1;

            if (closeToken == 0)
                return -1;

            if (startIndex > inString.Length - 1 || startIndex < 0)
                return -1;

            if (!matchCase)
            {
                inString = inString.ToLower();
                openToken = openToken.ToLower();
                closeToken = closeToken.ToLower();
            }

            int openTokenIndex = inString.IndexOf(openToken, startIndex);
            if (openTokenIndex < 0)
                return -1;

            if (startIndex + 1 > inString.Length)
                return -1;

            int closeTokenIndex = inString.IndexOf(closeToken, openTokenIndex + 1);
            if (closeTokenIndex < 0)
                return -1;

            int openCount = inString.Substring(openTokenIndex, closeTokenIndex - openTokenIndex).CharCount(openToken);
            if (openCount < 2)
                return closeTokenIndex;

            return IndexOfNextToken(inString, closeToken, closeTokenIndex, openCount);
        }


        /// <summary>
        /// Parses a line based on a comma as the separator.  Commas wrapped in matched double quotes are not separators.  
        /// Matched double quotes are normally removed prior to field return.  Fields are NOT trimmed of white spaces prior to return.
        /// </summary>
        /// <param name="inString">The string to parse.</param>
        /// <param name="startIndex">The index in the line from which to start parsing.</param>
        /// <param name="removeResultQuotes">Set to TRUE to unwrap quotes in returned array vis-a-vis Excel.</param>
        /// <returns>An array of the parsed strings (the fields within the line)</returns>
        /// <remarks>The string.split method is about 4 times faster.</remarks>
        public static string[] ParseStandardCSV(string inString, int startIndex = 0, bool removeResultQuotes = true)
        {

            if (string.IsNullOrEmpty(inString))
                return null;

            if (startIndex > inString.Length - 1)
                return null;

            int nextQ = IndexOfNextToken(inString, quoteDoubleChar, startIndex);
            int nextD = IndexOfNextToken(inString, commaChar, startIndex);

            if (nextQ < 0 && nextD < 0)  //no quote, no delimiter
                return new string[] { inString };

            string[] p = new string[workingArraySize];
            int index = 0;

            while (true)
            {
                if (startIndex >= inString.Length)
                    break;

                if (nextQ > -1 && nextD > -1)  //have a quote and delimiter remaining
                {

                    if (nextQ < nextD)  //quote is prior to delimiter (typical case)
                    {

                        //find the close quote.
                        int closeQ = inString.IndexOf(quoteDoubleChar, nextQ + 1);
                        if (closeQ > -1)
                        {
                            //where is the next delimiter
                            nextD = IndexOfNextToken(inString, commaChar, closeQ + 1);
                        }
                        else
                        {
                            //ignore spurious open quote and move to the next delimiter.
                            nextD = IndexOfNextToken(inString, commaChar, nextD + 1);
                        }

                        if (nextD < 0)
                        {
                            if (removeResultQuotes)
                                p[index] = inString.Substring(startIndex).quoteUnwrap();
                            else
                                p[index] = inString.Substring(startIndex);

                            Array.Resize(ref p, index + 1);
                            return p;
                        }
                        else
                        {
                            if (removeResultQuotes)
                                p[index] = inString.Substring(startIndex, nextD - startIndex).quoteUnwrap();
                            else
                                p[index] = inString.Substring(startIndex, nextD - startIndex);
                            startIndex = nextD + 1;
                        }
                    }
                }
                else if (nextD < 0 && nextQ < 0)  //we're done
                {
                    if (removeResultQuotes)
                        p[index] = inString.Substring(startIndex).quoteUnwrap();
                    else
                        p[index] = inString.Substring(startIndex);

                    Array.Resize(ref p, index + 1);
                    return p;
                }
                else if (nextD < 0) //no remaining delimiter, but have quote
                {
                    if (removeResultQuotes)
                        p[index] = inString.Substring(startIndex).quoteUnwrap();
                    else
                        p[index] = inString.Substring(startIndex);

                    Array.Resize(ref p, index + 1);
                    return p;
                }
                else //nextQ < 0, no remaining quote, at least one remaining delimiter
                {
                    p[index] = inString.Substring(startIndex, nextD - startIndex).Trim();
                    startIndex = nextD + 1;
                }

                nextQ = IndexOfNextToken(inString, quoteDoubleChar, startIndex);
                nextD = IndexOfNextToken(inString, commaChar, startIndex);

                if (++index >= p.Length)
                    Array.Resize(ref p, p.Length + workingArraySize);
            }

            Array.Resize(ref p, index + 1);
            return p;
        }

        /// <summary>
        /// Parses a line based on a collection of quote and delimiter characters,
        /// </summary>
        /// <param name="inString">The string to parse</param>
        /// <param name="quoteChars">
        ///               An array of characters to be used as the framing within fields or the "quote" characters.  Quotes must matched. 
        ///               Set to null to disable (split line at delimiter regardless of quotes).</param>
        /// <param name="delimiters">An array of characters to be used as delimiter characters.  These characters have equal weight in breaking up the line.</param>
        /// <param name="startIndex">The index in the line from which to start parsing.</param>
        /// <param name="removeResultQuotes">Set to TRUE to unwrap quotes in returned array vis-a-vis Excel.</param>
        /// <returns>An array of the parsed strings</returns>
        /// <remarks>The string.split method is about 12 times faster.</remarks>
        public static string[] ParseLine(string inString, char[] quoteChars, char[] delimiters, int startIndex = 0, bool removeResultQuotes = true)
        {

            if (string.IsNullOrEmpty(inString))
                return null;

            if (startIndex > inString.Length - 1)
                return null;

            if (delimiters == null)
                return null;

            if (quoteChars == null)
                return inString.Split(delimiters);

            int nextQ = IndexOfNextTokens(inString, quoteChars, startIndex);
            int nextD = IndexOfNextTokens(inString, delimiters, startIndex);

            if (nextQ < 0 && nextD < 0)
                return new string[] { inString };

            string[] p = new string[workingArraySize];
            int index = 0;

            while (true)
            {
                if (startIndex >= inString.Length)
                    break;

                if (nextQ > -1 && nextD > -1)  //have a quote and delimiter remaining
                {

                    if (nextQ < nextD)  //quote is prior to delimiter (typical case)
                    {

                        //find the close quote.
                        int closeQ = inString.IndexOf(inString[nextQ], nextQ + 1);
                        if (closeQ > -1)
                        {
                            //where is the next delimiter
                            nextD = IndexOfNextTokens(inString, delimiters, closeQ + 1);
                        }
                        else
                        {
                            //ignore spurious open quote and move to the next delimiter.
                            nextD = IndexOfNextTokens(inString, delimiters, nextD + 1);
                        }

                        if (nextD < 0)
                        {
                            if (removeResultQuotes)
                                p[index] = inString.Substring(startIndex).quoteUnwrap(quoteChars);
                            else
                                p[index] = inString.Substring(startIndex);

                            Array.Resize(ref p, index + 1);
                            return p;
                        }
                        else
                        {
                            if (removeResultQuotes)
                                p[index] = inString.Substring(startIndex, nextD - startIndex).quoteUnwrap(quoteChars);
                            else
                                p[index] = inString.Substring(startIndex, nextD - startIndex);
                            startIndex = nextD + 1;
                        }
                    }
                }
                else if (nextD < 0 && nextQ < 0)  //we're done
                {
                    if (removeResultQuotes)
                        p[index] = inString.Substring(startIndex).quoteUnwrap();
                    else
                        p[index] = inString.Substring(startIndex);

                    Array.Resize(ref p, index + 1);
                    return p;
                }
                else if (nextD < 0) //no remaining delimiter, but have quote
                {
                    if (removeResultQuotes)
                        p[index] = inString.Substring(startIndex).quoteUnwrap(quoteChars);
                    else
                        p[index] = inString.Substring(startIndex);

                    Array.Resize(ref p, index + 1);
                    return p;
                }
                else //nextQ < 0, no remaining quote, at least one remaining delimiter
                {
                    p[index] = inString.Substring(startIndex, nextD - startIndex).Trim();
                    startIndex = nextD + 1;
                }

                nextQ = IndexOfNextTokens(inString, quoteChars, startIndex);
                nextD = IndexOfNextTokens(inString, delimiters, startIndex);

                if (++index >= p.Length)
                    Array.Resize(ref p, p.Length + workingArraySize);
            }

            Array.Resize(ref p, index + 1);
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsedStrings"></param>
        /// <param name="expectedTypeCodes"></param>
        /// <param name="values">the returned values from the try parse.</param>
        /// <returns>TRUE if all values parse successfully.</returns>
        public static bool ParseCheck(string[] parsedStrings, TypeCode[] expectedTypeCodes, out object[] values)
        {
            values = null;

            if (parsedStrings == null || expectedTypeCodes == null)
                return false;

            if (parsedStrings.Length != expectedTypeCodes.Length)
                return false;

            Array.Resize(ref values, parsedStrings.Length);

            bool outcome = false;
            bool checkParse = true;

            int i = 0;

            foreach (string s in parsedStrings)
            {
                switch (expectedTypeCodes[i])
                {
                    case TypeCode.Boolean:
                        bool result1;
                        outcome = Boolean.TryParse(parsedStrings[i], out result1);
                        if (outcome)
                            values[i] = result1;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Byte:
                        byte result2;
                        outcome = Byte.TryParse(parsedStrings[i], out result2);
                        if (outcome)
                            values[i] = result2;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Char:
                        Char result3;
                        outcome = Char.TryParse(parsedStrings[i], out result3);
                        if (outcome)
                            values[i] = result3;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.DateTime:
                        DateTime result4;
                        outcome = DateTime.TryParse(parsedStrings[i], out result4);
                        if (outcome)
                            values[i] = result4;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Decimal:
                        Decimal result5;
                        outcome = Decimal.TryParse(parsedStrings[i], out result5);
                        if (outcome)
                            values[i] = result5;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Double:
                        Double result6;
                        outcome = Double.TryParse(parsedStrings[i], out result6);
                        if (outcome)
                            values[i] = result6;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Int16:
                        Int16 result7;
                        outcome = Int16.TryParse(parsedStrings[i], out result7);
                        if (outcome)
                            values[i] = result7;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Int32:
                        Int32 result8;
                        outcome = Int32.TryParse(parsedStrings[i], out result8);
                        if (outcome)
                            values[i] = result8;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Int64:
                        Int64 result9;
                        outcome = Int64.TryParse(parsedStrings[i], out result9);
                        if (outcome)
                            values[i] = result9;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.SByte:
                        SByte result10;
                        outcome = SByte.TryParse(parsedStrings[i], out result10);
                        if (outcome)
                            values[i] = result10;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.Single:
                        Single result11;
                        outcome = Single.TryParse(parsedStrings[i], out result11);
                        if (outcome)
                            values[i] = result11;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.UInt16:
                        UInt16 result12;
                        outcome = UInt16.TryParse(parsedStrings[i], out result12);
                        if (outcome)
                            values[i] = result12;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.UInt32:
                        UInt32 result13;
                        outcome = UInt32.TryParse(parsedStrings[i], out result13);
                        if (outcome)
                            values[i] = result13;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    case TypeCode.UInt64:
                        UInt64 result14;
                        outcome = UInt64.TryParse(parsedStrings[i], out result14);
                        if (outcome)
                            values[i] = result14;
                        else
                        {
                            values[i] = null;
                            checkParse = false;
                        }
                        break;

                    default:
                        values[i] = parsedStrings[i];
                        break;

                }

                i++;
            }

            return checkParse;
        }
    }
}
