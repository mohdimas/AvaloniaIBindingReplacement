using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace IBindingReplacement.Extensions;

public sealed class BindResourceExtension : MarkupExtension
{
    [AssignBinding]
    public IBinding Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var providerTarget = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
        var targetObject = providerTarget!.IntermediateRootObject as AvaloniaObject;

        return Initiate(targetObject);
    }

    private static object? ConvertBindingValue(object keyBinding)
    {
        if (keyBinding is null or BindingNotification)
            return null;

        return Application.Current!.TryFindResource(keyBinding, out var resource)
            ? resource
            : null;
    }

    private IBinding Initiate(AvaloniaObject target)
    {
        var dataContext = target
            .GetPropertyChangedObservable(StyledElement.DataContextProperty)
            .AsObservable();

#pragma warning disable CS0618 // Type or member is obsolete
        var keyBinding = Key.Initiate(target, null)!.Source;
#pragma warning restore CS0618 // Type or member is obsolete

        var input = new[] { keyBinding, dataContext }
            .CombineLatest()
            .Select(x => ConvertBindingValue(x.First()))
            .Where(x => x != BindingOperations.DoNothing);

        return input.ToBinding();
    }
}