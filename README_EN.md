# CodeGen

[中文](./README.md) / [EN](./README_EN.md)

A code generation tool for Unity.

Tested Version: Unity - 2022.3  
Roslyn Version: 3.8

---

## Usage

1. Import the dll into a folder under Assets.
2. In the Inspector:
   - Select **Platforms** → **Disable Any Platform**
   - Under **Include Platforms**, disable **Editor** and **Standalone** (i.e., disable all platform-related tools).
3. Create a label named `RoslynAnalyzer` in **Asset Labels** (case-sensitive).

### Scope of Application
- You can limit the analyzer's scope using assembly definitions to target specific parts of your code. Unity applies analyzers to:
  - **All assemblies in the project's Assets folder**, or 
  - **Assemblies in subfolders where no parent folder contains an assembly definition file**. 
  - If an analyzer is placed in a folder with an assembly definition (or its subfolders), it **only applies to assemblies generated from that definition** and **any assemblies referencing it**. (Adapted from Unity Documentation)

---

## Register

**Purpose**: Use the `[Register("xxx")]` attribute to register subclasses into a shared dictionary within a parent class.

### Attribute Template
```csharp
public class RegisterAttribute : Attribute
{
    // The name of the generated Helper class
    public string RegisterTypeName;

    public RegisterAttribute(string registerTypeName)
    {
        RegisterTypeName = registerTypeName;
    }
}
```

### Usage Example
```csharp
namespace CodeGen_Register
{
    // Constants; only the variable name will be read
    public static class Magic
    {
        public const string BaseSample1 = nameof(BaseSample1);
    }

    // Register accepts two types of values:
    // 1. Constants (uses the variable name)
    public class BaseSample1 { }

    [Register(Magic.BaseSample1)]
    public class SampleA : BaseSample1 { }

    [Register(Magic.BaseSample1)]
    public class SampleB : BaseSample1 { }

    // Generated Code:
    /*
    using System.Collections.Generic;
    namespace CodeGen_Register {
        public static partial class BaseSample1Helper {
            public static Dictionary<string, BaseSample1> Type = new Dictionary<string, BaseSample1>() {
                { "SampleA", new SampleA() },
                { "SampleB", new SampleB() }
            };
        }
    }
    */

    // 2. String literals (uses the literal value)
    public class BaseSample2 { }

    [Register("BaseSample2")]
    public class SampleA2 : BaseSample2 { }

    [Register("BaseSample2")]
    public class SampleB2 : BaseSample2 { }

    // Generated Code:
    /*
    using System.Collections.Generic;
    namespace CodeGen_Register {
        public static partial class BaseSample2Helper {
            public static Dictionary<string, BaseSample2> Type = new Dictionary<string, BaseSample2>() {
                { "SampleA2", new SampleA2() },
                { "SampleB2", new SampleB2() }
            };
        }
    }
    */

    public class Main
    {
        void Main()
        {
            // If generated successfully, you can use them directly here
            var list1 = BaseSample1Helper.Type;
            var list2 = BaseSample2Helper.Type;
        }
    }
}
```

