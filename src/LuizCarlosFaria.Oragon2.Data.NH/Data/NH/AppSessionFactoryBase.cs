using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Cfg.MappingSchema;
using System.Linq;
using NHibernate.Cfg.Loquacious;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LuizCarlosFaria.Oragon2.Data.NH;

public abstract class AppSessionFactoryBase
{
    private readonly ILogger logger;

    public ISessionFactory SessionFactory { get; }
    protected IConfiguration Configuration { get; }

    protected abstract bool CanRecreateSchemaOnStartup { get; }

    protected AppSessionFactoryBase(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, IConfiguration aspNetConfiguration)
    {
        this.logger = loggerFactory.CreateLogger<AppSessionFactoryBase>();

        this.Configuration = aspNetConfiguration;

        NHibernateLogger.SetLoggersFactory(new NHibernateToMicrosoftLoggerFactory(loggerFactory));

        var mapping = this.BuildMapping();

        var nhConfiguration = this.BuildConfiguration(mapping);

        nhConfiguration.SessionFactory().GenerateStatistics();

        this.SessionFactory = nhConfiguration.BuildSessionFactory();

        this.PrepareEnvironment(nhConfiguration);
    }

    protected abstract void AddMappings(ModelMapper mapper);

    protected abstract void ConfigureDataBaseIntegration(DbIntegrationConfigurationProperties db);

    private HbmMapping BuildMapping()
    {
        var mapper = new ModelMapper();
        this.AddMappings(mapper);
        var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
        this.logger.LogDebug("HbmMapping Configuration : {hbm}", mapping.AsString());
        return mapping;
    }

    private Configuration BuildConfiguration(HbmMapping mapping)
    {
        var configuration = new Configuration();

        configuration = configuration.DataBaseIntegration(this.ConfigureDataBaseIntegration);

        configuration.AddMapping(mapping);

        return configuration;
    }

    public ISession OpenSession()
    {
        return this.SessionFactory.OpenSession();
    }

    public void PrepareEnvironment(Configuration configuration)
    {
        if (this.CanRecreateSchemaOnStartup)
            this.RecreateSchemas(configuration);

        this.UpdateSchema(configuration);

        this.ValidateSchema(configuration);
    }

    protected virtual void UpdateSchema(Configuration configuration) => new SchemaUpdate(configuration).Execute(true, true);

    protected virtual void ExportSchema(Configuration configuration) => new SchemaExport(configuration).Create(true, true);

    protected virtual void ValidateSchema(Configuration configuration) => new SchemaValidator(configuration).Validate();

    private void RecreateSchemas(Configuration configuration)
    {
        using ISession session = this.OpenSession();
        foreach (var schema in configuration.ClassMappings.Select(it => it?.Table?.Schema).Where(it => !string.IsNullOrWhiteSpace(it)).Distinct().ToArray())
        {
            session.CreateSQLQuery($"CREATE SCHEMA IF NOT EXISTS {schema?.ToLowerInvariant()};").ExecuteUpdate();
        }
    }
}
