using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Commander.Extensions;

public static class DependencyObjectExtensions
{
    #region AttachedProperties

    public static readonly DependencyProperty IsStoryboardRunningProperty = DependencyProperty.RegisterAttached("IsStoryboardRunning",
        typeof(bool), typeof(DependencyObjectExtensions), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));
    public static bool GetIsStoryboardRunning(DependencyObject dO)
        => (bool)dO.GetValue(IsStoryboardRunningProperty);

    public static void SetIsStoryboardRunning(DependencyObject dO, bool value)
        => dO.SetValue(IsStoryboardRunningProperty, value);

    #endregion

    #region Extension Methods

    public static AncestorType FindAncestorOrSelf<AncestorType>(this DependencyObject element)
        where AncestorType : DependencyObject
    {
        while (element != null)
        {
            if (element is AncestorType)
                break;

            if (element is Visual)
                element = VisualTreeHelper.GetParent(element);
            else
                // If we're in Logical Land then we must walk 
                // up the logical tree until we find a 
                // Visual/Visual3D to get us back to Visual Land.
                element = LogicalTreeHelper.GetParent(element);
        }
        return (element as AncestorType)!;
    }

    public static AncestorType FindAncestor<AncestorType>(this DependencyObject element)
        where AncestorType : DependencyObject
    {
        while (element != null)
        {
            if (element is Visual)
                element = VisualTreeHelper.GetParent(element);
            else
                // If we're in Logical Land then we must walk 
                // up the logical tree until we find a 
                // Visual/Visual3D to get us back to Visual Land.
                element = LogicalTreeHelper.GetParent(element);

            if (element is AncestorType)
                break;
        }
        return (element as AncestorType)!;
    }

    /// <summary>
    /// Ein Kindelement von Element ermitteln, welches dem Typ "childItem" entspricht
    /// </summary>
    /// <typeparam name="childItem">Typ des gesuchten Kindelementes</typeparam>
    /// <param name="element">Element, dessen Kindelement gesucht werden soll</param>
    /// <returns>Das gewünschte Kindelement, oder null, falls nicht gefunden</returns>
    public static childItem FindVisualChild<childItem>(this DependencyObject element)
        where childItem : DependencyObject
        => (childItem)element.FindVisualChild(typeof(childItem));

    /// <summary>
    /// Ein Kindelement von Element ermitteln, welches dem Typ "childType" entspricht
    /// </summary>
    /// <param name="element">Element, dessen Kindelement gesucht werden soll</param>
    /// <param name="childType">Der Type des Kindelements</param>
    /// <returns></returns>
    public static DependencyObject? FindVisualChild(this DependencyObject element, Type childType)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            if (child != null && child.GetType() == childType)
                return child;
            else if (child != null)
            {
                var childOfChild = FindVisualChild(child, childType);
                if (childOfChild != null)
                    return childOfChild;
            }
            else
                return null;
        }
        return null;
    }

    /// <summary>
    /// Ein Kindelement von Element ermitteln, welches dem Typ "T" entspricht
    /// </summary>
    /// <typeparam name="T">Typ des gesuchten Kindelementes</typeparam>
    /// <param name="parent">Element, dessen Kindelement gesucht werden soll</param>
    /// <param name="childName">Name des gesuchten Kindelementes</param>
    /// <returns>Das gewünschte Kindelement, oder null, falls nicht gefunden</returns>
    public static T? FindVisualChild<T>(DependencyObject parent, string childName)
        where T : DependencyObject
    {
        if (parent == null) 
            return null;

        T? foundChild = null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            var childType = child as T;
            if (childType == null)
            {
                foundChild = FindVisualChild<T>(child, childName);
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                var frameworkElement = child as FrameworkElement;
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    foundChild = (T)child;
                    break;
                }
            }
            else
            {
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }

    public static Button? FindDefaultButton(this DependencyObject element)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, i);
            if (child != null && child is Button)
            {
                var btn = child as Button;
                if (btn?.IsDefault == true)
                    return btn;
            }
            else if (child != null)
            {
                var childOfChild = FindDefaultButton(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            else
                return null;
        }
        return null;
    }

    /// <summary>
    /// Ermittlung aller DependencyProperties, die in diesem Object definiert sind
    /// </summary>
    /// <param name="dO">Das zu betrachtende DependencyOpject, verwende bitte die Erweiterungssyntax: dependencyObject.GetBindings()</param>
    /// <returns>Aufzählung aller DependencyProperties dieses Objekts</returns>
    public static IEnumerable<DependencyProperty> GetDependencyProperties(this DependencyObject dO)
    {
        PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(dO, [new PropertyFilterAttribute(PropertyFilterOptions.All)]);
        return from n in pdc.Cast<PropertyDescriptor>()
               let dp = DependencyPropertyDescriptor.FromProperty(n)
               where dp != null
               select dp.DependencyProperty;
    }

    #endregion
}
