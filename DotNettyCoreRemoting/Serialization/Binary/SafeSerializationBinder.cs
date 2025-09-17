/*
 * Code is copied from https://github.com/zyanfx/SafeDeserializationHelpers
 * Many thanks to yallie for this great extensions to make BinaryFormatter a lot safer.
 */

namespace DotNettyCoreRemoting.Serialization.Binary
{
    using System;
    using System.Runtime.Serialization;

    /// <inheritdoc cref="SerializationBinder" />
    internal sealed class SafeSerializationBinder : SerializationBinder
    {
        /// <summary>
        /// Core library assembly name.
        /// </summary>
        public const string CORE_LIBRARY_ASSEMBLY_NAME = "mscorlib";

        /// <summary>
        /// System.Private.CoreLib assembly name (used in .NET Core/.NET 5+).
        /// </summary>
        public const string PRIVATE_CORE_LIB_ASSEMBLY_NAME = "System.Private.CoreLib";

        /// <summary>
        /// System.DelegateSerializationHolder type name.
        /// </summary>
        public const string DELEGATE_SERIALIZATION_HOLDER_TYPE_NAME = "System.DelegateSerializationHolder";

        /// <summary>
        /// System.Guid type name.
        /// </summary>
        public const string GUID_TYPE_NAME = "System.Guid";

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeSerializationBinder" /> class.
        /// </summary>
        /// <param name="nextBinder">Next serialization binder in chain.</param>
        public SafeSerializationBinder(SerializationBinder nextBinder = null)
        {
            NextBinder = nextBinder;
        }

        private SerializationBinder NextBinder { get; }

        /// <inheritdoc cref="SerializationBinder" />
        public override Type BindToType(string assemblyName, string typeName)
        {
            // 处理GUID类型的程序集兼容性问题
            //if (typeName == GUID_TYPE_NAME && 
            //    assemblyName.StartsWith(PRIVATE_CORE_LIB_ASSEMBLY_NAME, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    // 直接返回当前环境中的GUID类型，不考虑程序集版本
            //    return typeof(Guid);
            //}

            // 处理 Guid 类型的跨平台程序集兼容性（双向）
            if (typeName == GUID_TYPE_NAME)
            {
                if (assemblyName.StartsWith(PRIVATE_CORE_LIB_ASSEMBLY_NAME, StringComparison.InvariantCultureIgnoreCase) ||
                    assemblyName.StartsWith(CORE_LIBRARY_ASSEMBLY_NAME, StringComparison.InvariantCultureIgnoreCase))
                {
                    return typeof(Guid);
                }
            }

            // prevent delegate deserialization attack
            if (typeName == DELEGATE_SERIALIZATION_HOLDER_TYPE_NAME &&
                assemblyName.StartsWith(CORE_LIBRARY_ASSEMBLY_NAME, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(CustomDelegateSerializationHolder);
            }

            // suppress known blacklisted types
            TypeNameValidator.Default.ValidateTypeName(assemblyName, typeName);

            // chain to the original type binder if exists
            return NextBinder?.BindToType(assemblyName, typeName);
        }
    }
}