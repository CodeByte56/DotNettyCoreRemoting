using DotNettyCoreRemoting.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNettyCoreRemoting.Serialization.Binary
{
    /// <summary>
    /// Deserialization surrogate for the DataTable class.
    /// Handles cross-platform (Framework/Core) serialization compatibility.
    /// </summary>
    internal class DataTableSurrogate : ISerializationSurrogate
    {
        private static bool IsNetFramework =>
        typeof(object).Assembly.GetName().Name == "mscorlib";

        private static ConstructorInfo Constructor { get; } = typeof(DataTable).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) },
            null);

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var dt = obj as DataTable;
            if (dt == null)
                throw new ArgumentException("Object is not a DataTable.", nameof(obj));

            dt.GetObjectData(info, context);
        }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Validate(info, context, selector);

            var fixedInfo = new SerializationInfo(typeof(DataTable), new FormatterConverter());

            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "XmlSchema" && entry.Value is string schema)
                {
                    string fixedSchema = FixSchemaAssembly(schema);
                    fixedInfo.AddValue(entry.Name, fixedSchema, entry.ObjectType);
                }
                else
                {
                    fixedInfo.AddValue(entry.Name, entry.Value, entry.ObjectType);
                }
            }

            // discard original obj, create new DataTable from fixed info
            var dt = Constructor.Invoke(new object[] { fixedInfo, context });
            return dt;
        }

        private void Validate(SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            // DataTable doesn't have RemotingFormat like DataSet, so we assume binary if schema is absent or minimal
            // 可选：你可以根据实际需求添加更复杂的验证逻辑，比如检查列定义是否合法等
            // 这里简单通过是否存在 XmlSchema 来判断是否为 XML 格式 —— XML 通常较安全

            var e = info.GetEnumerator();
            bool hasXmlSchema = false;
            while (e.MoveNext())
            {
                if (e.Name == "XmlSchema" && e.Value is string schema && !string.IsNullOrEmpty(schema))
                {
                    hasXmlSchema = true;
                    break;
                }
            }

            // 如果是 XML 格式，认为较安全
            if (hasXmlSchema)
                return;

            // 如果是二进制格式，可选：用 Safe BinaryFormatter 进一步验证（如果需要）
            // 注意：DataTable 的二进制序列化数据结构较复杂，通常不建议手动解析
            // 所以这里我们只做轻量级验证或跳过，除非你有明确的安全威胁模型

            // 示例：你可以检查是否存在恶意类型（如 ObjectStateFormatter 等）
            // 或者记录日志提醒使用了二进制格式
            Logger.Error(typeof(DataTableSurrogate), "DataTable deserialized in binary format. Consider using Xml format for safety.");
        }

        private string FixSchemaAssembly(string schema)
        {
            if (string.IsNullOrEmpty(schema))
                return schema;

            bool isFramework = IsNetFramework;

            string sourceAssemblyRegex = isFramework ? "System\\.Private\\.CoreLib" : "mscorlib";

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
