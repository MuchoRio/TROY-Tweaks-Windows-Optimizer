using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NRTX.Optimizer.Gui.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strVal && parameter is string paramStr)
        {
            return strVal.Equals(paramStr, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NavActiveBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentNav && parameter is string targetNav)
        {
            if (currentNav.Equals(targetNav, StringComparison.OrdinalIgnoreCase))
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1)
                };
                brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0284c7"), 0));
                brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2563eb"), 1));
                return brush;
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NavActiveForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentNav && parameter is string targetNav)
        {
            if (currentNav.Equals(targetNav, StringComparison.OrdinalIgnoreCase))
            {
                return Brushes.White;
            }
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94a3b8"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NavActiveFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentNav && parameter is string targetNav)
        {
            if (currentNav.Equals(targetNav, StringComparison.OrdinalIgnoreCase))
            {
                return FontWeights.Bold;
            }
        }
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PrivilegeForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAdmin && isAdmin)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f43f5e"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return false;
    }
}

public class HealthScoreForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int score)
        {
            if (score >= 80) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
            if (score >= 40) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f59e0b"));
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f43f5e"));
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f43f5e"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
