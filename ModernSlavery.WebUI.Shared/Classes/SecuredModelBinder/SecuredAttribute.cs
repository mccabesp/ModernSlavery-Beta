using System;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SecuredAttribute : Attribute
    {
        public enum SecureMethods
        {
            Encrypt = 1,
            Obfuscate = 2
        }

        public readonly SecureMethods SecureMethod;
        public SecuredAttribute(SecureMethods secureMethod = SecureMethods.Encrypt)
        {
            SecureMethod = secureMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ObfuscatedAttribute : SecuredAttribute
    {
        public ObfuscatedAttribute():base(SecureMethods.Obfuscate)
        {
            
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EncryptedAttribute : SecuredAttribute
    {
        public EncryptedAttribute() : base(SecureMethods.Encrypt)
        {

        }
    }
}
