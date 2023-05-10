﻿using System;
using Avalonia.Media;

namespace Nlnet.Avalonia.Css;

public class ColorResource : CssResource<ColorResource>
{
    protected override object? Accept(string valueString)
    {
        var values = valueString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length == 0)
        {
            return null;
        }

        var colorString = values[0];
        var color       = TryParseColor(colorString);

        return color;
    }

    private static Color? TryParseColor(string colorString)
    {
        try
        {
            return Color.Parse(colorString);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}