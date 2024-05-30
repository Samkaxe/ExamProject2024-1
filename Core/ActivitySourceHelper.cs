using System.Diagnostics;

namespace Core;

public static class ActivitySourceHelper
{
    public static readonly ActivitySource ActivitySource = new ActivitySource("CheckoutService");
}