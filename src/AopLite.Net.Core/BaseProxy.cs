using System;
using System.Collections.Generic;
using System.Text;

namespace AopLite.Net.Core
{
    public abstract class BaseProxy
    {
        public Dictionary<string, object> MethodResults = null;


        public void SetMethodResults(Dictionary<string, object> methodResults)
        {
            this.MethodResults = methodResults;
        }

        public object GetMethodResult(string methodName)
        {
            if (MethodResults.ContainsKey(methodName))
                return MethodResults[methodName];
            return null;
        }

    }
}
