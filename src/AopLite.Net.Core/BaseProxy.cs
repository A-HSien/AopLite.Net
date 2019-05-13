using System;
using System.Collections.Generic;
using System.Text;

namespace AopLite.Net.Core
{
    public class BaseProxy
    {
        public Dictionary<string, object> methodResults = null;


        public void SetMethodResults(Dictionary<string, object> methodResults)
        {
            this.methodResults = methodResults;
        }

        public object GetMethodResult(string methodName)
        {
            if (methodResults.ContainsKey(methodName))
                return methodResults[methodName];
            return null;
        }

    }
}
