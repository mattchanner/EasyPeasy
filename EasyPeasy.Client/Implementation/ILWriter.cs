// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILWriter.cs">
//
//  The MIT License (MIT)
//  Copyright © 2013 Matt Channer (mchanner at gmail dot com)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a 
//  copy of this software and associated documentation files (the “Software”),
//  to deal in the Software without restriction, including without limitation 
//  the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//  and/or sell copies of the Software, and to permit persons to whom the 
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included 
//  in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS 
//  OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
//  THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyPeasy.Client.Implementation
{
    /// <summary>
    /// A set of utility methods for writing IL
    /// </summary>
    internal class ILWriter
    {
        /// <summary> The constructor attributes. </summary>
        private const MethodAttributes ConstructorAttributes =
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.RTSpecialName;

        /// <summary>
        /// Sets up a default constructor for the type
        /// </summary>
        /// <param name="typeBuilder"> The type builder. </param>
        /// <param name="baseClass">The base class</param>
        public static void CreateConstructor(TypeBuilder typeBuilder, Type baseClass)
        {
            ConstructorBuilder constructor =
                typeBuilder.DefineConstructor(
                    ConstructorAttributes,
                    CallingConventions.Standard,
                    Type.EmptyTypes);

            ConstructorInfo baseConstructor = baseClass.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                null,
                Type.EmptyTypes,
                new ParameterModifier[0]);

            ILGenerator il = constructor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0); // Load `this` onto the stack
            il.Emit(OpCodes.Call, baseConstructor); // Call the base constructor
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Sets the value of a property on an instance
        /// </summary>
        /// <param name="il"> The IL generator. </param>
        /// <param name="local"> The local builder containing the object reference. </param>
        /// <param name="type"> The type of object containing the property. </param>
        /// <param name="propertyName"> The property name. </param>
        /// <param name="propertyValue"> The property value. </param>
        public static void WritePropertyString(
            ILGenerator il,
            LocalBuilder local,
            Type type,
            string propertyName,
            string propertyValue)
        {
            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Ldstr, propertyValue);
            MethodInfo setter = type.GetProperty(propertyName).GetSetMethod();
            il.Emit(OpCodes.Call, setter);
        }

        /// <summary>
        /// Sets the value of a property on an instance
        /// </summary>
        /// <param name="il"> The IL generator. </param>
        /// <param name="local"> The local builder containing the object reference. </param>
        /// <param name="type"> The type of object containing the property. </param>
        /// <param name="propertyName"> The property name. </param>
        /// <param name="propertyValue"> The property value. </param>
        public static void WritePropertyInt(
            ILGenerator il,
            LocalBuilder local,
            Type type,
            string propertyName,
            int propertyValue)
        {
            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Ldc_I4, propertyValue);
            MethodInfo setter = type.GetProperty(propertyName).GetSetMethod();
            il.Emit(OpCodes.Call, setter);
        }

        /// <summary>
        /// Assigns the value of a property to a dictionary on the given local variable
        /// </summary>
        /// <param name="il">The IL generator</param>
        /// <param name="local">The local variable</param>
        /// <param name="parentType">The parent type</param>
        /// <param name="propertyName">The name of the property containing the dictionary</param>
        /// <param name="dictionaryKey">The key to add</param>
        /// <param name="parameter">The parameter to add as the value</param>
        public static void AssignParameterValueToDictionaryProperty(
            ILGenerator il, 
            LocalBuilder local, 
            Type parentType, 
            string propertyName,
            string dictionaryKey,
            ParameterInfo parameter)
        {
            Type dictType = typeof(IDictionary<string, object>);
            MethodInfo addMethod = dictType.GetMethod("Add");

            MethodInfo getter = parentType.GetProperty(propertyName).GetGetMethod();

            il.Emit(OpCodes.Ldloc, local);
            il.Emit(OpCodes.Call,  getter);
            il.Emit(OpCodes.Ldstr, dictionaryKey);
            il.Emit(OpCodes.Ldarg, parameter.Position + 1);

            if (parameter.ParameterType.IsValueType)
                il.Emit(OpCodes.Box, parameter.ParameterType);

            il.Emit(OpCodes.Callvirt, addMethod);
        }
    }
}
