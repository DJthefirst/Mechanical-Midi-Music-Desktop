using AvaloniaGUI.Data;
using AvaloniaGUI.ViewModels;
using System;

namespace AvaloniaGUI.Factories;

public class ComponentFactory(Func<Type, ComponentViewModel> factory)
{
    public ComponentViewModel GetComponentViewModel<T>(Action<T>? afterCreation = null)
        where T : ComponentViewModel
	{
        var viewModel = factory(typeof(T));

        afterCreation?.Invoke((T)viewModel);

        return viewModel;
    }
}