using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SwitchyLingus.Core;

public static class VerifyThat
{
    public static void IsNotNull<T>([NotNull] T? value, 
        [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value == null)
            throw new ArgumentNullException(paramName);
    }
    
    public static void IsTrue(bool value, string errorMessage)
    {
        if (!value)
            throw new Exception(errorMessage);
    }
    
    public static void IsNot(bool value, string errorMessage)
    {
        if (value)
            throw new Exception(errorMessage);
    }
}