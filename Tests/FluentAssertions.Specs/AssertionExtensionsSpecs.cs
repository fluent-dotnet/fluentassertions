﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions.Types;
using Xunit;

namespace FluentAssertions.Specs
{
    public class AssertionExtensionsSpecs
    {
        [Fact]
        public void Should_methods_have_a_matching_overload_to_guard_against_chaining_and_constraints()
        {
            // Arrange / Act
            List<MethodInfo> shouldOverloads = AllTypes.From(typeof(FluentAssertions.AssertionExtensions).Assembly)
                .ThatAreClasses()
                .ThatAreStatic()
                .Where(t => t.IsPublic)
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(m => m.Name == "Should")
                .ToList();

            List<Type> realOverloads = shouldOverloads
                .Where(m => !IsGuardOverload(m))
                .Select(t => GetMostParentType(t.ReturnType))
                .Distinct()
                .ToList();

            List<Type> fakeOverloads = shouldOverloads
                .Where(m => IsGuardOverload(m))
                .Select(e => e.GetParameters()[0].ParameterType)
                .ToList();

            // Assert
            fakeOverloads.Should().BeEquivalentTo(realOverloads, opt => opt
                .Using<Type>(ctx => ctx.Subject.Name.Should().Be(ctx.Expectation.Name))
                .WhenTypeIs<Type>());
        }

        private static bool IsGuardOverload(MethodInfo m) =>
            m.ReturnType == typeof(void) && m.IsDefined(typeof(ObsoleteAttribute));

        private static Type GetMostParentType(Type type)
        {
            while (type.BaseType != typeof(object))
            {
                type = type.BaseType;
            }

            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            return type;
        }
    }
}