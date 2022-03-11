using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types.Descriptors.Definitions;
using System.Linq.Expressions;
using System.Reflection;

namespace HotChocolateTest
{

    public record CmContext<T>(T Source);

    public class CmField<TParentType, TGraphType, TType>: CmField<TParentType>
        where TParentType : class
        where TGraphType : class, IOutputType
        where TType : IComparable
    {
        public CmField(string name): base(typeof(TGraphType), typeof(TType), name)
        {

        }
        public CmField<TParentType, TGraphType, TType> Description(string description)
        {
            _description = description;
            return this;
        }

        public CmField<TParentType, TGraphType, TType> ResolveAsync(Func<CmContext<TParentType>, ValueTask<TType?>> resolver)
        {
            _resolver = async ctx => await resolver.Invoke(ctx);
            return this;
        }

        public CmField<TParentType, TGraphType, TType> Resolve(Func<CmContext<TParentType>, TType?> resolver)
        {
            _resolver = ctx => ValueTask.FromResult((object?)resolver.Invoke(ctx));
            return this;
        }
    }

    public class CmField<TParentType>
        where TParentType : class
    {
        private readonly string _name;
        private Type _outputGraphType;
        private Type _outputType;
        protected Func<CmContext<TParentType>, ValueTask<object?>>? _resolver;
        protected string? _description;

        public CmField(Type outputGraphType, Type outputType, string name)
        {
            _outputGraphType = outputGraphType;
            _outputType = outputType;
            _name = name;
        }

        public void Build(IObjectTypeDescriptor<TParentType> descriptor)
        {
            var builder = descriptor.Field(_name).Type(_outputGraphType);
            if(_resolver != null)
            {
                FieldResolverDelegate resolver = (context) =>
                {
                    var parent = context.Parent<TParentType>();
                    return _resolver(new(parent));
                };
                builder = builder.Resolve(resolver);
            }
            if (_description != null)
                builder = builder.Description(_description);
        }
    }
    public class CmObjectType<T> : ObjectType<T> where T : class
    {
        private readonly List<CmField<T>> _fields = new List<CmField<T>>();
        protected CmField<T, TGraphType, TType> Field<TGraphType, TType>(string name)
             where TGraphType : class, IOutputType where TType : IComparable
        {
            var field = new CmField<T, TGraphType, TType>(name);
            _fields.Add(field);
            return field;
        }
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            foreach (var field in _fields)
                field.Build(descriptor);
        }
    }
    public class WeatherForecastType : CmObjectType<WeatherForecast>
    {
        public WeatherForecastType(DataLoaderAccessor dataloader)
        {
            Field<StringType, string>("Summary")
                .Description("The summary field.")
                .Resolve(x => x.Source.Summary);

            Field<DateTimeType, DateTime>("Date")
                .Resolve(x => x.Source.Date);

            Field<IntType, int>("TemperatureF")
                .Resolve(f => f.Source.TemperatureF);

            Field<IntType, int>("TemperatureC")
                .Resolve(f => f.Source.TemperatureC);

            Field<IntType, int>("Temp")
                .ResolveAsync(async x =>
                {
                    return await dataloader.BatchDataLoader<int, int>(
                        async (keys, ct) =>
                        {
                            var multiplier = new Random().Next(1, 10);
                            await Task.Delay(1);
                            return keys.ToDictionary(x => x, x => x * multiplier);
                        })
                    .LoadAsync(x.Source.TemperatureC);
                });
        }
    }

    public class TypeHelper<T> where T: class
    {
        private readonly IObjectTypeDescriptor<T> _descriptor;

        public TypeHelper(IObjectTypeDescriptor<T> descriptor)
        {
            _descriptor = descriptor;
        }

        public NamedFieldHelper<T> Name<TResponse>(string name) where TResponse: class, IOutputType
        {
            return new NamedFieldHelper<T>(_descriptor.Field(name).Type<TResponse>());
        }

    }

    public class NamedFieldHelper<T>: IObjectFieldDescriptor where T: class
    {
        private IObjectFieldDescriptor _descriptor;

        public NamedFieldHelper(IObjectFieldDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public IObjectFieldDescriptor ResolveAsync(Func<CmContext<T>, ValueTask<object?>> fieldResolver)
        {
            FieldResolverDelegate resolver = (context) =>
            {
                var parent = context.Parent<T>();
                return fieldResolver(new (parent));
            };
            return _descriptor.Resolve(resolver);
        }

        public IObjectFieldDescriptor Argument(NameString argumentName, Action<IArgumentDescriptor> argumentDescriptor)
        {
            return _descriptor.Argument(argumentName, argumentDescriptor);
        }

        public IObjectFieldDescriptor Deprecated(string? reason)
        {
            return _descriptor.Deprecated(reason);
        }

        public IObjectFieldDescriptor Deprecated()
        {
            return _descriptor.Deprecated();
        }

        public IObjectFieldDescriptor DeprecationReason(string? reason)
        {
            return _descriptor.DeprecationReason(reason);
        }

        public IObjectFieldDescriptor Description(string? value)
        {
            return _descriptor.Description(value);
        }

        public IObjectFieldDescriptor Directive<T1>(T1 directiveInstance) where T1 : class
        {
            return _descriptor.Directive(directiveInstance);
        }

        public IObjectFieldDescriptor Directive<T1>() where T1 : class, new()
        {
            return _descriptor.Directive<T1>();
        }

        public IObjectFieldDescriptor Directive(NameString name, params ArgumentNode[] arguments)
        {
            return _descriptor.Directive(name, arguments);
        }

        public IDescriptorExtension<ObjectFieldDefinition> Extend()
        {
            return _descriptor.Extend();
        }

        public IObjectFieldDescriptor Ignore(bool ignore = true)
        {
            return _descriptor.Ignore(ignore);
        }

        public IObjectFieldDescriptor Name(NameString value)
        {
            return _descriptor.Name(value);
        }

        public IObjectFieldDescriptor Resolve(FieldResolverDelegate fieldResolver)
        {
            return _descriptor.Resolve(fieldResolver);
        }

        public IObjectFieldDescriptor Resolve(FieldResolverDelegate fieldResolver, Type? resultType)
        {
            return _descriptor.Resolve(fieldResolver, resultType);
        }

        public IObjectFieldDescriptor Resolver(FieldResolverDelegate fieldResolver)
        {
            return _descriptor.Resolver(fieldResolver);
        }

        public IObjectFieldDescriptor Resolver(FieldResolverDelegate fieldResolver, Type resultType)
        {
            return _descriptor.Resolver(fieldResolver, resultType);
        }

        public IObjectFieldDescriptor ResolveWith<TResolver>(Expression<Func<TResolver, object?>> propertyOrMethod)
        {
            return _descriptor.ResolveWith(propertyOrMethod);
        }

        public IObjectFieldDescriptor ResolveWith(MemberInfo propertyOrMethod)
        {
            return _descriptor.ResolveWith(propertyOrMethod);
        }

        public IObjectFieldDescriptor Subscribe(SubscribeResolverDelegate subscribeResolver)
        {
            return _descriptor.Subscribe(subscribeResolver);
        }

        public IObjectFieldDescriptor SyntaxNode(FieldDefinitionNode? fieldDefinition)
        {
            return _descriptor.SyntaxNode(fieldDefinition);
        }

        public IObjectFieldDescriptor Type(ITypeNode typeNode)
        {
            return _descriptor.Type(typeNode);
        }

        public IObjectFieldDescriptor Type(Type type)
        {
            return _descriptor.Type(type);
        }

        public IObjectFieldDescriptor Use(FieldMiddleware middleware)
        {
            return _descriptor.Use(middleware);
        }

        IObjectFieldDescriptor IObjectFieldDescriptor.Type<TOutputType>()
        {
            return _descriptor.Type<TOutputType>();
        }

        IObjectFieldDescriptor IObjectFieldDescriptor.Type<TOutputType>(TOutputType outputType)
        {
            return _descriptor.Type(outputType);
        }
    }
}
