using Cryptography.Obfuscation;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Core.Classes
{
    public class InternalObfuscator : IObfuscator
    {
        private readonly Obfuscator _obfuscator;

        public InternalObfuscator(int seed)
        {
            _obfuscator = new Obfuscator {Seed = seed};
        }

        public string Obfuscate(string value)
        {
            return Obfuscate(value.ToInt32());
        }

        public string Obfuscate(int value)
        {
            return _obfuscator.Obfuscate(value); // e.g. xVrAndNb
        }

        public int DeObfuscate(string value)
        {
            return _obfuscator.Deobfuscate(value); // 15
        }
    }
}