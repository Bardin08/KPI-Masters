 using System.Text.Json;
using System.Text.Json.Serialization;
using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Visualization;

public class JsonTreeVisualizer : ITreeVisualizer
{
    public string Visualize(INode tree)
    {
        return JsonSerializer.Serialize(tree,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new NodeJsonConverter()
                }
            });
    }

    private class NodeJsonConverter : JsonConverter<INode>
    {
        public override INode Read(ref Utf8JsonReader reader, Type typeOfObject, JsonSerializerOptions options)
        {
            throw new InvalidOperationException("Deserialization is not supported.");
        }

        public override void Write(Utf8JsonWriter writer, INode node, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            WriteNodeDetails(writer, node);
            writer.WriteEndObject();
        }

        private void WriteNodeDetails(Utf8JsonWriter writer, INode node)
        {
            writer.WriteString("type", node.GetType().Name);
            writer.WriteString("tokenType", node.Type.ToString());

            switch (node)
            {
                case BinaryNode b:
                    WriteBinaryNodeDetails(writer, b);
                    break;

                case VariableNode v:
                    WriteVariableNodeDetails(writer, v);
                    break;

                case FunctionNode f:
                    WriteFunctionNodeDetails(writer, f);

                    break;

                case NumberNode n:
                    WriteNumberNodeDetails(writer, n);
                    break;

                default:
                    writer.WriteString("nodeClass", node.GetType().FullName);
                    break;
            }
        }

        private void WriteBinaryNodeDetails(Utf8JsonWriter writer, BinaryNode node)
        {
            writer.WriteString("operator", node.Operator);

            writer.WritePropertyName("left");
            Write(writer, node.Left, new JsonSerializerOptions());

            writer.WritePropertyName("right");
            Write(writer, node.Right, new JsonSerializerOptions());
        }

        private void WriteFunctionNodeDetails(Utf8JsonWriter writer, FunctionNode node)
        {
            writer.WriteString("name", node.Name);

            writer.WritePropertyName("arguments");
            writer.WriteStartArray();
            foreach (var argument in node.Arguments)
            {
                Write(writer, argument, new JsonSerializerOptions
                {
                    Converters = { new NodeJsonConverter() }
                });
            }

            writer.WriteEndArray();
        }

        private void WriteNumberNodeDetails(Utf8JsonWriter writer, NumberNode node)
        {
            writer.WriteString("value", node.Value);
        }

        private void WriteVariableNodeDetails(Utf8JsonWriter writer, VariableNode node)
        {
            writer.WriteString("name", node.Name);
        }
    }
}