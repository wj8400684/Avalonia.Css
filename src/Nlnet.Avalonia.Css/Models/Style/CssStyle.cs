﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Styling;

namespace Nlnet.Avalonia.Css;

internal interface ICssStyle : ICssSection
{
    public bool IsThemeChild { get; }

    public bool IsLogicalChild { get; }

    public Type? ThemeTargetType { get; }

    public IEnumerable<ICssSetter>? Setters { get; set; }

    public IEnumerable<ICssStyle>? Styles { get; set; }

    public IEnumerable<ICssResourceDictionary>? Resources { get; set; }

    public IEnumerable<ICssAnimation>? Animations { get; set; }

    public Selector? GetSelector();

    public ChildStyle ToAvaloniaStyle();
}

internal class CssStyle : CssSection, ICssStyle
{
    private readonly ICssBuilder _builder;
    private          Selector?   _selector;

    public bool IsThemeChild { get; set; }

    public bool IsLogicalChild { get; set; }

    public Type? ThemeTargetType { get; set; }

    public IEnumerable<ICssSetter>? Setters { get; set; }

    public IEnumerable<ICssStyle>? Styles { get; set; }

    public IEnumerable<ICssResourceDictionary>? Resources { get; set; }

    public IEnumerable<ICssAnimation>? Animations { get; set; }

    public CssStyle(ICssBuilder builder, string selector) : base(builder, selector)
    {
        _builder = builder;
    }

    public override void InitialSection(ICssParser parser, ReadOnlySpan<char> content)
    {
        _selector = CreateSelector();

        parser.ParseSettersAndChildren(content, out var settersSpan, out var childrenSpan);

        Setters = parser.ParsePairs(settersSpan).Select(p => new CssSetter(p.Item1, p.Item2));
        var list = parser.ParseSections(this, childrenSpan).ToList();
        if (list.Count > 0)
        {
            Children   = list;
            Styles     = list.OfType<ICssStyle>();
            Resources  = list.OfType<ICssResourceDictionary>();
            Animations = list.OfType<ICssAnimation>();
        }
    }

    private Selector? CreateSelector()
    {
        var isChild = (Parent != null && IsLogicalChild == false) || IsThemeChild;

        // Selector
        var selector   = isChild ? Selectors.Nesting(null) : null;
        var syntaxList = SelectorGrammar.Parse(Selector).ToList();
        var selectors  = new List<Selector>();

        if(IsThemeChild)
        {
            ThemeTargetType = syntaxList.First().ToSelector(_builder, null)?.TargetType;
            syntaxList = syntaxList.Skip(1).ToList();
        }

        foreach (var syntax in syntaxList)
        {
            if (syntax is CommaSyntax)
            {
                if (selector != null)
                {
                    selectors.Add(selector);
                }
                selector = isChild ? Selectors.Nesting(null) : null;
            }
            else
            {
                selector = syntax.ToSelector(_builder, selector);
            }
        }
        if (selector != null)
        {
            selectors.Add(selector);
        }
        
        return selectors.Count > 1 ? Selectors.Or(selectors) : selector;
    }

    public Selector? GetSelector()
    {
        return _selector;
    }

    public ChildStyle ToAvaloniaStyle()
    {
        this.WriteLine($"==== Begin parsing style with raw selector of '{Selector}'.");

        var style = GetNewStyle();

        var targetType = style.Selector?.TargetType ?? ThemeTargetType ?? (Parent as ICssStyle)?.ThemeTargetType;
        if (targetType != null)
        {
            // Resources
            if (Resources != null)
            {
                foreach (var cssResourceList in Resources)
                {
                    var dic = cssResourceList.ToAvaloniaResourceDictionary(_builder);
                    if (dic != null)
                    {
                        style.Resources.MergedDictionaries.Add((dic));
                    }
                }
            }

            // Setters
            if (Setters != null)
            {
                foreach (var setter in Setters.Select(s => s.ToAvaloniaSetter(_builder, targetType)).OfType<ISetter>())
                {
                    style.Add(setter);
                }
            }

            // Children Styles
            if (Styles != null)
            {
                foreach (var cssStyle in Styles)
                {
                    if (cssStyle.IsLogicalChild)
                    {
                        var setter = new Setter()
                        {
                            Property = ExStyler.AddingStyleProperty,
                            Value    = cssStyle,
                        };
                        style.Add(setter);
                    }
                    else
                    {
                        var childStyle = cssStyle.ToAvaloniaStyle();
                        style.Add(childStyle);
                    }
                }
            }

            // Style Animations
            if (Animations != null)
            {
                foreach (var cssAnimation in Animations)
                {
                    var animation = cssAnimation.ToAvaloniaAnimation();
                    if (animation != null)
                    {
                        style.Animations.Add(animation);
                    }
                }
            }
        }

        return style;
    }

    private ChildStyle GetNewStyle()
    {
        return IsLogicalChild
            ? new LogicChildStyle(this)
            {
                Selector = _selector
            }
            : new ChildStyle(this)
            {
                Selector = _selector
            };
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"'{Selector}'");
        if (Setters != null)
        {
            foreach (var cssSetter in Setters)
            {
                builder.AppendLine($"    {cssSetter.ToString()}");
            }
        }
        return builder.ToString();
    }
}
