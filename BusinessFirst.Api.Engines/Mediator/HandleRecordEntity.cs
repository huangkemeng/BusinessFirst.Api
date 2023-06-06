using RenameMe.Api.Primary.Contracts.Bases;
using RenameMe.Api.Primary.Entities.Bases;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;

namespace RenameMe.Api.Engines.Mediator
{
    public static class HandleRecordEntity
    {
        private static Assembly? Assembly { get; set; }
        private static MethodInfo? HandleMethodInfo { get; set; }
        public static Task Handle(DbContext dbContext, string traceId)
        {
            Assembly ??= Generate();
            if (Assembly != null)
            {
                HandleMethodInfo ??= GetMethodInfo(Assembly);
                if (HandleMethodInfo != null)
                {
                    return (Task)HandleMethodInfo.Invoke(null, new object[] { dbContext, traceId })!;
                }
            }
            return Task.CompletedTask;
        }

        private static MethodInfo? GetMethodInfo(Assembly assembly)
        {
            var handlerType = assembly.ExportedTypes.First();
            return handlerType.GetMethod("HandlerRecordAsync", BindingFlags.Static | BindingFlags.Public);
        }
        private static Assembly? Generate()
        {
            var icontractType = typeof(IContract<>);
            var recordEntityType = typeof(IRecordEntity<>);
            var recordEntities = icontractType.Assembly.ExportedTypes.Where(e => e.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == recordEntityType)).ToArray();
            StringBuilder stringBuilder = new();
            stringBuilder.Append($@"
using {typeof(IEntityPrimary).Namespace!.Replace(".Bases", "")};
using {typeof(DbContext).Namespace!};
using {typeof(CancellationToken).Namespace!};
using {typeof(object).Namespace!};
using {typeof(ValueTask<>).Namespace!};
using {typeof(IEnumerable<>).Namespace!};

namespace {typeof(HandleRecordEntity).Namespace}.AotuGenerate
{{
    public static class AutoAddRecordEntity
    {{
        public static async Task HandlerRecordAsync(DbContext? context,string? traceId)
        {{
            if (context != null)
            {{
                var entries = context.ChangeTracker.Entries();
                foreach (var entity in entries)
                {{
                    if(entity.State == EntityState.Modified)
                    {{
                        {GetAssertCode(recordEntities, recordEntityType, true)}
                    }}
                }}
            }}
        }}
    }}
}}
");
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(stringBuilder.ToString());
            var systemRuntime = Assembly.Load("System.Runtime");
            var references = new List<Assembly>()
                    {
                            typeof(object).Assembly,
                            typeof(IEntityPrimary).Assembly,
                            typeof(CancellationToken).Assembly,
                            typeof(IEnumerable<>).Assembly,
                            systemRuntime,
                            typeof(DbContext).Assembly
                    };
            var compilation = CSharpCompilation
            .Create($"AutoAddRecordEntityAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddSyntaxTrees(tree)
             .AddReferences(references.Select(x => MetadataReference.CreateFromFile(x.Location)));
            using var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);
            if (emitResult.Success)
            {
                return Assembly.Load(stream.GetBuffer());
            }
            return null;
        }
        private static StringBuilder GetAssertCode(Type[] recordEntities, Type recordBaseType, bool isAsync)
        {
            StringBuilder assertEntityBuilder = new();
            for (int i = 0; i < recordEntities.Length; i++)
            {
                var recordEntityType = recordEntities[i];
                var recordedEntity = recordEntityType.GetInterfaces().First(e => e.IsGenericType && e.GetGenericTypeDefinition() == recordBaseType).GenericTypeArguments[0];
                var entityObjectName = "entityObject_" + i;
                var originalObjectName = "originalObject_" + i;
                assertEntityBuilder.Append($@"
{(i != 0 ? "else" : "")} if (entity.Entity is {recordedEntity.Name})
{{
    var {originalObjectName} = entity.OriginalValues.ToObject() as {recordedEntity.Name};
    var {entityObjectName} = ({recordEntityType.Name}){recordEntityType.Name}.{nameof(IRecordEntity<IEntityPrimary>.FromOriginal)}({originalObjectName});
    {entityObjectName}.{nameof(IRecordEntity<IEntityPrimary>.TraceId)} = traceId;
    {(isAsync ? "await" : "")} context.Set<{recordEntityType.Name}>().Add{(isAsync ? "Async" : "")}({entityObjectName}!);
}}
");
            }
            return assertEntityBuilder;
        }
    }
}
