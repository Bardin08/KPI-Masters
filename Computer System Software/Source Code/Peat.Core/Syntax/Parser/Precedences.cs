namespace Peat.Core.Syntax.Parser;

public static class Precedences
{
    private static readonly Dictionary<string, int> Precedence = new()
    {
        // Parentheses
        {"(", 8},
        {")", 8},
       
        // Functions
        {"sin", 7},
        {"cos", 7},
        {"sqrt", 7},

        // Unary operators
        {"+u", 6},  // unary plus
        {"-u", 6},  // unary minus
       
        // Multiplicative
        {"*", 5},
        {"/", 5},
       
        // Additive 
        {"+", 4},
        {"-", 4},
       
        // Function argument separator
        {",", 1}
    };

    public static int GetPrecedence(string op) => Precedence.GetValueOrDefault(op, 0);

    public static bool IsLeftAssociative(string op) => op switch
    {
        "+" or "-" or "*" or "/" => true,
        _ => false
    };
}