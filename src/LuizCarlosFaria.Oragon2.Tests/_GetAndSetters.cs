using AmqpAdapters.Bus.Routers;
using AmqpAdapters.Rpc;
using System;
using System.Reflection;
using Xunit;

namespace LuizCarlosFaria.Oragon2.Tests;


public class GetAndSetters
{

    [Theory]
    [InlineData(typeof(TypeBasedRouter))]
    [InlineData(typeof(TypeAndFunctionBasedRouter))]
    [InlineData(typeof(FunctionBasedRouter))]

    public void GetAndSettersTest(Type type)
    {
        var instance = Activator.CreateInstance(type);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var oldValue = property.GetValue(instance);
            property.SetValue(instance, oldValue);
        }
    }



}