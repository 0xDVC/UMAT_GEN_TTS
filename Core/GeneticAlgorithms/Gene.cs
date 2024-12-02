using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;
public class Gene
{
    public Course Course { get; }
    public TimeSlot TimeSlot { get; set; }
    public Room? Room { get; set; }
    public bool IsCompromised { get; set; }

    public Gene(Course course, TimeSlot timeSlot, Room? room, bool isCompromised = false)
    {
        Course = course;
        TimeSlot = timeSlot;
        Room = course.Mode == CourseMode.Virtual ? null : room;
        IsCompromised = isCompromised;
    }

    public override string ToString()
    {
        var roomInfo = Room != null ? $"in {Room.Name}" : "(no room)";
        return $"{Course.Code} {roomInfo}{(IsCompromised ? "*" : "")} at {TimeSlot}";
    }
} 