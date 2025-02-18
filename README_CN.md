# CodeGen

[中文](./README_CN.md) / [EN](./README.md)

 Unity使用的代码生成工具。

测试版本：Unity - 2022.3 

Rosyln版本：3.8



## 使用方法

1. 将dll导入Assets下的文件夹中
2. 在Inspector中，Select Platforms/Disable Any Platform; 并在Include Platorms/disable Editor and Standalone。即禁用所有平台相关的工具
3. 在Asset Labels中创建一个名为`RoslynAnalyzer`的标签（注意大小写敏感）

- 应用范围：
  - 您可以通过使用程序集定义来限制项目中分析器的作用域，以便它们只分析代码的某些部分。Unity 将分析器应用于**项目 Assets 文件夹中的所有程序集**，或应用于**任何父文件夹不包含程序集定义文件的子文件夹中的程序集**。如果分析器位于包含程序集定义的文件夹中，或位于此类文件夹的子文件夹中，则分析器**仅适用于从该程序集定义生成的程序集**，以及**引用它的任何其他程序集**。(摘自Unity手册)


## Register

作用：利用`[Register("xxx")]`特性，将不同子类注册到分布类的同一个字典中。

```csharp
//特性模板
public class RegisterAttribute:Attribute
{
    //生成的Helper的名字
    public string registerTypeName;

    public RegisterAttribute(string registerTypeName)
    {
        this.registerTypeName = registerTypeName;
    }
    
}

```



```csharp
//使用例子
namespace CodeGen_Register
{
    //常量，只会读取变量名
    public static class Magic
    {
        public const string BaseSample1 = nameof(BaseSample1);
        
    }

    //注册Helper名可填入两种:
    //1. 常量，取变量名
    public class BaseSample1
    {
        
    }

    [Register(Magic.BaseSample1)]
    public class SampleA : BaseSample1
    {
    }


    [Register(Magic.BaseSample1)]
    public class SampleB : BaseSample1
    {
    }
    
     //生成代码
    /*
    using System.Collections.Generic;
    namespace CodeGen_Register{
    public static partial class BaseSample1Helper {
        public static Dictionary<string,BaseSample1> Type = new Dictionary<string,BaseSample1>(){
            { "SampleA",new SampleA() },
            { "SampleB",new SampleB() },
        };
     }
    }
    */
    
    

	//2. 字符串，取字面值
    public class BaseSample2 { }
    
    
    [Register("BaseSample2")]
    public class SampleA2 : BaseSample2
    {
    }


    [Register("BaseSample2")]
    public class SampleB2 : BaseSample2
    {
    }
    
    //生成代码
    /*
    using System.Collections.Generic;
    namespace CodeGen_Register{
    public static partial class BaseSample2Helper {
        public static Dictionary<string,BaseSample2> Type = new Dictionary<string,BaseSample2>(){
            { "SampleA2",new SampleA2() },
            { "SampleB2",new SampleB2() },
        };
     }
    }
    */
    
    
    public class Main
    {
        void main()
        {
            //如果成功生成此时可以直接使用
             var list1 = BaseSample1Helper.Type;
             var list2 = BaseSample2Helper.Type;
        }
    }
}
```





