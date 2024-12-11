namespace UMAT_GEN_TTS.Exceptions;

public class TimetableGenerationException : Exception
{
    public TimetableGenerationException(string message) 
        : base(message) { }

    public TimetableGenerationException(string message, Exception inner) 
        : base(message, inner) { }
}