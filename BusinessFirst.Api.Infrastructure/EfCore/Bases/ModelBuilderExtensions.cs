using RenameMe.Api.Primary.Entities.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.Reflection.Emit;

namespace RenameMe.Api.Infrastructure.EfCore.Bases
{
    public static class ModelBuilderExtensions
    {
        public static void LoadFromEntityConfigure(this ModelBuilder modelBuilder)
        {
            var mappingAssembly = CreateConfigureEntityAssembly();
            if (mappingAssembly != null)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(mappingAssembly);
            }
        }
        private static Assembly? CreateConfigureEntityAssembly()
        {
            var idbEntityType = typeof(IEfDbEntity<>);
            var idbEntityAssembly = idbEntityType.Assembly;
            var dbEntityTypes = idbEntityAssembly
                ?.ExportedTypes
                .Where(e => e.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == idbEntityType) && e.IsClass && !e.IsAbstract)
                .ToArray();
            if (dbEntityTypes != null && dbEntityTypes.Any())
            {
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("EfCoreDbEntityMappingAssembly"), AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("EfCoreEntityMappingModule");
                var ientityTypeConfigurationType = typeof(IEntityTypeConfiguration<>);
                var entityTypeBuilderType = typeof(EntityTypeBuilder<>);
                Type? createdType = null;
                foreach (var dbEntityType in dbEntityTypes)
                {
                    var dbContextProp = dbEntityType.GetMethod(nameof(IEfDbEntity<IEntityPrimary>.EntityConfigure))!;
                    var typeBuilder = moduleBuilder.DefineType($"{dbEntityType.Name}EfCoreEntityMapping", TypeAttributes.Public | TypeAttributes.Class);
                    var ientityTypeConfigurationGenericType = ientityTypeConfigurationType.MakeGenericType(dbEntityType);
                    typeBuilder.AddInterfaceImplementation(ientityTypeConfigurationGenericType);
                    var methodBuilder = typeBuilder.DefineMethod("Configure", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new Type[] { entityTypeBuilderType.MakeGenericType(dbEntityType) });
                    methodBuilder.DefineParameter(1, ParameterAttributes.None, "builder");
                    var il = methodBuilder.GetILGenerator();
                    var methodinfo = dbEntityType.GetMethod("EntityConfigure", BindingFlags.Static | BindingFlags.Public)!;
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, methodinfo);
                    il.Emit(OpCodes.Ret);
                    createdType = typeBuilder.CreateType();
                }
                return createdType?.Assembly;
            }
            return null;
        }
    }
}
