﻿using System;
using Avalonia.Media;

namespace Nlnet.Avalonia.Css;

[ResourceType(nameof(BoxShadows))]
[ResourceType(nameof(BoxShadow))]
internal class BoxShadowsResource : CssResourceBaseAndFac<BoxShadowsResource>
{
    protected override object? Accept(ICssBuilder cssBuilder, string valueString)
    {
        try
        {
            return BoxShadows.Parse(valueString);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
