using System;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SecuredAttribute : Attribute
    {
        public enum SecureMethods
        {
            Encrypt,
            Obfuscate
        }

        public readonly SecureMethods SecureMethod;
        public SecuredAttribute(SecureMethods secureMethod = SecureMethods.Encrypt)
        {
            SecureMethod = secureMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EncryptedAttribute : SecuredAttribute
    {
        public EncryptedAttribute() : base(SecureMethods.Encrypt) { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ObfuscatedAttribute : SecuredAttribute
    {
        public ObfuscatedAttribute() : base(SecureMethods.Obfuscate) { }
    }
}
