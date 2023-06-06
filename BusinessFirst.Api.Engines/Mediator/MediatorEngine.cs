using Autofac;
using RenameMe.Api.Engines.Bases;
using RenameMe.Api.Infrastructure.Bases;
using RenameMe.Api.Primary.Contracts.Bases;
using RenameMe.Api.Realization.Bases;
using FluentValidation;
using Mediator.Net;
using Mediator.Net.Autofac;
using Mediator.Net.Binding;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace RenameMe.Api.Engines.Mediator
{
    public class ConfigureMediator : IBuilderEngine
    {
        private readonly ContainerBuilder builder;

        public ConfigureMediator(ContainerBuilder builder)
        {
            this.builder = builder;
        }
        public void Run()
        {
            var mediatorBuilder = new MediatorBuilder();
            var realizationAssembly = typeof(IRealization).Assembly;
            var icontractType = typeof(IContract<>);
            var contractTypes = icontractType.Assembly?
                    .ExportedTypes
                    .Where(x => x.GetInterfaces().Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == icontractType) && x.IsInterface && !x.IsGenericType)
                    .ToArray();

            var realizationTypes = realizationAssembly?
                    .ExportedTypes
                    .Where(e => e.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == icontractType) && e.IsClass && !e.IsAbstract)
                    .ToArray();
            List<SyntaxTree> trees = new();
            if (contractTypes != null && contractTypes.Any())
            {
                var messageBindings = new List<MessageBinding>();
                foreach (var contractType in contractTypes)
                {
                    var realizationType = realizationTypes?.FirstOrDefault(contractType.IsAssignableFrom);
                    if (realizationType != null)
                    {
                        var handler = realizationType.GetMethod("Handle");
                        if (handler != null)
                        {
                            var msgType = handler.GetParameters()[0].ParameterType.GenericTypeArguments[0];
                            messageBindings.Add(new MessageBinding(msgType, realizationType));

                            var validatorMethod = realizationType.GetMethod("Validator");
                            if (validatorMethod != null)
                            {
                                var ret = Activator.CreateInstance(validatorMethod.GetParameters()[0].ParameterType)!;
                                builder.Register(context =>
                                {
                                    var realizationObj = context.Resolve(realizationType);
                                    validatorMethod?.Invoke(realizationObj, new[] { ret });
                                    return ret;
                                })
                                .As(typeof(IValidator<>).MakeGenericType(msgType))
                                .InstancePerLifetimeScope();
                            }
                        }
                    }
                    else
                    {
                        var syntaxTree = GetSyntaxTree(contractType);
                        if (syntaxTree != null)
                        {
                            trees.Add(syntaxTree);
                        }
                    }
                }
                var fakerAssembly = GetFakerAssembly(trees);
                if (fakerAssembly != null)
                {
                    var handlerTypes = fakerAssembly.ExportedTypes;
                    foreach (var handlerType in handlerTypes)
                    {
                        var contractType = handlerType.GetInterfaces().FirstOrDefault();
                        if (contractType != null)
                        {
                            var ihandlerType = GetContractInputAndOutputType(contractType);
                            if (ihandlerType != null)
                            {
                                messageBindings.Add(new MessageBinding(ihandlerType.GenericTypeArguments[0], handlerType));
                                var validatorType = typeof(ContractValidator<>).MakeGenericType(ihandlerType.GenericTypeArguments[0]);
                                var command = Activator.CreateInstance(validatorType)!;
                                builder.Register(context =>
                                {
                                    return command;
                                })
                                .As(typeof(IValidator<>).MakeGenericType(ihandlerType.GenericTypeArguments[0]))
                                .InstancePerLifetimeScope();
                            }
                        }
                    }
                }
                mediatorBuilder.RegisterHandlers(() => messageBindings);
                mediatorBuilder.ConfigureGlobalReceivePipe(c =>
                {
                    c.AddPipeSpecification(new DoValidatePipe(c.DependencyScope));
                    c.AddPipeSpecification(new EfCorePipe(c.DependencyScope));
                });
            }
            builder.RegisterMediator(mediatorBuilder);
        }

        private static Type? GetContractInputAndOutputType(Type contractType)
        {
            return contractType.GetInterfaces().FirstOrDefault(e => e.IsGenericType && new Type[] { typeof(ICommandHandler<>), typeof(ICommandHandler<,>), typeof(IRequestHandler<,>) }.Contains(e.GetGenericTypeDefinition()));
        }

        public SyntaxTree? GetSyntaxTree(Type contractType)
        {
            var ihandlerType = GetContractInputAndOutputType(contractType);
            if (ihandlerType != null)
            {
                var root = SyntaxFactory.CompilationUnit();
                var returnTypeString = nameof(Task);
                var body = $"await {nameof(Task)}.{nameof(Task.CompletedTask)};";
                if (ihandlerType.GenericTypeArguments.Length == 2)
                {
                    returnTypeString = $"{nameof(Task)}<{ihandlerType.GenericTypeArguments[1].Name}>";
                    body = $"return await {nameof(BusinessFaker<object>)}<{ihandlerType.GenericTypeArguments[1].Name}>.{nameof(BusinessFaker<object>.CreateAsync)}();";
                }
                root = root.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IContract<>).Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(contractType.Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IRealization).Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(AbstractValidator<>).Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IReceiveContext<>).Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(Task).Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(CancellationToken).Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(object).Namespace!)))
                           .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(ISetting).Namespace!)));

                var classDeclaration = SyntaxFactory.ClassDeclaration($"{contractType.Name}FakerHandler")
                              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                              .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(contractType.Name)));

                var handleMethodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnTypeString), nameof(ICommandHandler<ICommand>.Handle))
                                          .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                                          .AddParameterListParameters(
                                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("context"))
                                                         .WithType(SyntaxFactory.ParseTypeName($"IReceiveContext<{ihandlerType.GenericTypeArguments[0].Name}>")),
                                                             SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                                                              .WithType(SyntaxFactory.ParseTypeName("CancellationToken"))
                                                             )
                                            .WithBody(SyntaxFactory.Block(
                                                            SyntaxFactory.ParseStatement(body)
                                                         ));

                var testMethodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(nameof(Task)), nameof(IContract<IMessage>.TestAsync))
                                                         .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                                         .WithBody(SyntaxFactory.Block(
                                                            SyntaxFactory.ParseStatement("throw new NotImplementedException();")
                                                         ));

                var validatorMethodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), nameof(IContract<IMessage>.Validator))
                                           .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                           .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("validator")).WithType(SyntaxFactory.ParseTypeName($"{nameof(ContractValidator<IMessage>)}<{ihandlerType.GenericTypeArguments[0].Name}>")))
                                           .WithBody(SyntaxFactory.Block(
                                              SyntaxFactory.ParseStatement("return;")
                                           ));
                classDeclaration = classDeclaration.AddMembers(handleMethodDeclaration, testMethodDeclaration, validatorMethodDeclaration);
                var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(!string.IsNullOrWhiteSpace(contractType.Namespace) ? (contractType.Namespace + ".FakerHandlers") : "FakerHandlers"));
                namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);
                root = root.AddMembers(namespaceDeclaration);
                SyntaxTree tree = SyntaxFactory.SyntaxTree(root);
                return tree;
            }
            return null;
        }

        private static Assembly? GetFakerAssembly(List<SyntaxTree> trees)
        {
            if (trees.Any())
            {
                var references = new List<Assembly>
                {
                    typeof(object).Assembly,
                    typeof(IContract<>).Assembly,
                    typeof(ISetting).Assembly,
                    typeof(IRealization).Assembly,
                    typeof(IMediator).Assembly,
                    typeof(AbstractValidator<>).Assembly,
                    typeof(Task<>).Assembly,
                    typeof(AutoBogus.AutoFaker).Assembly,
                    typeof(Bogus.Faker).Assembly,
                    typeof(void).Assembly,
                    Assembly.Load("System.Runtime"),
                    Assembly.Load("netstandard")
                };
                var compilation = CSharpCompilation
                 .Create($"IContractFakerImplementationAssembly")
                 .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                 .AddSyntaxTrees(trees)
                 .AddReferences(references.Distinct().Select(x => MetadataReference.CreateFromFile(x.Location)));
                using var stream = new MemoryStream();
                var compileResult = compilation.Emit(stream);
                if (compileResult.Success)
                {
                    var fakerImplementationAssembly = Assembly.Load(stream.GetBuffer());
                    return fakerImplementationAssembly;
                }
            }
            return null;
        }
    }
}
