using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Schema
{
	// TableClass中每个属性的基本信息
	internal class FieldSchema
	{
		public string TableName { get; set; }           // Table名称
		public string FieldName { get; set; }           // 字段名称
		public string AliasName { get; set; }			// 列的别名（join表用）
		public bool IsPrimaryKey { get; set; }          // 主键字段
		public bool IsIdentity { get; set; }            // 自增字段
		public bool IsUpdCount { get; set; }            // UpdCount字段
		public int Length { get; set; }                 // 文字型字段的长度
		public PropertyInfo PropertyInfo;				// 字段的属性信息

		public override string ToString()
		{
			return string.Format("{0}.{1}", this.TableName, this.FieldName);
		}
	}

	internal class TableSchema
	{
		// TableClass中所有成员的信息
		public List<FieldSchema> FieldsSchema { get; set; }

		// 表名，或者JOIN时的主表名
		public string PrimaryTable { get; set; }
	}

	internal static class TableSchemaResolver
	{
		// TableSchema info cache
		private static Dictionary<Type, TableSchema> _dicTablesSchema = new Dictionary<Type, TableSchema>();

		public static TableSchema GetTableSchema(Type tableType)
		{
			if (_dicTablesSchema.TryGetValue(tableType, out TableSchema schema))
			{
				// 已缓存
				return schema;
			}

			schema = MappingTableClass(tableType);
			_dicTablesSchema.Add(tableType, schema);

			return schema;
		}

		// 分析TableClass的所有成员，保存在FieldsSchema中
		private static TableSchema MappingTableClass(Type tableClassType)
		{
			TableSchema tableSchema = new TableSchema();

			// Read Primary Table name
			var primaryTableAttribute = tableClassType.GetCustomAttribute<PrimaryTableAttribute>();
			if (primaryTableAttribute == null)
			{
				// 未指定PrimaryTableAttribute时，类名作为主表名
				tableSchema.PrimaryTable = tableClassType.Name;
			}
			else
			{
				tableSchema.PrimaryTable = primaryTableAttribute.PrimaryTableName;
			}

			tableSchema.FieldsSchema = new List<FieldSchema>();

			// add property info
			PropertyInfo[] classPropertyInfo = tableClassType.GetProperties();
			foreach (PropertyInfo info in classPropertyInfo)
			{
				MappingProperty(info, tableSchema.FieldsSchema, tableSchema.PrimaryTable);
			}

			return tableSchema;
		}

		// 分析TableClass的属性
		private static void MappingProperty(PropertyInfo info, List<FieldSchema> fieldsSchema, string primaryTable)
		{
			FieldSchema fieldSchema = new FieldSchema
			{
				TableName = primaryTable,
				FieldName = info.Name,
				PropertyInfo = info
			};

			var columnAttribute = info.GetCustomAttribute<ColumnAttribute>();
			if (columnAttribute != null)
			{
				if (columnAttribute.Table.Length > 0)
				{
					fieldSchema.TableName = columnAttribute.Table;
				}
				if (columnAttribute.Column.Length > 0)
				{
					if (fieldSchema.FieldName.Equals(columnAttribute.Column, StringComparison.OrdinalIgnoreCase) == false)
					{
						fieldSchema.AliasName = fieldSchema.FieldName;
					}
					fieldSchema.FieldName = columnAttribute.Column;
				}
				var ColumnType = columnAttribute.Type;
				if (ColumnType == ColumnType.PrimaryKey)
				{
					fieldSchema.IsPrimaryKey = true;
				}
				if (ColumnType == ColumnType.UpdCount)
				{
					fieldSchema.IsUpdCount = true;
				}
				fieldSchema.Length = columnAttribute.Length;
				fieldSchema.IsIdentity = columnAttribute.Identity;
			}
			fieldsSchema.Add(fieldSchema);
		}
	}
}
