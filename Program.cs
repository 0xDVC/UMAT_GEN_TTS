using UMAT_GEN_TTS.Tests.Unit;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

TestDataGeneratorTests.RunTest();
GeneticAlgorithmTests.RunTest();
SelectionTests.RunTest();
CrossoverTests.RunTest();
MutationTests.RunTest();
FitnessCalculatorTests.RunTest();
TimetableGeneticAlgorithmTests.RunTest();
ConstraintTests.RunTest();


app.Run();