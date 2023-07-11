﻿using System;
using System.Collections.Generic;
using Avalonia.Data.Core;
using Avalonia.Utilities;

namespace Nlnet.Avalonia.Css
{
    // 
    // Duplicated from here:
    // Avalonia/src/Markup/Avalonia.Markup/Markup/Parsers/SelectorGrammar.cs
    // 
    // Avalonia version : 11.0.0-preview7
    // 
    // https://github.com/AvaloniaUI/Avalonia/blob/939c94467502e0b63ae13ff6dd00d2947027f0d2/src/Markup/Avalonia.Markup/Markup/Parsers/SelectorGrammar.cs#L371
    // 
    internal static class SelectorGrammar
    {
        private enum State
        {
            Start,
            Middle,
            Colon,
            Class,
            Name,
            CanHaveType,
            Traversal,
            TypeName,
            Property,
            AttachedProperty,
            Template,
            End,
        }

        public static IEnumerable<ISyntax> Parse(string s)
        {
            var r = new CharacterReader(s.AsSpan());
            return Parse(ref r, null);
        }

        private static IEnumerable<ISyntax> Parse(ref CharacterReader r, char? end)
        {
            var state = State.Start;
            var selector = new List<ISyntax>();
            while (!r.End && state != State.End)
            {
                ISyntax? syntax = null;
                switch (state)
                {
                    case State.Start:
                        state = ParseStart(ref r);
                        break;
                    case State.Middle:
                        (state, syntax) = ParseMiddle(ref r, end);
                        break;
                    case State.CanHaveType:
                        state = ParseCanHaveType(ref r);
                        break;
                    case State.Colon:
                        (state, syntax) = ParseColon(ref r);
                        break;
                    case State.Class:
                        (state, syntax) = ParseClass(ref r);
                        break;
                    case State.Traversal:
                        (state, syntax) = ParseTraversal(ref r);
                        break;
                    case State.TypeName:
                        (state, syntax) = ParseTypeName(ref r);
                        break;
                    case State.Property:
                        (state, syntax) = ParseProperty(ref r);
                        break;
                    case State.Template:
                        (state, syntax) = ParseTemplate(ref r);
                        break;
                    case State.Name:
                        (state, syntax) = ParseName(ref r);
                        break;
                    case State.AttachedProperty:
                        (state, syntax) = ParseAttachedProperty(ref r);
                        break;
                }
                if (syntax != null)
                {
                    selector.Add(syntax);
                }
            }

            if (state != State.Start && state != State.Middle && state != State.End && state != State.CanHaveType)
            {
                throw new ExpressionParseException(r.Position, "Unexpected end of selector");
            }

            return selector;
        }

        private static State ParseStart(ref CharacterReader r)
        {
            r.SkipWhitespace();
            if (r.End)
            {
                return State.End;
            }

            if (r.TakeIf(':'))
            {
                return State.Colon;
            }
            else if (r.TakeIf('.'))
            {
                return State.Class;
            }
            else if (r.TakeIf('#'))
            {
                return State.Name;
            }
            //
            // By nlb 2023/7/11: For child style started with property or attached property.
            //
            else if (r.TakeIf('['))
            {
                return State.Property;
            }
            //
            // By nlb 2023/7/11: For child style started with /template/.
            //
            else if (r.TakeIf('/'))
            {
                return State.Template;
            }
            return State.TypeName;
        }

        private static (State, ISyntax?) ParseMiddle(ref CharacterReader r, char? end)
        {
            if (r.TakeIf(':'))
            {
                return (State.Colon, null);
            }
            else if (r.TakeIf('.'))
            {
                return (State.Class, null);
            }
            else if (r.TakeIf(char.IsWhiteSpace) || r.Peek == '>')
            {
                return (State.Traversal, null);
            }
            else if (r.TakeIf('/'))
            {
                return (State.Template, null);
            }
            else if (r.TakeIf('#'))
            {
                return (State.Name, null);
            }
            else if (r.TakeIf(','))
            {
                return (State.Start, new CommaSyntax());
            }
            else if (end.HasValue && !r.End && r.Peek == end.Value)
            {
                return (State.End, null);
            }
            return (State.TypeName, null);
        }

        private static State ParseCanHaveType(ref CharacterReader r)
        {
            if (r.TakeIf('['))
            {
                return State.Property;
            }
            return State.Middle;
        }

        private static (State, ISyntax) ParseColon(ref CharacterReader r)
        {
            var identifier = r.ParseStyleClass();

            if (identifier.IsEmpty)
            {
                throw new ExpressionParseException(r.Position, "Expected class name, is, nth-child or nth-last-child selector after ':'.");
            }

            const string IsKeyword = "is";
            const string NotKeyword = "not";
            const string NthChildKeyword = "nth-child";
            const string NthLastChildKeyword = "nth-last-child";

            if (identifier.SequenceEqual(IsKeyword.AsSpan()) && r.TakeIf('('))
            {
                var syntax = ParseType(ref r, new IsSyntax());
                Expect(ref r, ')');

                return (State.CanHaveType, syntax);
            }
            if (identifier.SequenceEqual(NotKeyword.AsSpan()) && r.TakeIf('('))
            {
                var argument = Parse(ref r, ')');
                Expect(ref r, ')');

                var syntax = new NotSyntax { Argument = argument };
                return (State.Middle, syntax);
            }
            if (identifier.SequenceEqual(NthChildKeyword.AsSpan()) && r.TakeIf('('))
            {
                var (step, offset) = ParseNthChildArguments(ref r);

                var syntax = new NthChildSyntax { Step = step, Offset = offset };
                return (State.Middle, syntax);
            }
            if (identifier.SequenceEqual(NthLastChildKeyword.AsSpan()) && r.TakeIf('('))
            {
                var (step, offset) = ParseNthChildArguments(ref r);

                var syntax = new NthLastChildSyntax { Step = step, Offset = offset };
                return (State.Middle, syntax);
            }
            else
            {
                return (
                    State.CanHaveType,
                    new ClassSyntax
                    {
                        Class = ":" + identifier.ToString()
                    });
            }
        }

        private static (State, ISyntax?) ParseTraversal(ref CharacterReader r)
        {
            r.SkipWhitespace();
            if (r.TakeIf('>'))
            {
                r.SkipWhitespace();
                return (State.Middle, new ChildSyntax());
            }
            else if (r.TakeIf('/'))
            {
                return (State.Template, null);
            }
            else if (!r.End)
            {
                return (State.Middle, new DescendantSyntax());
            }
            else
            {
                return (State.End, null);
            }
        }

        private static (State, ISyntax) ParseClass(ref CharacterReader r)
        {
            var @class = r.ParseStyleClass();
            if (@class.IsEmpty)
            {
                throw new ExpressionParseException(r.Position, $"Expected a class name after '.'.");
            }

            return (State.CanHaveType, new ClassSyntax { Class = @class.ToString() });
        }

        private static (State, ISyntax) ParseTemplate(ref CharacterReader r)
        {
            var template = r.ParseIdentifier();
            const string TemplateKeyword = "template";
            if (!template.SequenceEqual(TemplateKeyword.AsSpan()))
            {
                throw new ExpressionParseException(r.Position, $"Expected 'template', got '{template.ToString()}'");
            }
            else if (!r.TakeIf('/'))
            {
                throw new ExpressionParseException(r.Position, "Expected '/'");
            }
            return (State.Start, new TemplateSyntax());
        }

        private static (State, ISyntax) ParseName(ref CharacterReader r)
        {
            var name = r.ParseIdentifier();
            if (name.IsEmpty)
            {
                throw new ExpressionParseException(r.Position, $"Expected a name after '#'.");
            }
            return (State.CanHaveType, new NameSyntax { Name = name.ToString() });
        }

        private static (State, ISyntax) ParseTypeName(ref CharacterReader r)
        {
            return (State.CanHaveType, ParseType(ref r, new OfTypeSyntax()));
        }

        private static (State, ISyntax?) ParseProperty(ref CharacterReader r)
        {
            var property = r.ParseIdentifier();

            if (r.TakeIf('('))
            {
                return (State.AttachedProperty, default);
            }
            else if (!r.TakeIf('='))
            {
                throw new ExpressionParseException(r.Position, $"Expected '=', got '{r.Peek}'");
            }

            var value = r.TakeUntil(']');

            r.Take();

            return (State.CanHaveType, new PropertySyntax { Property = property.ToString(), Value = value.ToString() });
        }

        private static (State, ISyntax) ParseAttachedProperty(ref CharacterReader r)
        {
            var syntax = ParseType(ref r, new AttachedPropertySyntax());
            if (!r.TakeIf('.'))
            {
                throw new ExpressionParseException(r.Position, $"Expected '.', got '{r.Peek}'");
            }
            var property = r.ParseIdentifier();
            if (property.IsEmpty)
            {
                throw new ExpressionParseException(r.Position, $"Expected Attached Property Name, got '{r.Peek}'");
            }
            syntax.Property = property.ToString();

            if (!r.TakeIf(')'))
            {
                throw new ExpressionParseException(r.Position, $"Expected ')', got '{r.Peek}'");
            }

            if (!r.TakeIf('='))
            {
                throw new ExpressionParseException(r.Position, $"Expected '=', got '{r.Peek}'");
            }

            var value = r.TakeUntil(']');

            syntax.Value = value.ToString();

            r.Take();

            var state = r.End
                ? State.End
                : State.Middle;
            return (state, syntax);
        }

        private static TSyntax ParseType<TSyntax>(ref CharacterReader r, TSyntax syntax)
            where TSyntax : ITypeSyntax
        {
            ReadOnlySpan<char> ns = default;
            ReadOnlySpan<char> type;
            var namespaceOrTypeName = r.ParseIdentifier();

            if (namespaceOrTypeName.IsEmpty)
            {
                throw new ExpressionParseException(r.Position, $"Expected an identifier, got '{r.Peek}");
            }

            if (!r.End && r.TakeIf('|'))
            {
                ns = namespaceOrTypeName;
                if (r.End)
                {
                    throw new ExpressionParseException(r.Position, $"Unexpected end of selector.");
                }
                type = r.ParseIdentifier();
            }
            else
            {
                type = namespaceOrTypeName;
            }

            syntax.Xmlns = ns.ToString();
            syntax.TypeName = type.ToString();
            return syntax;
        }

        private static (int step, int offset) ParseNthChildArguments(ref CharacterReader r)
        {
            int step = 0;
            int offset = 0;

            if (r.Peek == 'o')
            {
                var constArg = r.TakeUntil(')').ToString().Trim();
                if (constArg.Equals("odd", StringComparison.Ordinal))
                {
                    step = 2;
                    offset = 1;
                }
                else
                {
                    throw new ExpressionParseException(r.Position, $"Expected nth-child(odd). Actual '{constArg}'.");
                }
            }
            else if (r.Peek == 'e')
            {
                var constArg = r.TakeUntil(')').ToString().Trim();
                if (constArg.Equals("even", StringComparison.Ordinal))
                {
                    step = 2;
                    offset = 0;
                }
                else
                {
                    throw new ExpressionParseException(r.Position, $"Expected nth-child(even). Actual '{constArg}'.");
                }
            }
            else
            {
                r.SkipWhitespace();

                var stepOrOffset = 0;
                var stepOrOffsetStr = r.TakeWhile(c => char.IsDigit(c) || c == '-' || c == '+').ToString();
                if (stepOrOffsetStr.Length == 0
                    || stepOrOffsetStr.Length == 1
                    && stepOrOffsetStr[0] == '+')
                {
                    stepOrOffset = 1;
                }
                else if (stepOrOffsetStr.Length == 1
                    && stepOrOffsetStr[0] == '-')
                {
                    stepOrOffset = -1;
                }
                else if (!int.TryParse(stepOrOffsetStr.ToString(), out stepOrOffset))
                {
                    throw new ExpressionParseException(r.Position, "Couldn't parse nth-child step or offset value. Integer was expected.");
                }

                r.SkipWhitespace();

                if (r.Peek == ')')
                {
                    step = 0;
                    offset = stepOrOffset;
                }
                else
                {
                    step = stepOrOffset;

                    if (r.Peek != 'n')
                    {
                        throw new ExpressionParseException(r.Position, "Couldn't parse nth-child step value, \"xn+y\" pattern was expected.");
                    }

                    r.Skip(1); // skip 'n'

                    r.SkipWhitespace();

                    if (r.Peek != ')')
                    {
                        int sign;
                        var nextChar = r.Take();
                        if (nextChar == '+')
                        {
                            sign = 1;
                        }
                        else if (nextChar == '-')
                        {
                            sign = -1;
                        }
                        else
                        {
                            throw new ExpressionParseException(r.Position, "Couldn't parse nth-child sign. '+' or '-' was expected.");
                        }

                        r.SkipWhitespace();

                        if (sign != 0
                            && !int.TryParse(r.TakeUntil(')').ToString(), out offset))
                        {
                            throw new ExpressionParseException(r.Position, "Couldn't parse nth-child offset value. Integer was expected.");
                        }

                        offset *= sign;
                    }
                }
            }

            Expect(ref r, ')');

            return (step, offset);
        }

        private static void Expect(ref CharacterReader r, char c)
        {
            if (r.End)
            {
                throw new ExpressionParseException(r.Position, $"Expected '{c}', got end of selector.");
            }
            else if (!r.TakeIf(')'))
            {
                throw new ExpressionParseException(r.Position, $"Expected '{c}', got '{r.Peek}'.");
            }
        }
    }
}