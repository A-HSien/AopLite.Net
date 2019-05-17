using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AopLite.Net.Core
{
    public class ProxyFactory<T>
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


        public virtual T GetProxy()
        {
            var generatedType = GetProxyType();
            return (T)Activator.CreateInstance(generatedType, new[] { new Dictionary<string, object>() });
        }

        
        protected virtual void createConstructor(TypeBuilder typeBuilder)
        {
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                    new Type[] { typeof(Dictionary<string, object>) });


            constructor.GetILGenerator().Then(generator =>
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, typeof(BaseProxy).GetField(nameof(BaseProxy.MethodResults), BindingFlags.Instance | BindingFlags.Public));
                generator.Emit(OpCodes.Ret);
            });
        }

        public Type GetProxyType()
        {
            if (generatedType == null)
            {
                var sourceType = typeof(T);
                TypeBuilder typeBuilder = getTypeBuilder(sourceType);
                createConstructor(typeBuilder);

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
                        createMethod(method, typeBuilder);
                }
                generatedType = typeBuilder.CreateTypeInfo();
            }
            return generatedType;
        }
        
        protected virtual Type baseType=> typeof(BaseProxy);

        private TypeBuilder getTypeBuilder(Type proxyTargetType)
        {
            var typeSignature = $"{proxyTargetType.Name}Proxy_{Guid.NewGuid().ToString().Replace("{", "").Replace('-', '$')}";
            var assemblyName = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                typeSignature
                , TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass
                | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout
                , baseType
                , new Type[] { proxyTargetType }
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


            getPropMthdBldr.GetILGenerator().Then(generator =>
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


            setPropMthdBldr.GetILGenerator().Then(generator =>
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


        protected virtual void createMethod(MethodInfo method, TypeBuilder typeBuilder)
        {
            createDefaultMethod(typeBuilder, method.Name, method.GetParameters().Select(a => a.ParameterType).ToArray(), method.ReturnType);
        }

        protected void createDefaultMethod(TypeBuilder tb, string methodName, Type[] argsType, Type returnType)
        {
            MethodBuilder methodBuilder = tb.DefineMethod(
                methodName
                , MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual
                , returnType
                , argsType
                );


            methodBuilder.GetILGenerator().Then(generator =>
           {
               if (returnType != typeof(void))
               {
                   generator.Emit(OpCodes.Ldarg_0);
                   generator.Emit(OpCodes.Ldstr, methodName);
                   var proxyMethod = typeof(BaseProxy).GetMethod(nameof(BaseProxy.GetMethodResult), BindingFlags.Public | BindingFlags.Instance);
                   generator.Emit(OpCodes.Call, proxyMethod);
                   generator.Emit(OpCodes.Ret);
               }
           });
        }
    }
}
