using Peat.Core.Lexer;
using Peat.Core.Parallelization;
using Peat.Core.Syntax.Nodes;
using Peat.Core.Syntax.Parser;

namespace Peat.Domain.Tests;

public class ParallelTreeBuilderTests
{
    [Fact]
    public void Simple_Binary_Expression_Should_Have_Two_Levels()
    {
        // A + B
        var ast = new BinaryNode(
            new VariableNode("A"),
            "+",
            new VariableNode("B"));

        var builder = new ParallelTreeBuilder();
        var result = builder.Build(ast);

        Assert.Equal(1, result.Level);
        Assert.Equal(0, result.Dependencies[0].Level);
        Assert.Equal(0, result.Dependencies[1].Level);
    }

    [Fact]
    public void Sequential_Operations_Should_Create_Multiple_Levels()
    {
        // A + B + C -> (A + B) + C
        var ast = new BinaryNode(
            new BinaryNode(
                new VariableNode("A"),
                "+",
                new VariableNode("B")),
            "+",
            new VariableNode("C"));

        var builder = new ParallelTreeBuilder();
        var result = builder.Build(ast);
        var metrics = builder.GetMetrics();

        Assert.Equal(2, result.Level);
        Assert.Equal(5, metrics.TotalNodes);
        Assert.Equal(3, metrics.Height);
        Assert.True(metrics.LevelDistribution[0] >= 2); // Variables at level 0
    }

    [Fact]
    public void Function_Call_Should_Process_Arguments_In_Parallel()
    {
        // f(A+B, C+D)
        var ast = new FunctionNode("f", new List<INode>
        {
            new BinaryNode(new VariableNode("A"), "+", new VariableNode("B")),
            new BinaryNode(new VariableNode("C"), "+", new VariableNode("D"))
        });

        var builder = new ParallelTreeBuilder();
        var result = builder.Build(ast);
        var metrics = builder.GetMetrics();

        Assert.Equal(2, result.Level);
        Assert.Equal(7, metrics.TotalNodes);
        Assert.Equal(3, metrics.Height);
        Assert.Equal(4, metrics.LevelDistribution[0]); // Variables
    }

    [Fact]
    public void Complex_Expression_Should_Have_Correct_Dependency_Order()
    {
        // (A+B)/(C+D) + E
        var ast = new BinaryNode(
            new BinaryNode(
                new BinaryNode(new VariableNode("A"), "+", new VariableNode("B")),
                "/",
                new BinaryNode(new VariableNode("C"), "+", new VariableNode("D"))),
            "+",
            new VariableNode("E"));

        var builder = new ParallelTreeBuilder();
        var result = builder.Build(ast);

        // Verify dependencies are correct
        Assert.All(result.Dependencies, d => Assert.True(d.Level < result.Level));
        Assert.Equal(9, builder.GetMetrics().TotalNodes);
    }

    [Fact]
    public void Cyclic_References_Should_Be_Handled()
    {
        var a = new VariableNode("A");
        var b = new VariableNode("B");
        var binaryNode1 = new BinaryNode(a, "+", b);
        var binaryNode2 = new BinaryNode(binaryNode1, "+", binaryNode1); // Same node referenced twice

        var builder = new ParallelTreeBuilder();
        var result = builder.Build(binaryNode2);
        var metrics = builder.GetMetrics();

        Assert.True(metrics.TotalNodes < 5); // Should reuse nodes
        Assert.All(result.Dependencies,
            d => Assert.Equal(d, result.Dependencies[0])); // Both dependencies should reference same node
    }

    public class PrattParserParenthesesTests
    {
        private readonly IParser _parser = new PrattParser();
        private readonly ILexer _lexer = new Lexer();

        [Fact]
        public void Parse_Nested_Parentheses()
        {
            var tokens = _lexer.Tokenize("((A+B)+C)");
            var ast = _parser.Parse(tokens);
        
            Assert.IsType<BinaryNode>(ast);
            var root = (BinaryNode)ast;
            Assert.IsType<BinaryNode>(root.Left);
        }

        [Fact]
        public void Parse_Unmatched_Opening_Parenthesis()
        {
            Assert.Throws<BulkParserException>(() => 
                _parser.Parse(_lexer.Tokenize("(A+B")));
        }

        [Fact]
        public void Parse_Unmatched_Closing_Parenthesis()
        {
            Assert.Throws<BulkParserException>(() => 
                _parser.Parse(_lexer.Tokenize("A+B)")));
        }

        [Fact]
        public void Parse_Empty_Parentheses()
        {
            Assert.Throws<BulkParserException>(() => 
                _parser.Parse(_lexer.Tokenize("()")));
        }

        [Fact]
        public void Parse_Multiple_Parentheses_Groups()
        {
            var tokens = _lexer.Tokenize("(A+B)*(C+D)");
            var ast = _parser.Parse(tokens);
        
            Assert.IsType<BinaryNode>(ast);
            var root = (BinaryNode)ast;
            Assert.IsType<BinaryNode>(root.Left);
            Assert.IsType<BinaryNode>(root.Right);
        }
    }
}