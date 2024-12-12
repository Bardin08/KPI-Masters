# PEAT CLI

PEAT (Parallel Expression Analysis Tool) - A tool for analyzing and optimizing parallel expressions.

```bash
peat [command] [options]
```

## Commands

### Core Analysis
Status: ❌

`analyze <expression>` - Parse and analyze expression details
```bash
# Options
-o, --output <format>       Output format (text/json/graph)
-v, --verbose              Show detailed analysis
-s, --save <filename>      Save results to file

# Example
peat analyze "a + b * (c - d)" --output json --verbose
```

### Syntax Parsing
Status: ❌

`parse <expression>` - Validate expression syntax
```bash
# Options
-o, --output <format>       Output format (text/json)
-v, --verbose              Show detailed parsing steps

# Example
peat parse "x * y + z" --verbose
```

### Tree Visualization
Status: ❌

`tree <expression>` - Generate expression tree
```bash
# Options
-o, --output <format>       Output format (text/graph/json)
-s, --save <filename>      Save tree visualization

# Example
peat tree "a/b/c" --output graph --save tree.png
```

### Parallel Optimization
Status: ❌

`optimize <expression>` - Find optimal parallel form
```bash
# Options
-p, --processors <number>    Set processor count
-a, --arch <type>           Architecture type (matrix/vliw/dataflow/pipeline)
-o, --output <format>       Output format

# Example
peat optimize "a + b * c" -p 4 --arch dataflow
```

### Performance Benchmarking
Status: ❌

`bench <expression>` - Benchmark parallel forms
```bash
# Options
-p, --processors <number>    Processor count
-a, --arch <type>           Architecture type
-v, --verbose              Show detailed metrics
-s, --save <filename>      Save benchmark results

# Example
peat bench "x + y * z" -p 4 --verbose
```

## Global Options
```bash
-h, --help                 Show command help
-v, --version              Show PEAT version
```