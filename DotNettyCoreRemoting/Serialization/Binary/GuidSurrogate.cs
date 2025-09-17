/*
 * Code is copied from https://github.com/zyanfx/SafeDeserializationHelpers
 * Many thanks to yallie for this great extensions to make BinaryFormatter a lot safer.
 */

using System.Diagnostics.CodeAnalysis;

namespace DotNettyCoreRemoting.Serialization.Binary
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Surrogate for System.Guid type to handle cross-framework serialization compatibility.
    /// </summary>
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    internal sealed class GuidSurrogate : ISerializationSurrogate
    {
        private static ConstructorInfo Constructor { get; } = typeof(Guid).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) },
            null);

        /// <inheritdoc cref="ISerializationSurrogate" />
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj is not Guid guid) return;
            
            // 存储GUID的字符串表示，这样可以在不同框架间兼容
            info.AddValue("GuidString", guid.ToString());
        }

        /// <inheritdoc cref="ISerializationSurrogate" />
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Validate(info, context);

            // 如果Guid类型有接受SerializationInfo和StreamingContext的构造函数，则使用它
            if (Constructor != null)
            {
                try
                {
                    return Constructor.Invoke(new object[] { info, context });
                }
                catch (TargetInvocationException)
                {
                    // 如果构造函数调用失败，则回退到字符串解析方式
                }
            }

            // 回退方案：从字符串重建Guid对象
            var guidString = info.GetString("GuidString");
            return new Guid(guidString);
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void Validate(SerializationInfo info, StreamingContext context)
        {
            // 验证是否包含必需的GuidString字段
            // 使用GetEnumerator方法来兼容旧版本的.NET
            var enumerator = info.GetEnumerator();
            bool foundGuidString = false;
            
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "GuidString")
                {
                    foundGuidString = true;
                    break;
                }
            }
            
            if (!foundGuidString)
            {
                throw new SerializationException("Missing required field 'GuidString' for Guid deserialization.");
            }
        }
    }
}