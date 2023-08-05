using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Models; 

public class Test {
    public int TestId { get; set; }
    public string UserId { get; set; }
    public string TestTitle { get; set; } = "";
    public string TestDescription { get; set; } = "";
    public DateTime Created { get; set; }
    public DateTime LastUpdated { get; set; }
    public IList<TestQuestion>? Problems { get; set; }
}

public class TestCreationDto {
    public int? TestId { get; set; }
    public string? UserId { get; set; }
    public string TestTitle { get; set; } = "";
    public string? TestDescription { get; set; } = "";
    public DateTime? Created { get; set; }
    public DateTime? LastUpdated { get; set; }
    public IList<TestQuestion>? Problems { get; set; }
}

public interface ITestQuestionUserAnswer {
    public TestQuestion Question { get; set; }
    public string UserId { get; set; }
}
public class FreeFormUserUserAnswer: ITestQuestionUserAnswer {
    public int UserAnswerID { get; set; }
    public FreeFormProblem Question { get; set; }
    public string UserId { get; set; }
    public string? Answer { get; set; }
    TestQuestion ITestQuestionUserAnswer.Question {
        get => Question;
        set => Question = (FreeFormProblem)value;
    }
}

public class MultipleChoiceUserUserAnswer:ITestQuestionUserAnswer {
    public MultipleChoiceProblem Question { get; set; }
    public string UserId { get; set; }
    TestQuestion ITestQuestionUserAnswer.Question {
        get => Question;
        set => Question = (MultipleChoiceProblem)value;
    }
}