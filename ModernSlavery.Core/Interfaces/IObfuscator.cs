namespace ModernSlavery.Core.Interfaces
{
    public interface IObfuscator
    {
        string Obfuscate(string value); 
        string Obfuscate(long value);
        string Obfuscate(int value);
        int DeObfuscate(string value);
    }
}