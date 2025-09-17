/*
 * Code is copied from https://github.com/zyanfx/SafeDeserializationHelpers
 * Many thanks to yallie for this great extensions to make BinaryFormatter a lot safer.
 */

using System.Diagnostics.CodeAnalysis;

namespace DotNettyCoreRemoting.Serialization.Binary
{
    using System;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Deserialization surrogate for the DataSet class.
    /// </summary>
    internal class DataSetSurrogate : ISerializationSurrogate
    {
        private static bool IsNetFramework =>
                typeof(object).Assembly.GetName().Name == "mscorlib";

        private static ConstructorInfo Constructor { get; } = typeof(DataSet).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) },
            null);

        /// <inheritdoc cref="ISerializationSurrogate" />
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var ds = obj as DataSet;
            ds.GetObjectData(info, context);
        }

        /// <inheritdoc cref="ISerializationSurrogate" />
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Validate(info, context, selector);

            var fixedInfo = new SerializationInfo(typeof(DataSet), new FormatterConverter());

            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "XmlSchema" && entry.Value is string schema)
                {
                    string fixedSchema = FixSchemaAssembly(schema); // 只替换含 System.Private.CoreLib 的部分
                    fixedInfo.AddValue(entry.Name, fixedSchema, entry.ObjectType);
                }
                //else if (entry.Name.StartsWith("DataSet.Tables_") && entry.Value is byte[] buffer)
                //{
                //    var fixedBuffer = FixSerializedDataTableSchema(buffer, context, selector);
                //    fixedInfo.AddValue(entry.Name, fixedBuffer, entry.ObjectType);
                //}
                else
                {
                    fixedInfo.AddValue(entry.Name, entry.Value, entry.ObjectType);
                }
            }

            // discard obj
            var ds = Constructor.Invoke(new object[] { fixedInfo, context });
            return ds;
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private void Validate(SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var remotingFormat = SerializationFormat.Xml;

            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Name == "DataSet.RemotingFormat") // DataSet.RemotingFormat does not exist in V1/V1.1 versions
                    remotingFormat = (SerializationFormat)e.Value;
            }

            // XML dataset serialization isn't known to be vulnerable
            if (remotingFormat == SerializationFormat.Xml)
            {
                return;
            }

            // binary dataset serialization should be double-checked
            var tableCount = info.GetInt32("DataSet.Tables.Count");
            for (int i = 0; i < tableCount; i++)
            {
                var key = $"DataSet.Tables_{i}";
                var buffer = info.GetValue(key, typeof(byte[])) as byte[];

                // check the serialized data table using a guarded BinaryFormatter
                // 传递当前的selector参数，确保嵌套的Guid也能被处理
                var fmt =
                    new BinaryFormatter(
                        selector: selector, // 传递当前的selector
                        context: new StreamingContext(context.State, false))
                    .Safe();

                using var ms = new MemoryStream(buffer);

                var dt = fmt.Deserialize(ms);

                if (dt is DataTable)
                {
                    continue;
                }

                // the deserialized data doesn't appear to be a data table
                throw new UnsafeDeserializationException("Serialized DataSet probably includes malicious data.");
            }
        }


        private string FixSchemaAssembly(string schema)
        {
            if (string.IsNullOrEmpty(schema))
                return schema;

            bool isFramework = IsNetFramework;

            // ✅ 用于正则匹配（转义后的字符串）
            string sourceAssemblyRegex = isFramework ? "System\\.Private\\.CoreLib" : "mscorlib";

            // ✅ 如果不包含目标程序集，直接返回
            if (schema.IndexOf(isFramework ? "System.Private.CoreLib" : "mscorlib", StringComparison.OrdinalIgnoreCase) < 0)
                return schema;

            string targetAssembly = isFramework ? "mscorlib" : "System.Private.CoreLib";
            string targetPublicKeyToken = isFramework ? "b77a5c561934e089" : "7cec85d7bea7798e";

            return Regex.Replace(schema,
                $@"msdata:DataType=""System\.(\w+),\s*{sourceAssemblyRegex}[^""]*""",
                match =>
                {
                    string typeName = match.Groups[1].Value;
                    return $"msdata:DataType=\"System.{typeName}, {targetAssembly}, Version=4.0.0.0, Culture=neutral, PublicKeyToken={targetPublicKeyToken}\"";
                },
                RegexOptions.IgnoreCase);
        }

    }
}