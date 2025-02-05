﻿using Avalonia.Controls;
using Avalonia.Media;

namespace Nlnet.Avalonia.Css;

/// <summary>
/// For acss behavior resolving.
/// </summary>
public interface IBehaviorResolverManager : IResolverManager<IBehaviorResolver>
{

}

/// <summary>
/// The default implementation for <see cref="IBehaviorResolverManager"/>.
/// </summary>
internal class BehaviorResolverManager : ResolverManager<IBehaviorResolver>, IBehaviorResolverManager
{
    
}