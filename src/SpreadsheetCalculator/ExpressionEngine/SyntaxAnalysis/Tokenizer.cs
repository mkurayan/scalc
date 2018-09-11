﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SpreadsheetCalculator.ExpressionEngine.SyntaxAnalysis
{   
    /// <summary>
    /// Split string to tokens.
    /// </summary>
    internal abstract class Tokenizer : ITokenizer
    {
        protected abstract Dictionary<char, TokenType> SymbolsMap { get; }

        private Stack<char> _symbolsStack;

        public Token[] Tokenize(string str)
        {
            var tokens = new List<Token>();

            _symbolsStack = new Stack<char>(str.ToCharArray().Reverse());

            while (_symbolsStack.TryPeek(out var ch))
            {
                if (char.IsWhiteSpace(ch))
                {
                    _symbolsStack.Pop();
                    continue;
                }

                tokens.Add(ReadToken());
            }

            return tokens.ToArray();
        }

        private Token ReadToken()
        {
            var nextChar = _symbolsStack.Peek();

            if (SymbolsMap.ContainsKey(nextChar))
            {
                return CharToToken(_symbolsStack.Pop());
            }

            // Token is number
            if (char.IsDigit(nextChar) || nextChar == '.')
            {
                return ReadNumberToken();
            }

            // Token is cell reference
            if (char.IsLetter(nextChar))
            {
                return ReadCellReferenceToken();
            }

            throw new SyntaxException();
        }

        private Token CharToToken(char ch) => new Token(SymbolsMap[ch], ch);

        private char[] ReadDigits() => ReadSequence(char.IsDigit).ToArray();

        private char[] ReadLetters() => ReadSequence(char.IsLetter).ToArray();

        private IEnumerable<char> ReadSequence(Func<char, bool> check)
        {
            while (_symbolsStack.TryPeek(out var ch) && check(ch))
            {
                yield return _symbolsStack.Pop();
            }
        }

        private Token ReadNumberToken()
        {
            var number = ReadDigits();

            if (_symbolsStack.TryPeek(out var nextChar) && nextChar == '.')
            {
                //Skip '.' symbol
                _symbolsStack.Pop();

                var decimalPart = ReadDigits();

                if (decimalPart.Length > 0)
                {
                    var integerPartLength = number.Length;

                    Array.Resize(ref number, integerPartLength + decimalPart.Length + 1);
                    number[integerPartLength] = '.';
                    decimalPart.CopyTo(number, integerPartLength + 1);
                }

                if (number.Length == 0)
                {
                    throw new SyntaxException();
                }
            }

            return new Token(TokenType.Number, new string(number));
        }

        private Token ReadCellReferenceToken()
        {
            var letterPart = ReadLetters();

            var digitPart = ReadDigits();

            if (letterPart.Length == 0 || digitPart.Length == 0)
            {
                throw new SyntaxException();
            }
            
            // Result cell reference.
            var cellReference = new char[letterPart.Length + digitPart.Length];
            
            // Copy letters.
            Array.Copy(letterPart, cellReference, letterPart.Length);

            // Copy digits.
            Array.Copy(digitPart, 0, cellReference, letterPart.Length, digitPart.Length);
           
            return new Token(TokenType.CellReference, string.Concat(cellReference));
        }
    }
}
