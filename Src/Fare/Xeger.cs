﻿/*
 * Copyright 2009 Wilfred Springer
 * http://github.com/moodmosaic/Fare/
 * Original Java code:
 * http://code.google.com/p/xeger/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;
using System.Text;

namespace Fare
{
    /// <summary>
    /// An object that will generate text from a regular expression. In a way, 
    /// it's the opposite of a regular expression matcher: an instance of this class
    /// will produce text that is guaranteed to match the regular expression passed in.
    /// </summary>
    public class Xeger
    {
        private const RegExpSyntaxOptions AllExceptAnyString = RegExpSyntaxOptions.All & ~RegExpSyntaxOptions.Anystring;

        private readonly Automaton automaton;
        private readonly Random random;
        private readonly string anyCharAlphabet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xeger"/> class.
        /// </summary>
        /// <param name="regex">The regex.</param>
        /// <param name="random">The random.</param>
        /// <param name="anyCharAlphabet">The list of characters used for computing the possible values for classes "." "\s", "\d", "\w" (and "\S", "\D", "\W"). It does not check explicitly defined chars in regexp.</param>
        public Xeger(string regex, Random random, string anyCharAlphabet = null)
        {
            if (string.IsNullOrEmpty(regex))
            {
                throw new ArgumentNullException("regex");
            }

            if (random == null)
            {
                throw new ArgumentNullException("random");
            }

            if (anyCharAlphabet != null)
            {
                this.anyCharAlphabet = anyCharAlphabet;
            }

            regex = RemoveStartEndMarkers(regex);
            var rx = new RegExp(regex, anyCharAlphabet, AllExceptAnyString);
            this.UsedAlphabet = rx.UsedAlphabet();
            this.automaton = rx.ToAutomaton();
            this.random = random;
        }

        public string UsedAlphabet
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xeger"/> class.<br/>
        /// Note that if multiple instances are created within short time using this overload,<br/>
        /// the instances might generate identical random strings.<br/>
        /// To avoid this, use the constructor overload that accepts an argument of type Random.
        /// </summary>
        /// <param name="regex">The regex.</param>
        public Xeger(string regex)
            : this(regex, new Random())
        {
        }

        /// <summary>
        /// Generates a random String that is guaranteed to match the regular expression passed to the constructor.
        /// </summary>
        /// <returns></returns>
        public string Generate()
        {
            var builder = new StringBuilder();
            this.Generate(builder, automaton.Initial);
            return builder.ToString();
        }

        /// <summary>
        /// Generates a random number within the given bounds.
        /// </summary>
        /// <param name="min">The minimum number (inclusive).</param>
        /// <param name="max">The maximum number (inclusive).</param>
        /// <param name="random">The object used as the randomizer.</param>
        /// <returns>A random number in the given range.</returns>
        private static int GetRandomInt(int min, int max, Random random)
        {
            int maxForRandom = max - min + 1;
            return random.Next(maxForRandom) + min;
        }

        private void Generate(StringBuilder builder, State state)
        {
            var transitions = state.GetSortedTransitions(true);
            if (transitions.Count == 0)
            {
                if (!state.Accept)
                {
                    throw new InvalidOperationException("state");
                }

                return;
            }

            if (state.Accept)
            {

                var optionsCount = transitions.Sum(x => x.Max - x.Min + 1);
                if (this.random.Next(optionsCount + 1) == 0) return;
            }

            var transition = transitions.RandomItemWithProbability(x => x.Max - x.Min + 1);
            
            this.AppendChoice(builder, transition);
            Generate(builder, transition.To);
        }

        private void AppendChoice(StringBuilder builder, Transition transition)
        {
            var c = (char)Xeger.GetRandomInt(transition.Min, transition.Max, random);
            builder.Append(c);
        }

        private string RemoveStartEndMarkers(string regExp)
        {
            if (regExp.StartsWith("^"))
            {
                regExp = regExp.Substring(1);
            }

            if (regExp.EndsWith("$"))
            {
                regExp = regExp.Substring(0, regExp.Length - 1);
            }

            return regExp;
        }
    }
}
