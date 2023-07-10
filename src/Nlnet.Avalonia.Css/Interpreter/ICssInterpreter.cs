﻿using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Data;
using Avalonia.Styling;

namespace Nlnet.Avalonia.Css;

internal interface ICssInterpreter
{
    public Selector? ToSelector(IEnumerable<ISyntax> syntaxList);

    public AvaloniaProperty? ParseAvaloniaProperty(Type avaloniaObjectType, string property);

    public object? ParseValue(Type declaredType, string? rawValue);

    public object? ParseValue(AvaloniaProperty avaloniaProperty, string? rawValue);

    public bool IsVar(string? valueString, out string? varKey);

    public bool IsBinding(string? valueString, out Binding? binding);

    public ITransition? ParseTransition(string valueString);

    public IEnumerable<KeyFrame>? ParseKeyFrames(Type selectorTargetType, string valueString);
}
