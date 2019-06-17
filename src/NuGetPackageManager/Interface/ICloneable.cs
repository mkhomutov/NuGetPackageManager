namespace NuGetPackageManager
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}
