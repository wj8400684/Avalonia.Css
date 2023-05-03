﻿using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Nlnet.Avalonia.Css.Fluent
{
    public partial class CssFluentTheme : Styles
    {
        public CssFluentTheme()
        {
            AvaloniaXamlLoader.Load(this);

            CssStyles.Load("../../../Nlnet.Avalonia.Css.Fluent/Css/button.css", true);
        }
    }
}
