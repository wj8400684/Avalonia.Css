using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Avalonia.Logging;
using Nlnet.Avalonia.Css.Behaviors;
using Nlnet.Avalonia.SampleAssistant;
using Nlnet.Avalonia.Svg.Controls;
using Nlnet.Sharp.Avalonia;
using Nlnet.Sharp.Utils;

namespace Nlnet.Avalonia.Css.App
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                typeof(App).WriteError(e.ToString());
                if (e.InnerException != null)
                {
                    typeof(App).WriteError(e.InnerException.ToString());
                }

                Logger.Sink?.Log(LogEventLevel.Error, nameof(Program), null, e.ToString());
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI()
                .LogToLocalFile()

                // Use avalonia css stuff.
                .UseAcssDefaultBuilder()
                // Type resolver for Nlnet.Avalonia.Svg
                .WithTypeResolverForAcssDefaultBuilder(new GenericTypeResolver<Icon>())
                // Type resolver for Nlnet.Avalonia.SampleAssistant
                .WithTypeResolverForAcssDefaultBuilder(new GenericTypeResolver<Case>())
                // Type resolver for Nlnet.Avalonia.Css.App
                .WithTypeResolverForAcssDefaultBuilder(new GenericTypeResolver<App>())

                // Use avalonia behavior.
                .UseAcssBehaviorForDefaultBuilder();
        }
    }
}
