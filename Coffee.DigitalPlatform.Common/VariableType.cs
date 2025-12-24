namespace Coffee.DigitalPlatform.Common;

public class VariableType
{
    public VariableType(Type typeClass, string abbr)
    {
        TypeClass = typeClass;
        TypeAbbrName = abbr;
    }

    //变量类型
    public Type TypeClass {  get; private set; }

    //变量类型类名，如UInt16
    public string TypeName
    {
        get
        {
            return TypeClass.Name;
        }
    }

    //变量类型简写，如UInt16等价于ushort
    public string TypeAbbrName {  get; set; }
}

public class PrimitiveVariableTypeFactory
{
    public IList<VariableType> CreateVariableTypes()
    {
        return new List<VariableType>()
        {
            new VariableType(typeof(bool), "bool"),
            new VariableType(typeof(byte), "byte"),
            new VariableType(typeof(ushort), "ushort"),
            new VariableType(typeof(short), "short"),
            new VariableType(typeof(uint), "uint"),
            new VariableType(typeof(int), "int"),
            new VariableType(typeof(ulong), "ulong"),
            new VariableType(typeof(long), "long"),
            new VariableType(typeof(float), "float"),
            new VariableType(typeof(double), "double"),
        };
    }
}
