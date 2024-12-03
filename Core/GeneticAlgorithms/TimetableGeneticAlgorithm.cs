using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms
{
    public class TimetableGeneticAlgorithm
    {
        private readonly IFitnessCalculator _fitnessCalculator;
        private readonly ISelectionStrategy _selectionStrategy;
        private readonly ICrossoverStrategy _crossoverStrategy;
        private readonly IMutationStrategy _mutationStrategy;
        private readonly int _populationSize;
        private readonly int _maxGenerations;
        private readonly double _targetFitness;

        private List<Course> _courses;
        private List<Room> _rooms;
        private List<TimeSlot> _timeSlots;

        public TimetableGeneticAlgorithm(
            IFitnessCalculator fitnessCalculator,
            ISelectionStrategy selectionStrategy,
            ICrossoverStrategy crossoverStrategy,
            IMutationStrategy mutationStrategy,
            int populationSize = 50,
            int maxGenerations = 1000,
            double targetFitness = 0.95)
        {
            _fitnessCalculator = fitnessCalculator;
            _selectionStrategy = selectionStrategy;
            _crossoverStrategy = crossoverStrategy;
            _mutationStrategy = mutationStrategy;
            _populationSize = populationSize;
            _maxGenerations = maxGenerations;
            _targetFitness = targetFitness;
        }

        public void Initialize(List<Course> courses, List<Room> rooms, List<TimeSlot> timeSlots)
        {
            _courses = courses;
            _rooms = rooms;
            _timeSlots = timeSlots;
        }

        public Chromosome Run()
        {
            // 1. Initialize population
            var population = InitializePopulation();
            var bestChromosome = population[0];
            var generation = 0;

            Console.WriteLine("Starting genetic algorithm...");
            Console.WriteLine($"Population size: {_populationSize}");
            Console.WriteLine($"Target fitness: {_targetFitness}");

            while (generation < _maxGenerations)
            {
                generation++;

                // 2. Evaluate fitness for all chromosomes
                foreach (var chromosome in population)
                {
                    chromosome.CalculateFitness(_fitnessCalculator);
                }

                // 3. Find best chromosome
                var currentBest = population.MaxBy(c => c.Fitness);
                if (currentBest.Fitness > bestChromosome.Fitness)
                {
                    bestChromosome = currentBest;
                    Console.WriteLine($"Generation {generation}: New best fitness = {bestChromosome.Fitness}");
                }

                // 4. Check if target fitness reached
                if (bestChromosome.Fitness >= _targetFitness)
                {
                    Console.WriteLine($"Target fitness reached at generation {generation}!");
                    break;
                }

                // 5. Create new population
                var newPopulation = new List<Chromosome>();

                while (newPopulation.Count < _populationSize)
                {
                    // Select parents
                    var parents = _selectionStrategy.Select(population, 2);
                    if (parents.Count != 2) continue;

                    // Perform crossover
                    var (offspring1, offspring2) = _crossoverStrategy.Crossover(parents[0], parents[1]);

                    // Perform mutation
                    _mutationStrategy.Mutate(offspring1);
                    _mutationStrategy.Mutate(offspring2);

                    newPopulation.Add(offspring1);
                    newPopulation.Add(offspring2);
                }

                // 6. Replace old population
                population = newPopulation;
            }

            Console.WriteLine($"\nGenetic Algorithm completed after {generation} generations");
            Console.WriteLine($"Best fitness achieved: {bestChromosome.Fitness}");

            return bestChromosome;
        }

        private List<Chromosome> InitializePopulation()
        {
            var population = new List<Chromosome>();

            for (int i = 0; i < _populationSize; i++)
            {
                var chromosome = CreateRandomChromosome();
                chromosome.CalculateFitness(_fitnessCalculator);
                population.Add(chromosome);
            }

            return population;
        }

        private Room? FindSuitableRoom(Course course, List<Room> availableRooms)
        {
            if (course.Mode == CourseMode.Virtual)
                return null;

            var suitableRooms = availableRooms
                .Where(r => course.CanUseRoom(r))
                .OrderBy(r => r.Ownership != RoomOwnership.Departmental)  // Prefer departmental rooms
                .ThenBy(r => Math.Abs(r.Capacity - course.StudentCount))  // Best capacity fit
                .ToList();

            return suitableRooms.FirstOrDefault();
        }

        public Chromosome CreateRandomChromosome()
        {
            var chromosome = new Chromosome();
            
            // Sort courses by constraints (most constrained first):
            var sortedCourses = _courses
                .OrderByDescending(c => c.ProgrammeYears.Count)  // Multi-programme courses first
                .ThenByDescending(c => c.RequiresLab)           // Lab courses second
                .ThenByDescending(c => c.StudentCount)          // Larger classes third
                .ThenByDescending(c => c.CreditHours)           // Higher credit hours fourth
                .ToList();
            
            foreach (var course in sortedCourses)
            {
                // Find available time slots (no conflicts)
                var availableSlots = _timeSlots
                    .Where(ts => !HasTimeConflict(chromosome, course, ts))
                    .ToList();

                // If no conflict-free slots, find least conflicting slot
                TimeSlot selectedSlot;
                if (!availableSlots.Any())
                {
                    selectedSlot = FindLeastConflictingSlot(chromosome, course, _timeSlots);
                    Console.WriteLine($"Warning: Had to use conflicting slot for {course.Code}");
                }
                else
                {
                    selectedSlot = availableSlots[Random.Shared.Next(availableSlots.Count)];
                }

                // Find suitable room
                var selectedRoom = course.Mode != CourseMode.Virtual 
                    ? FindSuitableRoom(course, _rooms)
                    : null;

                chromosome.Genes.Add(new Gene(course, selectedSlot, selectedRoom));
            }

            return chromosome;
        }

        private bool HasTimeConflict(Chromosome chromosome, Course newCourse, TimeSlot timeSlot)
        {
            return chromosome.Genes.Any(g => 
                g.TimeSlot.Overlaps(timeSlot) && (
                    // Same lecturer
                    g.Course.LecturerId == newCourse.LecturerId ||
                    // Same programme year
                    g.Course.HasProgrammeConflict(newCourse) ||
                    // Same room (if not virtual)
                    (newCourse.Mode != CourseMode.Virtual && 
                     g.Room != null && 
                     g.Room == FindSuitableRoom(newCourse, _rooms))
                )
            );
        }

        private TimeSlot FindLeastConflictingSlot(Chromosome chromosome, Course course, List<TimeSlot> slots)
        {
            return slots
                .OrderBy(ts => CountConflicts(chromosome, course, ts))
                .First();
        }

        private int CountConflicts(Chromosome chromosome, Course course, TimeSlot slot)
        {
            var conflicts = 0;
            foreach (var gene in chromosome.Genes.Where(g => g.TimeSlot.Overlaps(slot)))
            {
                // Lecturer conflict (highest priority)
                if (gene.Course.LecturerId == course.LecturerId)
                    conflicts += 100;

                // Programme year conflict (high priority)
                if (gene.Course.HasProgrammeConflict(course))
                    conflicts += 50;

                // Room conflict (medium priority)
                if (course.Mode != CourseMode.Virtual && 
                    gene.Room != null && 
                    gene.Room == FindSuitableRoom(course, _rooms))
                    conflicts += 25;
            }
            return conflicts;
        }
    }
}