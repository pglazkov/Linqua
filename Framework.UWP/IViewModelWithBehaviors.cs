using System.Windows.Input;
using JetBrains.Annotations;

namespace Framework
{
    public interface IViewModelWithBehaviors : IViewModel
    {
        void AddBehavior([NotNull] string key, [NotNull] IViewModelBahavior behavior);

        void RegisterAttachedCommand([NotNull] string commandKey, [NotNull] ICommand command);
    }
}