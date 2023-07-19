﻿using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Styling;

namespace Nlnet.Avalonia.Css
{
    internal static class InternalHelper
    {
        private static PropertyInfo? GetPropertyInfo<T>(string name) where T : class
        {
            return typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static MethodInfo? GetMethodInfo<T>(string name) where T : class
        {
            return typeof(T).GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static PropertyInfo? SelectorTargetTypePropertyInfo { get; } = GetPropertyInfo<Selector>("TargetType");

        private static MethodInfo? StyledElementInvalidateStylesMethodInfo { get; } = GetMethodInfo<StyledElement>("InvalidateStyles");

        private static MethodInfo? StyledElementOnControlThemeChangedMethodInfo { get; } = GetMethodInfo<StyledElement>("OnControlThemeChanged");

        private static MethodInfo? StyledElementOnTemplatedParentControlThemeChangedMethodInfo { get; } = GetMethodInfo<StyledElement>("OnTemplatedParentControlThemeChanged");

        public static Type? GetTargetType(this Selector selector)
        {
            if (SelectorTargetTypePropertyInfo == null)
            {
                throw new NotSupportedException($"Can not find the 'TargetType' property in the type of '{nameof(Selector)}'.");
            }

            var targetType = SelectorTargetTypePropertyInfo.GetValue(selector) as Type;
            return targetType;
        }

        public static void InvalidStyles(this StyledElement element)
        {
            // TODO Trace

            StyledElementInvalidateStylesMethodInfo?.Invoke(element, new object[] { true });
        }

        public static void OnControlThemeChanged(this StyledElement element)
        {
            // TODO Trace

            StyledElementOnControlThemeChangedMethodInfo?.Invoke(element, null);
        }

        public static void OnTemplatedParentControlThemeChanged(this StyledElement element)
        {
            // TODO Trace

            StyledElementOnTemplatedParentControlThemeChangedMethodInfo?.Invoke(element, null);
        }
    }
}
