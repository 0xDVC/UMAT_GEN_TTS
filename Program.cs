using UMAT_GEN_TTS.Core.Constraints;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Debug;
using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Validator;
using UMAT_GEN_TTS.Interfaces;
using UMAT_GEN_TTS.Services;
using UMAT_GEN_TTS.Middleware;
using UMAT_GEN_TTS.Core.Configuration;
using UMAT_GEN_TTS.Core.Models;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configuration Setup
var systemConfig = builder.Configuration
    .GetSection("SystemConfiguration")
    .Get<SystemConfiguration>() 
    ?? throw new InvalidOperationException("SystemConfiguration is missing from appsettings.json");

builder.Services.AddSingleton(systemConfig);

// Core Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "UMaT Timetabling API", 
        Version = "v1",
        Description = "API for generating and managing university timetables",
        Contact = new OpenApiContact
        {
            Name = "UMaT",
            Email = "support@umat.edu.gh"
        }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Timetable Generation Services
ConfigureTimeTableServices(builder.Services, systemConfig);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Pipeline Configuration
ConfigureApplicationPipeline(app);

app.Run();

// Service Configuration Methods
static void ConfigureTimeTableServices(IServiceCollection services, SystemConfiguration config)
{
    // Add this near the top of the method
    services.AddSingleton<TimetableDebugger>(sp => 
        new TimetableDebugger(
            sp.GetRequiredService<ILogger<TimetableDebugger>>(),
            sp.GetRequiredService<ConstraintManager>()
        ));

    // Register TimetableService as singleton ONCE
    services.AddSingleton<ITimetableService, TimetableService>();

    // Core Components
    services.AddSingleton<ConstraintManager>();
    services.AddSingleton<IFitnessCalculator>(sp => sp.GetRequiredService<ConstraintManager>());
    services.AddSingleton<TimetableValidator>();

    // Genetic Algorithm Components
    services.AddSingleton<ISelectionStrategy>(sp => 
        new TournamentSelection(config.AlgorithmSettings.TournamentSize));
    
    services.AddSingleton<ICrossoverStrategy>(sp => 
        new MultiPointCrossover(numPoints: 2));

    services.AddSingleton<IMutationStrategy>(sp => 
    {
        var timetableService = sp.GetRequiredService<ITimetableService>();
        return new AdaptiveMutation(
            rooms: GenerateRooms(config.RoomSettings),
            timeSlots: timetableService.GenerateTimeSlots(),
            baseMutationRate: config.AlgorithmSettings.MutationRate,
            adaptiveFactor: 0.5
        );
    });

    services.AddSingleton<PopulationManager>(sp => 
        new PopulationManager(
            sp.GetRequiredService<IFitnessCalculator>(),
            new List<Course>(),
            populationSize: config.AlgorithmSettings.PopulationSize,
            elitePercentage: config.AlgorithmSettings.ElitismRate
        ));

    services.AddSingleton<TimetableGeneticAlgorithm>(sp =>
        new TimetableGeneticAlgorithm(
            sp.GetRequiredService<PopulationManager>(),
            sp.GetRequiredService<ISelectionStrategy>(),
            sp.GetRequiredService<ICrossoverStrategy>(),
            sp.GetRequiredService<IMutationStrategy>(),
            sp.GetRequiredService<IFitnessCalculator>(),
            sp.GetRequiredService<TimetableValidator>(),
            sp.GetRequiredService<ILogger<TimetableGeneticAlgorithm>>(),
            sp.GetRequiredService<TimetableDebugger>(),
            maxGenerations: config.AlgorithmSettings.MaxGenerations,
            targetFitness: config.AlgorithmSettings.TargetFitness,
            stagnationLimit: config.AlgorithmSettings.StagnationLimit,
            convergenceThreshold: config.ConstraintSettings.MinAcceptableFitness
        ));

    // Remove this duplicate registration
    // services.AddScoped<ITimetableService, TimetableService>();
}

static void ConfigureApplicationPipeline(WebApplication app)
{
    // Error Handling
    app.UseMiddleware<ErrorHandlingMiddleware>();

    // Development Tools
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Security and Routing
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}



static List<Room> GenerateRooms(RoomConfiguration roomConfig)
{
    // Initialize with empty list - actual room generation would be implemented based on requirements
    return new List<Room>();
}


app.Run();

