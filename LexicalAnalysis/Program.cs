using LexicalAnalysis.Automata;
using LexicalAnalysis.Rules;
using LexicalAnalysis.Rules.OperatorNodes;
using LexicalAnalysis.Tokens;

namespace LexicalAnalysis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Parse regular expressions.
            RegularExpressionParser regexParser = new();
            Dictionary<TokenName, IOperatorNode> operatorTrees = [];
            foreach (TokenRule tokenRule in Definitions.Rules)
            {
                IOperatorNode operatorTree = regexParser.GetOperatorTree(tokenRule.Pattern);
                operatorTrees.Add(tokenRule.Name, operatorTree);
            }

            // Build automaton.
            HashSet<NondeterministicFiniteAutomaton> rulesAutomatons = [];
            foreach (KeyValuePair<TokenName, IOperatorNode> pair in operatorTrees)
            {
                NondeterministicFiniteAutomaton automaton = AutomatonTools.BuildFromOperatorTree(pair.Key, pair.Value);
                rulesAutomatons.Add(automaton);
            }
            NondeterministicFiniteAutomaton master = AutomatonTools.MergeAll(rulesAutomatons);
            DeterministicFiniteAutomaton deterministicMaster = AutomatonTools.ConvertToDeterministic(master);
            DeterministicFiniteAutomaton minimizedMaster = AutomatonTools.Minimize(deterministicMaster);
            Console.WriteLine($"Master states : {master.Transitions.GetAllStates().Count}");
            Console.WriteLine($"DetMaster states : {deterministicMaster.Transitions.GetAllStates().Count}");
            Console.WriteLine($"MinMaster states : {minimizedMaster.Transitions.GetAllStates().Count}");
            Console.WriteLine();

            // Create input reader.
            int bufferLength = 32;
            string inputFilePath = "C:\\Users\\alexi\\OneDrive\\Bureau\\Compilation & Interprétation\\TP1\\LexicalAnalysis\\Data\\input.cf";
            InputReader inputReader = new(bufferLength, inputFilePath);

            // Get tokens.
            Tokenizer tokenizer = new(inputReader, minimizedMaster);
            List<Token> tokens = [];
            Token newToken;
            do
            {
                Console.WriteLine("token");
                newToken = tokenizer.GetNextToken();
                tokens.Add(newToken);
                Console.WriteLine($"<{newToken.Name}, {newToken.Value}>");
            }
            while (newToken.Name != TokenName.End);

            Console.WriteLine();
            Console.WriteLine($"Token count : {tokens.Count}");
        }
    }
}
