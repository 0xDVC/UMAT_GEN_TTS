## Test results

test data is random, basically AI generated `test_data`. To test the implementations 

#### 1. Constraint
```
Testing Constraint System

----------------------------------------

Testing Hard Constraints:
------------------------
Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.

1. Time Slot Conflict Constraint:
Time slot conflict penalty: 0
Time slot conflicts found: 0 violations

2. Room Conflict Constraint:
Room conflict penalty: 0
Room conflicts found: 0 violations

Testing Soft Constraints:
------------------------
Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.

1. Lab Preference Constraint:
Lab preference penalty: 0
Lab preference not met for technical courses: 

2. Time Preference Constraint:
Time preference penalty: 0
Time preference violations: 

Testing Constraint Manager:
-------------------------
Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%), GEG101 in LT1 (90/200 = 45%) (Penalty: 0.022)

1. Perfect chromosome fitness: 0.96375

Constraint Violations:
- Lab requirement violations: CSC201 requires lab but assigned to LT1 (Penalty: 0.250)
- Time slot conflicts found: 1 violations (Penalty: 0.200)
- Room conflicts found: 1 violations (Penalty: 0.200)
- Time preference violations: MATH101 at Monday 08:00-10:00, CSC201 at Monday 08:00-10:00, GEG101 at Monday 08:00-10:00 (Penalty: 0.150)
- Room efficiency issues: CSC201 in LT1 (60/200 = 30%), GEG101 in LT1 (90/200 = 45%) (Penalty: 0.032)

2. Violated chromosome fitness: 0.2587500000000001


```


#### 2. Crossover
```
Testing Crossover Strategy

Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00, GEG101 at Monday 08:00-10:00 (Penalty: 0.100)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00, GEG101 at Monday 08:00-10:00 (Penalty: 0.100)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)
Parent 1:

Chromosome Details:
------------------

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room:  (Capacity: , Students: 120)
Time: Monday 08:00-10:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: CR2 (Capacity: 100, Students: 90)
Time: Monday 08:00-10:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: LAB1 (Capacity: 120, Students: 60)
Time: Monday 10:00-12:00

Fitness: 0.9450

Parent 2:

Chromosome Details:
------------------

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room:  (Capacity: , Students: 120)
Time: Monday 08:00-10:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: CR2 (Capacity: 100, Students: 90)
Time: Monday 08:00-10:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: LAB1 (Capacity: 120, Students: 60)
Time: Monday 10:00-12:00

Fitness: 0.9450

Offspring 1:

Chromosome Details:
------------------

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room:  (Capacity: , Students: 120)
Time: Monday 08:00-10:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: CR2 (Capacity: 100, Students: 90)
Time: Monday 08:00-10:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: LAB1 (Capacity: 120, Students: 60)
Time: Monday 10:00-12:00

Fitness: 0.0000

Offspring 2:

Chromosome Details:
------------------

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room:  (Capacity: , Students: 120)
Time: Monday 08:00-10:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: CR2 (Capacity: 100, Students: 90)
Time: Monday 08:00-10:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: LAB1 (Capacity: 120, Students: 60)
Time: Monday 10:00-12:00

Fitness: 0.0000

```

#### 3. Fitness Calculator
```
Testing Fitness Calculator

Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.

Testing different chromosome configurations:
----------------------------------------

1. Conflict-free chromosome:
Expected: High fitness (close to 1.0)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%), GEG101 in LT1 (90/200 = 45%) (Penalty: 0.022)
Actual fitness: 0.96375
Chromosome details:

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room: (virtual - no room)
Time: Monday 08:00-10:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: in LAB1 (Capacity: 120, Students: 60)
Time: Monday 10:00-12:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: in LT1 (Capacity: 200, Students: 90)
Time: Monday 12:00-14:00

2. Time conflict chromosome:
Expected: Lower fitness (around 0.6-0.8 due to time slot conflicts)

Constraint Violations:
- Time slot conflicts found: 1 violations (Penalty: 0.200)
- Time preference violations: MATH101 at Monday 08:00-10:00, CSC201 at Monday 08:00-10:00, GEG101 at Monday 08:00-10:00 (Penalty: 0.150)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%), GEG101 in LT1 (90/200 = 45%) (Penalty: 0.022)
Actual fitness: 0.7137500000000001
Chromosome details:

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room: (virtual - no room)
Time: Monday 08:00-10:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: in LAB1 (Capacity: 120, Students: 60)
Time: Monday 08:00-10:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: in LT1 (Capacity: 200, Students: 90)
Time: Monday 08:00-10:00

3. Room conflict chromosome:
Expected: Lower fitness (around 0.6-0.8 due to room conflicts)

Constraint Violations:
- Lab requirement violations: CSC201 requires lab but assigned to LT1 (Penalty: 0.250)
- Time slot conflicts found: 1 violations (Penalty: 0.200)
- Room conflicts found: 1 violations (Penalty: 0.200)
- Time preference violations: MATH101 at Monday 08:00-10:00, CSC201 at Monday 08:00-10:00, GEG101 at Monday 08:00-10:00 (Penalty: 0.150)
- Room efficiency issues: CSC201 in LT1 (60/200 = 30%), GEG101 in LT1 (90/200 = 45%) (Penalty: 0.032)
Actual fitness: 0.2587500000000001
Chromosome details:

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room: (virtual - no room)
Time: Monday 08:00-10:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: in LT1 (Capacity: 200, Students: 60)
Time: Monday 08:00-10:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: in LT1 (Capacity: 200, Students: 90)
Time: Monday 08:00-10:00

```

#### 4. Mutation
```
Testing Mutation Strategy

Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.
Original Chromosome:
Course: MATH101 (virtual - no room) at Monday 08:00-10:00
Course: CSC201 in LAB1 at Monday 10:00-12:00
Course: GEG101 in LT1 at Monday 12:00-14:00

Mutated Chromosome:
Course: MATH101 (virtual - no room) at Monday 08:00-10:00
Course: CSC201 in LAB1 at Monday 10:00-12:00
Course: GEG101 in LT1 at Monday 12:00-14:00

```

#### 5. Selection
```
Testing Selection Strategy

Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.
Selected 2 chromosomes from population of 4
Original Population:
Chromosome Fitness: 0.2
- Course: MATH101 (Virtual) | (virtual - no room) | Time: Monday 08:00:00
- Course: CSC201 (Physical) | Room: LAB1 | Time: Monday 10:00:00
- Course: GEG101 (Hybrid) | Room: LT1 | Time: Monday 12:00:00

Chromosome Fitness: 0.4
- Course: MATH101 (Virtual) | (virtual - no room) | Time: Monday 08:00:00
- Course: CSC201 (Physical) | Room: LAB1 | Time: Monday 10:00:00
- Course: GEG101 (Hybrid) | Room: LT1 | Time: Monday 12:00:00

Chromosome Fitness: 0.6000000000000001
- Course: MATH101 (Virtual) | (virtual - no room) | Time: Monday 08:00:00
- Course: CSC201 (Physical) | Room: LAB1 | Time: Monday 10:00:00
- Course: GEG101 (Hybrid) | Room: LT1 | Time: Monday 12:00:00

Chromosome Fitness: 0.8
- Course: MATH101 (Virtual) | (virtual - no room) | Time: Monday 08:00:00
- Course: CSC201 (Physical) | Room: LAB1 | Time: Monday 10:00:00
- Course: GEG101 (Hybrid) | Room: LT1 | Time: Monday 12:00:00


Selected Chromosomes:
Chromosome Fitness: 0.4
- Course: MATH101 (Virtual) | (virtual - no room) | Time: Monday 08:00:00
- Course: CSC201 (Physical) | Room: LAB1 | Time: Monday 10:00:00
- Course: GEG101 (Hybrid) | Room: LT1 | Time: Monday 12:00:00

Chromosome Fitness: 0.2
- Course: MATH101 (Virtual) | (virtual - no room) | Time: Monday 08:00:00
- Course: CSC201 (Physical) | Room: LAB1 | Time: Monday 10:00:00
- Course: GEG101 (Hybrid) | Room: LT1 | Time: Monday 12:00:00

```

#### 6. Genetic Algorithm
```
Testing Genetic Algorithm Components

Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.

Constraint Violations:
- Lab requirement violations: CSC201 requires lab but assigned to LT1 (Penalty: 0.250)
- Time preference violations: GEG101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LT1 (60/200 = 30%) (Penalty: 0.020)

Chromosome Details:
------------------

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room:  (Capacity: , Students: 120)
Time: Tuesday 12:00-14:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: LT1 (Capacity: 200, Students: 60)
Time: Thursday 14:00-16:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: CR2 (Capacity: 100, Students: 90)
Time: Monday 08:00-10:00

Fitness: 0.7150

```

#### 7. Timetable Genetic Algorithm
```
Testing Timetable Genetic Algorithm

Generating courses...
Generating rooms...
Generating time slots...
Generated 3 courses, 3 rooms, and 25 time slots.

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)
Starting genetic algorithm...
Population size: 20
Target fitness: 0.95

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)

Constraint Violations:
- Time preference violations: MATH101 at Monday 08:00-10:00 (Penalty: 0.050)
- Room efficiency issues: CSC201 in LAB1 (60/120 = 50%) (Penalty: 0.010)
Target fitness reached at generation 1!

Genetic Algorithm completed after 1 generations
Best fitness achieved: 0.97

Best Solution Found:
--------------------

Chromosome Details:
------------------

Course: MATH101 (Engineering Mathematics I)
Mode: Virtual
Programs: CSC-Y1, EEE-Y1
Room:  (Capacity: , Students: 120)
Time: Monday 08:00-10:00

Course: GEG101 (Technical Communication)
Mode: Hybrid
Programs: CSC-Y2, EEE-Y3
Room: CR2 (Capacity: 100, Students: 90)
Time: Monday 10:00-12:00

Course: CSC201 (Data Structures)
Mode: Physical
Programs: CSC-Y2
Room: LAB1 (Capacity: 120, Students: 60)
Time: Monday 12:00-14:00

Fitness: 0.9700

```


