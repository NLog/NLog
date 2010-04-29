using System;
using System.Reflection;

class C1
{
    static int Main(string[] args)
    {
        try
        {
            string assemblyName = args[0];
            string className = args[1];
            string methodName = args[2];
            object[] arguments = new object[args.Length - 3];
            for (int i = 0; i < arguments.Length; ++i)
                arguments[i] = args[3 + i];

            Assembly assembly = Assembly.Load(assemblyName);
            Type type = assembly.GetType(className);
            if (type == null)
                throw new Exception(className + " not found in " + assemblyName);
            MethodInfo method = type.GetMethod(methodName);
            if (method == null)
                throw new Exception(methodName + " not found in " + type);
            object targetObject = null;
            if (!method.IsStatic)
                targetObject = Activator.CreateInstance(type);
            method.Invoke(targetObject, arguments);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
    }
}
