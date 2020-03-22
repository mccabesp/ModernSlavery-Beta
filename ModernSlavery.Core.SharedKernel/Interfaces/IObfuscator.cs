namespace ModernSlavery.Core.SharedKernel.Interfaces
{
    public interface IObfuscator
    {
        string Obfuscate(string value);
        string Obfuscate(int value);
        int DeObfuscate(string value);
    }
}