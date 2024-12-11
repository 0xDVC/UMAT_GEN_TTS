using Moq;
using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Interfaces;
using Xunit;


namespace UMAT_GEN_TTS.Tests.Tests.GeneticAlgorithms
{
    public class PopulationManagerTests
    {
        [Fact]
        public void InitializePopulation_ShouldCreateValidChromosomes()
        {
            // Arrange
            var mockFitnessCalculator = new Mock<IFitnessCalculator>();
            var timeSlots = new List<TimeSlot> 
            { 
                new TimeSlot(
                    DayOfWeek.Monday, 
                    TimeSpan.FromHours(9), 
                    TimeSpan.FromHours(10)
                )
            };
            var rooms = new List<Room> { new Room() };

            var manager = new PopulationManager(
                mockFitnessCalculator.Object,
                new List<Course>(),
                populationSize: 10,
                elitePercentage: 0.1,
                diversityThreshold: 0.001,
                rooms: rooms,
                timeSlots: timeSlots
            );

            // Act
            var population = manager.InitializePopulation(new List<Course>());

            // Assert
            Assert.NotEmpty(population);
            Assert.All(population, chromosome => 
            {
                Assert.Empty(chromosome.Genes);
                Assert.All(chromosome.Genes, gene => 
                {
                    Assert.NotNull(gene.TimeSlot);
                    Assert.NotNull(gene.Course);
                });
            });
        }
    }
} 