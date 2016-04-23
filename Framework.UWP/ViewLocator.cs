using System;
using System.Composition.Convention;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Framework.Resources;

namespace Framework
{
    /// <summary>
    ///     A strategy for determining which view to use for a given model.
    /// </summary>
    public static class ViewLocator
    {
        public static ICompositionManager CompositionManager
        {
            get { return compositionManager ?? Framework.CompositionManager.Current; }
            set { compositionManager = value; }
        }

        public static UIElement Locate(object viewModel)
        {
            return LocateForModel(viewModel, null, null);
        }

        /// <summary>
        ///     Locates the view for the specified model type.
        /// </summary>
        /// <returns>The view.</returns>
        /// <remarks>Pass the model type, display location (or null) and the context instance (or null) as parameters and receive a view instance.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible",
            Justification = "Intended to be overriden.")]
        public static FrameworkElement LocateForModelType(Type modelType, DependencyObject displayLocation,
            object context)
        {
            int nameEndIndex = modelType.FullName.IndexOf("`", StringComparison.Ordinal) < 0
                                   ? modelType.FullName.Length
                                   : modelType.FullName.IndexOf("`", StringComparison.Ordinal);

            string viewExportName = modelType.FullName.Substring(0, nameEndIndex).Replace("Model", string.Empty);

            if (context != null)
            {
                viewExportName = viewExportName.Remove(viewExportName.Length - 4, 4);
                viewExportName = viewExportName + "." + context;
            }

            var view = CompositionManager.GetInstance<FrameworkElement>(viewExportName);

            return view ?? new TextBlock
            {
                Text = string.Format(LocalizedStrings.CannotFindViewPlaceholderTemplate, modelType)
            };
        }

        /// <summary>
        ///     Locates the view for the specified model instance.
        /// </summary>
        /// <returns>The view.</returns>
        /// <remarks>Pass the model instance, display location (or null) and the context (or null) as parameters and receive a view instance.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible",
            Justification = "Intended to be overriden.")]
        public static FrameworkElement LocateForModel(object model, DependencyObject displayLocation, object context)
        {
            // If the model is null the view for this model is also null
            if (model == null)
            {
                return null;
            }

            return LocateForModelType(model.GetType(), displayLocation, context);
        }

        public static void BuildMefConventions(ConventionBuilder conventionBuilder)
        {
            Contract.Requires(conventionBuilder != null);

            // Export views as FrameworkElement with contract name as the type full name, so the ViewLocator can find it.
            conventionBuilder.ForTypesMatching(t => t.Name.EndsWith("View"))
                             .Export<FrameworkElement>(
                                 builder =>
                                 builder.AsContractName(t => t.FullName));
        }

        #region Model Attached Property

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached("Model",
                                                typeof(object),
                                                typeof(ViewLocator),
                                                new PropertyMetadata(null, OnModelChanged));

        private static ICompositionManager compositionManager;

        public static object GetModel(DependencyObject obj)
        {
            return obj.GetValue(ModelProperty);
        }

        public static void SetModel(DependencyObject obj, object value)
        {
            obj.SetValue(ModelProperty, value);
        }

        private static void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue == args.NewValue)
            {
                return;
            }

            if (args.NewValue != null)
            {
                UIElement view = LocateForModel(args.NewValue, targetLocation, null);

                SetDataContext(view, args.NewValue);
                SetContentProperty(targetLocation, view);
            }
            else
            {
                SetContentProperty(targetLocation, args.NewValue);
            }
        }

        private static void SetDataContext(DependencyObject targetLocation, object dataContext)
        {
            var fe = targetLocation as FrameworkElement;

            if (fe != null)
            {
                fe.DataContext = dataContext;
            }
        }

        private static void SetContentProperty(object targetLocation, object view)
        {
            var fe = view as FrameworkElement;
            if (fe != null && fe.Parent != null)
            {
                SetContentPropertyCore(fe.Parent, null);
            }

            SetContentPropertyCore(targetLocation, view);
        }

        private static void SetContentPropertyCore(object targetLocation, object view)
        {
            Type type = targetLocation.GetType();

            ContentPropertyAttribute contentProperty =
                type.GetTypeInfo().GetCustomAttribute<ContentPropertyAttribute>(true);

            type.GetTypeInfo().GetDeclaredProperty(contentProperty != null ? contentProperty.Name : "Content").SetValue(targetLocation, view, null);
        }

        #endregion
    }
}