using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AopLite.Net.Core
{
    public class ProxyFactory
    {

        private Type generatedType = null;
        private Dictionary<string, object> methodResults = new Dictionary<string, object>();



        public object GetReturnValue(string methodName)
        {
            return methodResults[methodName];
        }

        public void SetMethodResult(string methodName, object result)
        {
            if (methodResults.ContainsKey(methodName))
                methodResults[methodName] = result;
            else
                methodResults.Add(methodName, result);
        }

        public object GetProxy(Type sourceType)
        {
            if (generatedType == null)
            {
                TypeBuilder typeBuilder = getTypeBuilder(sourceType);
                ConstructorBuilder constructor = typeBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                    new Type[] { typeof(Dictionary<string, object>) });


                createILGeneratorContext(
                    constructor.GetILGenerator(),
                    generator =>
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Stfld, typeof(BaseProxy).GetField(nameof(BaseProxy.methodResults), BindingFlags.Instance | BindingFlags.Public));
                    generator.Emit(OpCodes.Ret);
                });

                var omitMethods = new List<string>();
                foreach (var field in sourceType.GetProperties())
                {
                    createProperty(typeBuilder, field.Name, field.PropertyType);
                    omitMethods.Add("get_" + field.Name);
                    omitMethods.Add("set_" + field.Name);
                }
                foreach (var method in sourceType.GetMethods())
                {
                    if (omitMethods.IndexOf(method.Name) == -1)
                    {
                        createMethod(method, typeBuilder);
                    }
                }
                generatedType = typeBuilder.CreateTypeInfo();
            }

            ConstructorInfo constructorInfo = generatedType.GetConstructor(new Type[] { typeof(Dictionary<string, object>) });
            return constructorInfo.Invoke(new object[] { methodResults });
        }

        protected virtual void createMethod(MethodInfo method, TypeBuilder typeBuilder)
        {
            createDefaultMethod(typeBuilder, method.Name, method.GetParameters().Select(a => a.ParameterType).ToArray(), method.ReturnType);
        }

        private TypeBuilder getTypeBuilder(Type apiType)
        {
            var typeSignature = $"DynamicType_{Guid.NewGuid().ToString().Replace("{", "").Replace('-', '$')}";
            var assemblyName = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                typeSignature
                , TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass
                | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout
                , typeof(BaseProxy)
                , new Type[] { apiType }
                );
            return typeBuilder;
        }

        private void createProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.SpecialName, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod(
                "get_" + propertyName
                , MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig
                | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Virtual
                , propertyType
                , Type.EmptyTypes
                );


            createILGeneratorContext(getPropMthdBldr.GetILGenerator(), generator =>
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, fieldBuilder);
                generator.Emit(OpCodes.Ret);
            });


            MethodBuilder setPropMthdBldr =
                tb.DefineMethod(
                    "set_" + propertyName
                    , MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig
                    | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Virtual
                    , null
                    , new[] { propertyType }
                    );


            createILGeneratorContext(setPropMthdBldr.GetILGenerator(), generator =>
            {
                Label modifyProperty = generator.DefineLabel();
                Label exitSet = generator.DefineLabel();

                generator.MarkLabel(modifyProperty);
                generator.Emit(OpCodes.Ldarg_0); // this
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, fieldBuilder);

                generator.Emit(OpCodes.Nop);
                generator.MarkLabel(exitSet);
                generator.Emit(OpCodes.Ret);
            });


            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }


        protected void createDefaultMethod(TypeBuilder tb, string methodName, Type[] argsType, Type returnType)
        {
            MethodBuilder methodBuilder = tb.DefineMethod(
                methodName
                , MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual
                , returnType
                , argsType
                );


            createILGeneratorContext(methodBuilder.GetILGenerator(), generator =>
            {
                if (returnType != typeof(void))
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, methodName);
                    generator.Emit(
                    OpCodes.Call
                    , typeof(BaseProxy).GetMethod(nameof(BaseProxy.GetMethodResult), BindingFlags.Public | BindingFlags.Instance)
                    );
                    generator.Emit(OpCodes.Ret);
                }
            });
        }


        protected static void createILGeneratorContext(ILGenerator generator, Action<ILGenerator> action) => action(generator);
    }
}
