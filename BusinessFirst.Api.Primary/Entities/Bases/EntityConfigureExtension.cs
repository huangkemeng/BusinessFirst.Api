using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;
using System.Reflection;
using System.Text.Json.Nodes;
using PropertyBuilder = Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder;

namespace RenameMe.Api.Primary.Entities.Bases
{
    public static class EntityConfigureExtension
    {
        public static void AutoConfigure<T>(this EntityTypeBuilder<T> builder) where T : class, IEntityPrimary
        {
            var entityType = typeof(T);
            builder.ToTable(entityType.Name)
                   .HasKey(x => x.Id);
            var entityProperties = GetEntityProperties(entityType);
            var typeMaps = CreateClrTypeToSqlTypeMaps();
            var propMethodInfo = typeof(EntityTypeBuilder<T>).GetMethod(nameof(EntityTypeBuilder<T>.Property), genericParameterCount: 0, new Type[] { typeof(Type), typeof(string) })!;
            var propertyBuilderType = typeof(RelationalPropertyBuilderExtensions);
            var hasColumnNameType = propertyBuilderType.GetMethod(nameof(RelationalPropertyBuilderExtensions.HasColumnName), genericParameterCount: 0, new Type[] { typeof(PropertyBuilder), typeof(string) });
            PropertyBuilder? propBuilder = null;
            foreach (var entityProperty in entityProperties)
            {
                if (typeMaps.ContainsKey(entityProperty.PropertyType))
                {
                    propBuilder = (PropertyBuilder?)propMethodInfo.Invoke(builder, new object[] { entityProperty.PropertyType, entityProperty.Name });
                    if (propBuilder != null)
                    {
                        var dbType = typeMaps[entityProperty.PropertyType];
                        propBuilder = RelationalPropertyBuilderExtensions.HasColumnName(propBuilder, entityProperty.Name);
                        propBuilder = RelationalPropertyBuilderExtensions.HasColumnType(propBuilder, dbType.ToString());
                        if (entityProperty.PropertyType.IsGenericType && entityProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var isRequiredType = typeof(PropertyBuilder).GetMethod(nameof(PropertyBuilder.IsRequired), new Type[] { typeof(bool) });
                            propBuilder = (PropertyBuilder?)isRequiredType?.Invoke(propBuilder, new object[] { false });
                        }
                        if (entityProperty.PropertyType == typeof(string))
                        {
                            var hasMaxLengthType = typeof(PropertyBuilder).GetMethod(nameof(PropertyBuilder.HasMaxLength), new Type[] { typeof(int) });
                            propBuilder = (PropertyBuilder?)hasMaxLengthType?.Invoke(propBuilder, new object[] { 100 });
                        }
                    }
                }
            }
        }

        private static PropertyInfo[] GetEntityProperties(Type? entityType)
        {
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            if (entityType != null)
            {
                var entityTypeProperties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);
                propertyInfos.AddRange(entityTypeProperties);
                var baseProperties = GetEntityProperties(entityType.BaseType);
                foreach (var baseProperty in baseProperties)
                {
                    if (!propertyInfos.Contains(baseProperty))
                    {
                        propertyInfos.Add(baseProperty);
                    }
                }
            }
            return propertyInfos.ToArray();
        }

        private static Dictionary<Type, SqlDbType> CreateClrTypeToSqlTypeMaps()
        {
            return new Dictionary<Type, SqlDbType>
            {
                {typeof (bool), SqlDbType.Bit},
                {typeof (bool?), SqlDbType.Bit},
                {typeof (byte), SqlDbType.TinyInt},
                {typeof (byte?), SqlDbType.TinyInt},
                {typeof (string), SqlDbType.NVarChar},
                {typeof (DateTime), SqlDbType.DateTime},
                {typeof (DateTime?), SqlDbType.DateTime},
                {typeof (short), SqlDbType.SmallInt},
                {typeof (short?), SqlDbType.SmallInt},
                {typeof (int), SqlDbType.Int},
                {typeof (int?), SqlDbType.Int},
                {typeof (long), SqlDbType.BigInt},
                {typeof (long?), SqlDbType.BigInt},
                {typeof (decimal), SqlDbType.Decimal},
                {typeof (decimal?), SqlDbType.Decimal},
                {typeof (double), SqlDbType.Float},
                {typeof (double?), SqlDbType.Float},
                {typeof (float), SqlDbType.Real},
                {typeof (float?), SqlDbType.Real},
                {typeof (TimeSpan), SqlDbType.Time},
                {typeof (Guid), SqlDbType.UniqueIdentifier},
                {typeof (Guid?), SqlDbType.UniqueIdentifier},
                {typeof (byte[]), SqlDbType.Binary},
                {typeof (byte?[]), SqlDbType.Binary},
                {typeof (char[]), SqlDbType.Char},
                {typeof (char?[]), SqlDbType.Char},
                {typeof (JsonObject), SqlDbType.NVarChar},
                {typeof (DateTimeOffset), SqlDbType.DateTimeOffset},
                {typeof (DateTimeOffset?), SqlDbType.DateTimeOffset},
            };
        }
    }
}
