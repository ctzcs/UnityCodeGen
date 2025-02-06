using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeGen
{
    
    public class RegisterSerializeData
    {
        public string typeNamespace;
        public string selfTypeName;
        public string baseTypeName;
        public string registerTypeName;

        public RegisterSerializeData()
        {
            typeNamespace = "";
            selfTypeName = "";
            baseTypeName = "";
            registerTypeName = "";
        }
    }
    
    [Generator]
    public class RegisterSourceGenerator : ISourceGenerator
    {
        private StringBuilder _stringBuilder = new StringBuilder();
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new CollectRegisterSyntaxReceiver()
            );
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is CollectRegisterSyntaxReceiver receiver)
            {
                foreach (var data in receiver.registerDataDic)
                {
                    var list = data.Value;
                    if (list.Count <= 0) continue;
                    var firstData = list.FirstOrDefault();
                    if (firstData is null) continue;
                    _stringBuilder.Clear();
                    _stringBuilder.Append("using System.Collections.Generic;\n");
                    _stringBuilder.Append($"namespace {firstData.typeNamespace}{{\n");
                    _stringBuilder.Append($"public static partial class {data.Key}Helper {{\n");

                    _stringBuilder.Append($"    public static Dictionary<string,{firstData.baseTypeName}> Type " +
                                          $"= new Dictionary<string,{firstData.baseTypeName}>(){{\n");
                    
                    foreach (var registerData in list)
                    {
                        _stringBuilder.Append($"        {{ \"{registerData.selfTypeName}\",new {registerData.selfTypeName}() }},\n");
                    }

                    _stringBuilder.Append($"    }};\n"); 
                    
                    _stringBuilder.Append(" }\n");
                    _stringBuilder.Append("}");
                    var src = _stringBuilder.ToString();
                    System.Diagnostics.Debug.WriteLine("Generated Source:");
                    System.Diagnostics.Debug.WriteLine(src);
                    context.AddSource($"{data.Key}.gen.cs",SourceText.From(src,Encoding.UTF8));
                }
            }
        }
    }
    
    public class CollectRegisterSyntaxReceiver : ISyntaxReceiver
    {
        //注册的类型名对应要注册进入的类型
        public Dictionary<string,List<RegisterSerializeData>> registerDataDic = new Dictionary<string, List<RegisterSerializeData>>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classNode)
            {
                foreach (var attributeList in classNode.AttributeLists)
                {
                    foreach (var atr in attributeList.Attributes)
                    {
                        if (atr.Name.ToString().Contains("Register"))
                        {
                            // 若找到包含 "Register" 特性的类，可在此处进行处理
                            var registerData = new RegisterSerializeData();
                            //获取命名空间
                            var namespaceNode = classNode.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                            if (namespaceNode != null)
                            {
                                registerData.typeNamespace = namespaceNode.Name.ToString();
                            }
                            
                            //获取自己的类型名
                            registerData.selfTypeName = classNode.Identifier.ToString();
                            //获取注册到哪个类型里面
                            var registerTypeNameSyntax = atr.ArgumentList?.Arguments.FirstOrDefault(
                                argSyntax => 
                                    (argSyntax.NameEquals != null && argSyntax.NameEquals.Name.Identifier.ValueText == "registerTypeName") || //赋值表达式
                                    (argSyntax.NameColon != null && argSyntax.NameColon.Name.Identifier.ValueText == "registerTypeName") ||  //冒号表达式
                                    (argSyntax.NameColon == null && argSyntax.NameEquals == null && atr.ArgumentList.Arguments.IndexOf(argSyntax) == 0));// 位置参数情况（第一个参数）
                            if (registerTypeNameSyntax != null)
                            {
                                if (registerTypeNameSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                                {
                                    // 处理类似 MyClass.TypeName 这样的常量访问
                                    if (memberAccessExpressionSyntax.Name is IdentifierNameSyntax memberIdentifier)
                                    {
                                        registerData.registerTypeName = memberIdentifier.Identifier.ValueText;
                                    }
                                }
                                else if (registerTypeNameSyntax.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                                {
                                    registerData.registerTypeName = literalExpressionSyntax.Token.ValueText;
                                }
                                else if (registerTypeNameSyntax.Expression is IdentifierNameSyntax identifierNameSyntax)
                                {
                                    registerData.registerTypeName = identifierNameSyntax.Identifier.ValueText;
                                }
                                else
                                {
                                    throw new ArgumentException($"类 {registerData.selfTypeName} 的 registerTypeName 参数必须是字符串字面量或 nameof 表达式");
                                }
                            }

                            var baseClassSyntax = classNode.BaseList?.Types.First();
                            if (baseClassSyntax != null)
                            {
                                registerData.baseTypeName = baseClassSyntax.Type.ToString();
                            }
                            
                            if (!registerDataDic.TryGetValue(registerData.registerTypeName,out var list))
                            {
                                list = new List<RegisterSerializeData>();
                                registerDataDic.Add(registerData.registerTypeName,list);
                            }
                            //类型对比
                            if (list.Count > 0)
                            {
                                var first = list.FirstOrDefault();
                                if (first == null)
                                    throw new NullReferenceException($"Null Happened In Some Type Of {registerData.registerTypeName}");
                                if (first.baseTypeName != registerData.baseTypeName)
                                {
                                    throw new ArgumentException($"{registerData.selfTypeName} or {first.selfTypeName} have unique baseTypeName");
                                }
                            }
                            list.Add(registerData);
                            return;
                        }
                    }
                }
            }
        }


        
    }
}


/*

public class RegisterAttribute:Attribute
{
    public string registerTypeName;

    public RegisterAttribute(string registerTypeName)
    {
        this.registerTypeName = registerTypeName;
    }
    
}

//用法:

*/