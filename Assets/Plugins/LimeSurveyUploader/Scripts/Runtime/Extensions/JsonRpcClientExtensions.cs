﻿using GEAR.Gadgets.Extensions;
using Newtonsoft.Json.Linq;

namespace GameLabGraz.LimeSurvey.Extensions
{
    public static class JsonRpcClientExtensions
    {
        public static void SetMethod(this JsonRpcClient client, LimeSurveyMethod method)
        {
            client.Method = method.GetStringValue();
        }

        public static void AddParameter(this JsonRpcClient client, LimeSurveyParameter parameter, JToken value)
        {
            client.Parameters.Add(parameter.GetStringValue(), value);
        }
    }
}
