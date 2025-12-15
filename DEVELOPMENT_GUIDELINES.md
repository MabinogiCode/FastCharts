# FastCharts Development Guidelines

## Code Quality Standards

### Namespace Organization
- Namespaces must correspond to folder/path structure
- Follow the pattern: `FastCharts.{ProjectName}.{FolderName}.{SubFolder}`
- Example: `FastCharts.Core.Axes`, `FastCharts.Rendering.Skia.Helpers`

### Language Requirements
- All methods and comments must be in English
- Use clear, descriptive names for all public APIs
- Avoid French abbreviations or terminology in code

### File Organization (StyleCop SA1402)
- **One element per file**: Each file must contain only one class, enum, struct, record, or interface
- **No nested types**: Avoid nested classes/structs/records/interfaces within public types
- **No static methods in instantiable objects**: If a class has instance members, move static methods to separate static utility classes
- **Test file naming**: Test files should match their target class name with "Tests" suffix

#### Current Violation Example (to fix):
```csharp
// ? BAD: LogNumericAxisTests.cs contains 4 classes
public class LogNumericAxisTests { }
public class LogScaleTests { }
public class LogTickerTests { }
public class ScientificNumberFormatterTests { }
```

#### Correct Structure:
```
tests/FastCharts.Core.Tests/Axes/
??? LogNumericAxisTests.cs      // Only LogNumericAxisTests
??? LogScaleTests.cs           // Only LogScaleTests  
??? LogTickerTests.cs          // Only LogTickerTests
??? Formatting/
    ??? ScientificNumberFormatterTests.cs  // Only ScientificNumberFormatterTests
```

### Naming Conventions
- **No underscores in method names**: Applies to both public and private methods
- Use PascalCase for public members, camelCase for private fields
- Test methods: `{MethodName}_{Scenario}_{ExpectedBehavior}`

### Architecture Principles
- Apply **SOLID principles**
- Follow **Clean Architecture** patterns
- Implement **Clean Code** practices as defined in .editorconfig
- Use **Clean Testing** methodologies
- Comply with StyleCop, Roslyn, and NetAnalyzers rules

### Control Flow Formatting
- **All control structures must use braces `{ ... }`**, even for single statements
- **Brace placement**: Opening brace must be on the next line after the control expression

#### Accepted Examples:
```csharp
if (result)
{
    return;
}

foreach (var value in testValues)
{
    Process(value);
}
```

#### Prohibited Examples:
```csharp
if (result) return;           // Missing braces
if (x) { Do(); }             // Brace on same line
```

### Testing Standards
- **Arrange-Act-Assert** pattern mandatory
- Use FluentAssertions for readable assertions
- Comprehensive test coverage for all public APIs
- Performance tests for algorithms (LTTB, rendering)
- Mock external dependencies appropriately
- Test edge cases and error conditions

#### Test Method Structure:
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = CreateTestData();
    
    // Act
    var result = systemUnderTest.Method(input);
    
    // Assert
    result.Should().Be(expectedValue);
}
```

## Build Requirements
- **Multi-target**: .NET Standard 2.0, .NET 6, .NET 8, .NET Framework 4.8
- **Cross-platform**: Windows, macOS, Linux support
- **Rendering backends**: 
  - Skia for cross-platform (SkiaSharp)
  - WPF for Windows-specific features
- Zero build errors and warnings
- Strict code analysis enabled (`TreatWarningsAsErrors=true`)

## Performance Guidelines
- Target smooth rendering for datasets up to 10M points
- Implement efficient resampling algorithms (LTTB)
- Memory-conscious data structures
- GPU acceleration where beneficial (via Skia)
- Benchmark critical paths

## Documentation Requirements
- XML documentation for all public APIs
- Inline comments for complex algorithms
- Architecture decision records (ADRs) for major changes
- Performance characteristics documented
- Chart type examples and usage patterns

## Project Structure Standards
```
src/
??? FastCharts.Core/           // Core abstractions and algorithms
??? FastCharts.Rendering.Skia/ // Cross-platform rendering
??? FastCharts.Wpf/           // Windows-specific controls

tests/
??? FastCharts.Core.Tests/     // Core library tests
??? FastCharts.Tests/          // Integration and rendering tests
```

### Dependency Rules
- **Core**: No UI dependencies, pure algorithms and data structures
- **Rendering.Skia**: Depends on Core + SkiaSharp only
- **Wpf**: Depends on Core + Rendering.Skia + WPF frameworks

## Immediate Action Required

### File Split Task
The following files violate SA1402 and must be split:

1. **`tests/FastCharts.Core.Tests/Axes/LogNumericAxisTests.cs`**
   - Split into 4 separate files
   - Move `ScientificNumberFormatterTests` to `Formatting/` folder
   - Maintain existing test logic and assertions

## Automatic Correction Guidelines
- Add missing braces `{ }` without changing logic
- Preserve existing indentation and code style
- Maintain using statements and namespace structure
- Split multi-class files while preserving all tests

---

**Version**: 1.0  
**Last Updated**: Current Session  
**Compliance**: Mandatory for all contributions  
**Priority**: Fix SA1402 violations immediately