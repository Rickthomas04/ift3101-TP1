using LexicalAnalysis;
using LexicalAnalysis.Rules;
using LexicalAnalysis.Rules.OperatorNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysisTests
{
    public class RegularExpressionParserTests
    {
        private RegularExpressionParser _regexParser;

        [SetUp]
        public void Setup()
        {
            _regexParser = new();
        }

        [Test]
        public void IdRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Id.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode concat = (ConcatenationOperatorNode)root;
                Assert.That(concat.Pattern, Is.EqualTo("[A-Za-z_][A-Za-z_0-9]*"));

                Assert.That(concat.LeftFactor, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode firstCharSet = (CharacterSetOperatorNode)concat.LeftFactor;
                Assert.That(firstCharSet.Pattern, Is.EqualTo("[A-Za-z_]"));

                List<string> expectedFirstPatterns = [];
                expectedFirstPatterns.AddRange(Alphabet.UpperLettersRange.Select(item => item.ToString()));
                expectedFirstPatterns.AddRange(Alphabet.LowerLettersRange.Select(item => item.ToString()));
                expectedFirstPatterns.Add("_");
                AssertCharSetPatternsAreEqual(firstCharSet, expectedFirstPatterns);

                Assert.That(concat.RightFactor, Is.TypeOf(typeof(StarOperatorNode)));
                StarOperatorNode followingStar = (StarOperatorNode)concat.RightFactor;
                Assert.That(followingStar.Pattern, Is.EqualTo("[A-Za-z_0-9]*"));

                Assert.That(followingStar.Operand, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode followingCharSet = (CharacterSetOperatorNode)followingStar.Operand;
                Assert.That(followingCharSet.Pattern, Is.EqualTo("[A-Za-z_0-9]"));

                List<string> expectedFollowingPatterns = [];
                expectedFollowingPatterns.AddRange(Alphabet.UpperLettersRange.Select(item => item.ToString()));
                expectedFollowingPatterns.AddRange(Alphabet.LowerLettersRange.Select(item => item.ToString()));
                expectedFollowingPatterns.Add("_");
                expectedFollowingPatterns.AddRange(Alphabet.DigitsRange.Select(item => item.ToString()));
                AssertCharSetPatternsAreEqual(followingCharSet, expectedFollowingPatterns);
            });
        }

        [Test]
        public void NumberRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Number.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode rootConcat = (ConcatenationOperatorNode)root;
                Assert.That(rootConcat.Pattern, Is.EqualTo("[0-9]+(\\.[0-9]+)?"));

                Assert.That(rootConcat.LeftFactor, Is.TypeOf(typeof(PlusOperatorNode)));
                PlusOperatorNode firstPlus = (PlusOperatorNode)rootConcat.LeftFactor;
                Assert.That(firstPlus.Pattern, Is.EqualTo("[0-9]+"));

                Assert.That(firstPlus.Operand, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode firstCharSet = (CharacterSetOperatorNode)firstPlus.Operand;
                Assert.That(firstCharSet.Pattern, Is.EqualTo("[0-9]"));

                List<string> expectedFirstPatterns = Alphabet.DigitsRange.Select(item => item.ToString()).ToList();
                AssertCharSetPatternsAreEqual(firstCharSet, expectedFirstPatterns);

                Assert.That(rootConcat.RightFactor, Is.TypeOf(typeof(OptionalOperatorNode)));
                OptionalOperatorNode followingOptional = (OptionalOperatorNode)rootConcat.RightFactor;
                Assert.That(followingOptional.Pattern, Is.EqualTo("(\\.[0-9]+)?"));

                Assert.That(followingOptional.Operand, Is.TypeOf(typeof(GroupOperatorNode)));
                GroupOperatorNode followingGroup = (GroupOperatorNode)followingOptional.Operand;
                Assert.That(followingGroup.Pattern, Is.EqualTo("(\\.[0-9]+)"));

                Assert.That(followingGroup.Group, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode followingConcat = (ConcatenationOperatorNode)followingGroup.Group;
                Assert.That(followingConcat.Pattern, Is.EqualTo("\\.[0-9]+"));

                Assert.That(followingConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode followingDot = (SymbolOperatorNode)followingConcat.LeftFactor;
                Assert.That(followingDot.Pattern, Is.EqualTo("\\."));

                Assert.That(followingConcat.RightFactor, Is.TypeOf(typeof(PlusOperatorNode)));
                PlusOperatorNode followingPlus = (PlusOperatorNode)followingConcat.RightFactor;
                Assert.That(followingPlus.Pattern, Is.EqualTo("[0-9]+"));

                Assert.That(followingPlus.Operand, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode followingCharSet = (CharacterSetOperatorNode)followingPlus.Operand;
                Assert.That(followingCharSet.Pattern, Is.EqualTo("[0-9]"));

                List<string> expectedFollowingPatterns = Alphabet.DigitsRange.Select(item => item.ToString()).ToList();
                AssertCharSetPatternsAreEqual(followingCharSet, expectedFollowingPatterns);
            });
        }

        [Test]
        public void StringRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.String.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode rootConcat = (ConcatenationOperatorNode)root;
                Assert.That(rootConcat.Pattern, Is.EqualTo(@"""[A-Za-z0-9\. ]*"""));

                Assert.That(rootConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstSymbol = (SymbolOperatorNode)rootConcat.LeftFactor;
                Assert.That(firstSymbol.Pattern, Is.EqualTo(@""""));

                Assert.That(rootConcat.RightFactor, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode rightConcat = (ConcatenationOperatorNode)rootConcat.RightFactor;
                Assert.That(rightConcat.Pattern, Is.EqualTo(@"[A-Za-z0-9\. ]*"""));

                Assert.That(rightConcat.LeftFactor, Is.TypeOf(typeof(StarOperatorNode)));
                StarOperatorNode star = (StarOperatorNode)rightConcat.LeftFactor;
                Assert.That(star.Pattern, Is.EqualTo("[A-Za-z0-9\\. ]*"));

                Assert.That(star.Operand, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode charSet = (CharacterSetOperatorNode)star.Operand;
                Assert.That(charSet.Pattern, Is.EqualTo("[A-Za-z0-9\\. ]"));

                List<string> expectedPatterns = [];
                expectedPatterns.AddRange(Alphabet.UpperLettersRange.Select(item => item.ToString()));
                expectedPatterns.AddRange(Alphabet.LowerLettersRange.Select(item => item.ToString()));
                expectedPatterns.AddRange(Alphabet.DigitsRange.Select(item => item.ToString()));
                expectedPatterns.Add("\\.");
                expectedPatterns.Add(" ");
                AssertCharSetPatternsAreEqual(charSet, expectedPatterns);

                Assert.That(rightConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode lastSymbol = (SymbolOperatorNode)rightConcat.RightFactor;
                Assert.That(lastSymbol.Pattern, Is.EqualTo(@""""));
            });
        }

        [Test]
        public void CharRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Char.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode rootConcat = (ConcatenationOperatorNode)root;
                Assert.That(rootConcat.Pattern, Is.EqualTo(@"'[A-Za-z0-9\. ]'"));

                Assert.That(rootConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstSymbol = (SymbolOperatorNode)rootConcat.LeftFactor;
                Assert.That(firstSymbol.Pattern, Is.EqualTo("'"));

                Assert.That(rootConcat.RightFactor, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode rightConcat = (ConcatenationOperatorNode)rootConcat.RightFactor;
                Assert.That(rightConcat.Pattern, Is.EqualTo(@"[A-Za-z0-9\. ]'"));

                Assert.That(rightConcat.LeftFactor, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode charSet = (CharacterSetOperatorNode)rightConcat.LeftFactor;
                Assert.That(charSet.Pattern, Is.EqualTo("[A-Za-z0-9\\. ]"));

                List<string> expectedPatterns = [];
                expectedPatterns.AddRange(Alphabet.UpperLettersRange.Select(item => item.ToString()));
                expectedPatterns.AddRange(Alphabet.LowerLettersRange.Select(item => item.ToString()));
                expectedPatterns.AddRange(Alphabet.DigitsRange.Select(item => item.ToString()));
                expectedPatterns.Add("\\.");
                expectedPatterns.Add(" ");
                AssertCharSetPatternsAreEqual(charSet, expectedPatterns);

                Assert.That(rightConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode lastSymbol = (SymbolOperatorNode)rightConcat.RightFactor;
                Assert.That(lastSymbol.Pattern, Is.EqualTo(@"'"));
            });
        }

        [Test]
        public void ArithmeticRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Arithmetic.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode firstUnion = (UnionOperatorNode)root;
                Assert.That(firstUnion.Pattern, Is.EqualTo(@"\+|\-|\*|/|%"));

                Assert.That(firstUnion.LeftTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstSymbol = (SymbolOperatorNode)firstUnion.LeftTerm;
                Assert.That(firstSymbol.Pattern, Is.EqualTo(@"\+"));

                Assert.That(firstUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode secondUnion = (UnionOperatorNode)firstUnion.RightTerm;
                Assert.That(secondUnion.Pattern, Is.EqualTo(@"\-|\*|/|%"));

                Assert.That(secondUnion.LeftTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode secondSymbol = (SymbolOperatorNode)secondUnion.LeftTerm;
                Assert.That(secondSymbol.Pattern, Is.EqualTo(@"\-"));

                Assert.That(secondUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode thirdUnion = (UnionOperatorNode)secondUnion.RightTerm;
                Assert.That(thirdUnion.Pattern, Is.EqualTo(@"\*|/|%"));

                Assert.That(thirdUnion.LeftTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode thirdSymbol = (SymbolOperatorNode)thirdUnion.LeftTerm;
                Assert.That(thirdSymbol.Pattern, Is.EqualTo(@"\*"));

                Assert.That(thirdUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode fourthUnion = (UnionOperatorNode)thirdUnion.RightTerm;
                Assert.That(fourthUnion.Pattern, Is.EqualTo(@"/|%"));

                Assert.That(fourthUnion.LeftTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fourthSymbol = (SymbolOperatorNode)fourthUnion.LeftTerm;
                Assert.That(fourthSymbol.Pattern, Is.EqualTo(@"/"));

                Assert.That(fourthUnion.RightTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fifthSymbol = (SymbolOperatorNode)fourthUnion.RightTerm;
                Assert.That(fifthSymbol.Pattern, Is.EqualTo(@"%"));
            });
        }

        [Test]
        public void RelationalRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Relational.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode firstUnion = (UnionOperatorNode)root;
                Assert.That(firstUnion.Pattern, Is.EqualTo("==|!=|<|>|<=|>="));

                Assert.That(firstUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode firstConcat = (ConcatenationOperatorNode)firstUnion.LeftTerm;
                Assert.That(firstConcat.Pattern, Is.EqualTo("=="));

                Assert.That(firstConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstLeftSymbol = (SymbolOperatorNode)firstConcat.LeftFactor;
                Assert.That(firstLeftSymbol.Pattern, Is.EqualTo("="));

                Assert.That(firstConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstRightSymbol = (SymbolOperatorNode)firstConcat.RightFactor;
                Assert.That(firstLeftSymbol.Pattern, Is.EqualTo("="));

                Assert.That(firstUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode secondUnion = (UnionOperatorNode)firstUnion.RightTerm;
                Assert.That(secondUnion.Pattern, Is.EqualTo("!=|<|>|<=|>="));

                Assert.That(secondUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode secondConcat = (ConcatenationOperatorNode)secondUnion.LeftTerm;
                Assert.That(secondConcat.Pattern, Is.EqualTo("!="));

                Assert.That(secondConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode secondLeftSymbol = (SymbolOperatorNode)secondConcat.LeftFactor;
                Assert.That(secondLeftSymbol.Pattern, Is.EqualTo("!"));

                Assert.That(secondConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode secondRightSymbol = (SymbolOperatorNode)secondConcat.RightFactor;
                Assert.That(secondRightSymbol.Pattern, Is.EqualTo("="));

                Assert.That(secondUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode thirdUnion = (UnionOperatorNode)secondUnion.RightTerm;
                Assert.That(thirdUnion.Pattern, Is.EqualTo("<|>|<=|>="));

                Assert.That(thirdUnion.LeftTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode thirdSymbol = (SymbolOperatorNode)thirdUnion.LeftTerm;
                Assert.That(thirdSymbol.Pattern, Is.EqualTo("<"));

                Assert.That(thirdUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode fourthUnion = (UnionOperatorNode)thirdUnion.RightTerm;
                Assert.That(fourthUnion.Pattern, Is.EqualTo(">|<=|>="));

                Assert.That(fourthUnion.LeftTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fourthSymbol = (SymbolOperatorNode)fourthUnion.LeftTerm;
                Assert.That(fourthSymbol.Pattern, Is.EqualTo(">"));

                Assert.That(fourthUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode fifthUnion = (UnionOperatorNode)fourthUnion.RightTerm;
                Assert.That(fifthUnion.Pattern, Is.EqualTo("<=|>="));

                Assert.That(fifthUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode fifthConcat = (ConcatenationOperatorNode)fifthUnion.LeftTerm;
                Assert.That(fifthConcat.Pattern, Is.EqualTo("<="));

                Assert.That(fifthConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fifthLeftSymbol = (SymbolOperatorNode)fifthConcat.LeftFactor;
                Assert.That(fifthLeftSymbol.Pattern, Is.EqualTo("<"));

                Assert.That(fifthConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fifthRightSymbol = (SymbolOperatorNode)fifthConcat.RightFactor;
                Assert.That(fifthRightSymbol.Pattern, Is.EqualTo("="));

                Assert.That(fifthUnion.RightTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode sixthConcat = (ConcatenationOperatorNode)fifthUnion.RightTerm;
                Assert.That(sixthConcat.Pattern, Is.EqualTo(">="));

                Assert.That(sixthConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode sixthLeftSymbol = (SymbolOperatorNode)sixthConcat.LeftFactor;
                Assert.That(sixthLeftSymbol.Pattern, Is.EqualTo(">"));

                Assert.That(sixthConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode sixthRightSymbol = (SymbolOperatorNode)sixthConcat.RightFactor;
                Assert.That(sixthRightSymbol.Pattern, Is.EqualTo("="));
            });
        }

        [Test]
        public void LogicalRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Logical.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode firstUnion = (UnionOperatorNode)root;
                Assert.That(firstUnion.Pattern, Is.EqualTo(@"&&|\|\||!"));

                Assert.That(firstUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode firstConcat = (ConcatenationOperatorNode)firstUnion.LeftTerm;
                Assert.That(firstConcat.Pattern, Is.EqualTo("&&"));

                Assert.That(firstConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstLeftSymbol = (SymbolOperatorNode)firstConcat.LeftFactor;
                Assert.That(firstLeftSymbol.Pattern, Is.EqualTo("&"));

                Assert.That(firstConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstRightSymbol = (SymbolOperatorNode)firstConcat.RightFactor;
                Assert.That(firstRightSymbol.Pattern, Is.EqualTo("&"));

                Assert.That(firstUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode secondUnion = (UnionOperatorNode)firstUnion.RightTerm;
                Assert.That(secondUnion.Pattern, Is.EqualTo(@"\|\||!"));

                Assert.That(secondUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode secondConcat = (ConcatenationOperatorNode)secondUnion.LeftTerm;
                Assert.That(secondConcat.Pattern, Is.EqualTo(@"\|\|"));

                Assert.That(secondConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode secondLeftSymbol = (SymbolOperatorNode)secondConcat.LeftFactor;
                Assert.That(secondLeftSymbol.Pattern, Is.EqualTo(@"\|"));

                Assert.That(secondConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode secondRightSymbol = (SymbolOperatorNode)secondConcat.RightFactor;
                Assert.That(secondRightSymbol.Pattern, Is.EqualTo(@"\|"));

                Assert.That(secondUnion.RightTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode thirdSymbol = (SymbolOperatorNode)secondUnion.RightTerm;
                Assert.That(thirdSymbol.Pattern, Is.EqualTo("!"));
            });
        }

        [Test]
        public void AssignRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Assign.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode firstUnion = (UnionOperatorNode)root;
                Assert.That(firstUnion.Pattern, Is.EqualTo(@"=|\+=|\-=|\*=|/=|%=|\+\+|\-\-"));

                Assert.That(firstUnion.LeftTerm, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode firstSymbol = (SymbolOperatorNode)firstUnion.LeftTerm;
                Assert.That(firstSymbol.Pattern, Is.EqualTo("="));

                Assert.That(firstUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode secondUnion = (UnionOperatorNode)firstUnion.RightTerm;
                Assert.That(secondUnion.Pattern, Is.EqualTo(@"\+=|\-=|\*=|/=|%=|\+\+|\-\-"));

                Assert.That(secondUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode secondConcat = (ConcatenationOperatorNode)secondUnion.LeftTerm;
                Assert.That(secondConcat.Pattern, Is.EqualTo(@"\+="));

                Assert.That(secondConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode secondLeftSymbol = (SymbolOperatorNode)secondConcat.LeftFactor;
                Assert.That(secondLeftSymbol.Pattern, Is.EqualTo(@"\+"));

                Assert.That(secondConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode secondRightSymbol = (SymbolOperatorNode)secondConcat.RightFactor;
                Assert.That(secondRightSymbol.Pattern, Is.EqualTo("="));

                Assert.That(secondUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode thirdUnion = (UnionOperatorNode)secondUnion.RightTerm;
                Assert.That(thirdUnion.Pattern, Is.EqualTo(@"\-=|\*=|/=|%=|\+\+|\-\-"));

                Assert.That(thirdUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode thirdConcat = (ConcatenationOperatorNode)thirdUnion.LeftTerm;
                Assert.That(thirdConcat.Pattern, Is.EqualTo(@"\-="));

                Assert.That(thirdConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode thirdLeftSymbol = (SymbolOperatorNode)thirdConcat.LeftFactor;
                Assert.That(thirdLeftSymbol.Pattern, Is.EqualTo(@"\-"));

                Assert.That(thirdConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode thirdRightSymbol = (SymbolOperatorNode)thirdConcat.RightFactor;
                Assert.That(thirdRightSymbol.Pattern, Is.EqualTo("="));

                Assert.That(thirdUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode fourthUnion = (UnionOperatorNode)thirdUnion.RightTerm;
                Assert.That(fourthUnion.Pattern, Is.EqualTo(@"\*=|/=|%=|\+\+|\-\-"));

                Assert.That(fourthUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode fourthConcat = (ConcatenationOperatorNode)fourthUnion.LeftTerm;
                Assert.That(fourthConcat.Pattern, Is.EqualTo(@"\*="));

                Assert.That(fourthConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fourthLeftSymbol = (SymbolOperatorNode)fourthConcat.LeftFactor;
                Assert.That(fourthLeftSymbol.Pattern, Is.EqualTo(@"\*"));

                Assert.That(fourthConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fourthRightSymbol = (SymbolOperatorNode)fourthConcat.RightFactor;
                Assert.That(fourthRightSymbol.Pattern, Is.EqualTo("="));

                Assert.That(fourthUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode fifthUnion = (UnionOperatorNode)fourthUnion.RightTerm;
                Assert.That(fifthUnion.Pattern, Is.EqualTo(@"/=|%=|\+\+|\-\-"));

                Assert.That(fifthUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode fifthConcat = (ConcatenationOperatorNode)fifthUnion.LeftTerm;
                Assert.That(fifthConcat.Pattern, Is.EqualTo("/="));

                Assert.That(fifthConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fifthLeftSymbol = (SymbolOperatorNode)fifthConcat.LeftFactor;
                Assert.That(fifthLeftSymbol.Pattern, Is.EqualTo("/"));

                Assert.That(fifthConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode fifthRightSymbol = (SymbolOperatorNode)fifthConcat.RightFactor;
                Assert.That(fifthRightSymbol.Pattern, Is.EqualTo("="));

                Assert.That(fifthUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode sixthUnion = (UnionOperatorNode)fifthUnion.RightTerm;
                Assert.That(sixthUnion.Pattern, Is.EqualTo(@"%=|\+\+|\-\-"));

                Assert.That(sixthUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode sixthConcat = (ConcatenationOperatorNode)sixthUnion.LeftTerm;
                Assert.That(sixthConcat.Pattern, Is.EqualTo("%="));

                Assert.That(sixthConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode sixthLeftSymbol = (SymbolOperatorNode)sixthConcat.LeftFactor;
                Assert.That(sixthLeftSymbol.Pattern, Is.EqualTo("%"));

                Assert.That(sixthConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode sixthRightSymbol = (SymbolOperatorNode)sixthConcat.RightFactor;
                Assert.That(sixthRightSymbol.Pattern, Is.EqualTo("="));

                Assert.That(sixthUnion.RightTerm, Is.TypeOf(typeof(UnionOperatorNode)));
                UnionOperatorNode seventhUnion = (UnionOperatorNode)sixthUnion.RightTerm;
                Assert.That(seventhUnion.Pattern, Is.EqualTo(@"\+\+|\-\-"));

                Assert.That(seventhUnion.LeftTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode seventhConcat = (ConcatenationOperatorNode)seventhUnion.LeftTerm;
                Assert.That(seventhConcat.Pattern, Is.EqualTo(@"\+\+"));

                Assert.That(seventhConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode seventhLeftSymbol = (SymbolOperatorNode)seventhConcat.LeftFactor;
                Assert.That(seventhLeftSymbol.Pattern, Is.EqualTo(@"\+"));

                Assert.That(seventhConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode seventhRightSymbol = (SymbolOperatorNode)seventhConcat.RightFactor;
                Assert.That(seventhRightSymbol.Pattern, Is.EqualTo(@"\+"));

                Assert.That(seventhUnion.RightTerm, Is.TypeOf(typeof(ConcatenationOperatorNode)));
                ConcatenationOperatorNode eighthConcat = (ConcatenationOperatorNode)seventhUnion.RightTerm;
                Assert.That(eighthConcat.Pattern, Is.EqualTo(@"\-\-"));

                Assert.That(eighthConcat.LeftFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode eighthLeftSymbol = (SymbolOperatorNode)eighthConcat.LeftFactor;
                Assert.That(eighthLeftSymbol.Pattern, Is.EqualTo(@"\-"));

                Assert.That(eighthConcat.RightFactor, Is.TypeOf(typeof(SymbolOperatorNode)));
                SymbolOperatorNode eighthRightSymbol = (SymbolOperatorNode)eighthConcat.RightFactor;
                Assert.That(eighthRightSymbol.Pattern, Is.EqualTo(@"\-"));
            });
        }

        [Test]
        public void PunctuationRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Punctuation.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode charSet = (CharacterSetOperatorNode)root;
                Assert.That(charSet.Pattern, Is.EqualTo(@"[\(\){}\[\];:,\.]"));

                List<string> expectedPatterns = [@"\(", @"\)", "{", "}", @"\[", @"\]", ";", ":", ",", @"\."];
                AssertCharSetPatternsAreEqual(charSet, expectedPatterns);
            });
        }

        [Test]
        public void WhitespaceRule_ExpectedOperatorTree()
        {
            IOperatorNode root = _regexParser.GetOperatorTree(Definitions.Whitespace.Pattern);

            Assert.Multiple(() =>
            {
                Assert.That(root, Is.TypeOf(typeof(PlusOperatorNode)));
                PlusOperatorNode plus = (PlusOperatorNode)root;
                Assert.That(plus.Pattern, Is.EqualTo(@"[ \\t\\r\\n]+"));

                Assert.That(plus.Operand, Is.TypeOf(typeof(CharacterSetOperatorNode)));
                CharacterSetOperatorNode charSet = (CharacterSetOperatorNode)plus.Operand;
                Assert.That(charSet.Pattern, Is.EqualTo(@"[ \\t\\r\\n]"));

                List<string> expectedPatterns = [" ", @"\\t", @"\\r", @"\\n"];
                AssertCharSetPatternsAreEqual(charSet, expectedPatterns);
            });
        }

        private void AssertCharSetPatternsAreEqual(CharacterSetOperatorNode charSet, List<string> expectedPatterns)
        {
            Assert.That(charSet.Symbols, Has.Count.EqualTo(expectedPatterns.Count));
            Assert.Multiple(() =>
            {
                for (int i = 0; i < charSet.Symbols.Count; i++)
                {
                    Assert.That(charSet.Symbols[i].Pattern, Is.EqualTo(expectedPatterns[i]));
                }
            });
        }
    }
}