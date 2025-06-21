﻿namespace ZODs.Api.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class PaidPlanOnly : Attribute
{
}
