using System;
using System.Reflection.Emit;

namespace AopLite.Net.Core
{
    public static class ILGeneratorExtension
    {

        public static ILGenerator Then(this ILGenerator generator, Action<ILGenerator> action)
        {
            action(generator);
            return generator;
        }
    }
}
