﻿using Cryptography.Obfuscation;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;

namespace ModernSlavery.Core.Classes
{
    public interface IObfuscator
    {

        string Obfuscate(string value);
        string Obfuscate(int value);
        int DeObfuscate(string value);

    }

    public class InternalObfuscator : IObfuscator
    {

        private static readonly int Seed = Config.GetAppSetting("ObfuscationSeed").ToInt32(127);
        private readonly Obfuscator _obfuscator;

        public InternalObfuscator()
        {
            _obfuscator = new Obfuscator {Seed = Seed};
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