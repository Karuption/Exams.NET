using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Models; 

public class TestQuestion {
    public string CreatedBy { get; set; }
    public int TestQuestionId { get;set; }
    public string Prompt { get; set; }
    public int TotalPointValue { get; set; }
}

public class FreeFormProblem:TestQuestion {
    public string? Answer { get; set; }
}

public class MultipleChoiceProblem:TestQuestion {
    public char Answer { get; set; }
    public IList<Choice>? Choices { get; set; }
}

public class Choice {
    public int Id { get; set; }
    public required char ChoiceLetter { get; set; }
    public required string Description { get; set; }
    public int ChoicePointValue { get; set; } = 0;
}