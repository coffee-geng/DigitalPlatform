using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public class VariableNameGenerator
    {
        //字典保存类名和当前类的变量名生成器，确保在同一个类中变量名唯一
        private static readonly Dictionary<string, VariableNameGenerator> instanceDict = new Dictionary<string, VariableNameGenerator>();

        public static VariableNameGenerator GetInstance(string className)
        {
            if (instanceDict.ContainsKey(className))
            {
                return instanceDict[className];
            }
            else
            {
                var instance = new VariableNameGenerator();
                instanceDict.Add(className, instance);
            }
            return instanceDict[className];
        }

        private VariableNameGenerator()
        {
        }

        //保存已生成的变量名字，确保唯一
        private IList<string> _availableVariableNames = new List<string>();

        public string GenerateValidVariableName(string input, bool useCamelCase = false, bool isUniqueInSession = false)
        {
            //将中文转换为拼音
            string inputText = NPinyin.Pinyin.GetPinyin(input).Replace(" ", "");

            var variableName = GenerateValidVariableName(inputText, useCamelCase);
            var tempVariableName = variableName;
            if (isUniqueInSession)
            {
                string suffix = "";
                int i = 1;
                while (_availableVariableNames.Contains(tempVariableName))
                {
                    var lastChar = variableName.Last();
                    var tempName = variableName;
                    //如果最后字符是数字，则将其递增直到没有重复的变量名字
                    if (char.IsDigit(lastChar) && int.TryParse(lastChar.ToString(), out int lastDigital))
                    {
                        tempName = variableName.Substring(0, variableName.Length - 1);
                        lastDigital += i;
                        i++;

                        tempVariableName = tempName + lastDigital.ToString();
                    }
                    else
                    {
                        tempVariableName = tempName + i.ToString();
                    }
                }
                return tempVariableName;
            }
            else
            {
                return variableName;
            }
        }

        // C# 关键字列表
        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            // C# 关键字
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate",
            "do", "double", "else", "enum", "event", "explicit", "extern", "false",
            "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
            "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
            "new", "null", "object", "operator", "out", "override", "params", "private",
            "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
            "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
            "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        
            // 上下文关键字
            "add", "alias", "ascending", "async", "await", "by", "descending",
            "dynamic", "equals", "from", "get", "global", "group", "into", "join",
            "let", "nameof", "on", "orderby", "partial", "remove", "select", "set",
            "value", "var", "when", "where", "yield"
        };

        /// <summary>
        /// 将输入字符串转换为合法的C#变量名
        /// </summary>
        private static string GenerateValidVariableName(string input, bool useCamelCase = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "_";

            StringBuilder result = new StringBuilder();
            bool nextCharUpper = useCamelCase;

            // 处理第一个字符
            char firstChar = input[0];

            // 检查第一个字符是否合法
            if (!IsValidFirstCharacter(firstChar))
            {
                result.Append('_');
            }
            else
            {
                if (useCamelCase)
                    result.Append(char.ToLower(firstChar));
                else
                    result.Append(firstChar);
            }

            // 处理剩余字符
            for (int i = 1; i < input.Length; i++)
            {
                char currentChar = input[i];

                if (IsValidVariableCharacter(currentChar))
                {
                    if (nextCharUpper)
                    {
                        result.Append(char.ToUpper(currentChar));
                        nextCharUpper = false;
                    }
                    else
                    {
                        result.Append(currentChar);
                    }
                }
                else
                {
                    // 非合法字符转换为下划线，下一个字符大写（如果是camelCase）
                    if (result[result.Length - 1] != '_')
                    {
                        result.Append('_');
                    }
                    nextCharUpper = useCamelCase;
                }
            }

            string variableName = result.ToString();

            // 处理多个连续下划线的情况
            variableName = Regex.Replace(variableName, @"_+", "_");

            // 移除结尾的下划线（除非整个字符串都是下划线）
            if (variableName.Length > 1 && variableName.EndsWith("_"))
            {
                variableName = variableName.TrimEnd('_');
            }

            // 检查是否是关键字
            if (IsCSharpKeyword(variableName))
            {
                variableName = "@" + variableName;
            }

            return variableName;
        }

        private static bool IsValidFirstCharacter(char c)
        {
            return char.IsLetter(c) || c == '_' || c == '@';
        }

        private static bool IsValidVariableCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private static bool IsCSharpKeyword(string name)
        {
            return CSharpKeywords.Contains(name.ToLower());
        }
    }
}
