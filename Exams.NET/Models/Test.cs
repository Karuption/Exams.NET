using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Models; 

public class Test {
    public int TestId { get; set; }
    public string UserId { get; set; }
    public string TestTitle { get; set; } = "";
    public DateTime Created { get; set; }
    public DateTime LastUpdated { get; set; }
    public IList<TestQuestion>? Problems { get; set; }
}

public class TestDto {
    public int? TestId { get; set; }
    public string? UserId { get; set; }
    public string TestTitle { get; set; } = "";
    public DateTime? Created { get; set; }
    public DateTime? LastUpdated { get; set; }
    public IList<TestQuestion>? Problems { get; set; }
}

[PrimaryKey(nameof(TestQuestionId))]
public class TestQuestion {
    [Key]
    public string CreatedBy { get; set; }
    public int TestQuestionId { get;set; }
    public string Prompt { get; set; }
    public int TotalPointValue { get; set; }
}

public interface ITestQuestionUserAnswer {
    public TestQuestion Question { get; set; }
    public string UserId { get; set; }
}

public class FreeFormProblem:TestQuestion {
    public string? Answer { get; set; }
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

public class MultipleChoiceProblem:TestQuestion {
    public char Answer { get; set; }
    public IList<Choice>? Choices { get; set; }
}

public class Choice {
    public required char Key { get; set; }
    public required string Description { get; set; }
    public int ChoicePointValue { get; set; } = 0;
}

public class MultipleChoiceUserUserAnswer:ITestQuestionUserAnswer {
    public MultipleChoiceProblem Question { get; set; }
    public string UserId { get; set; }
    public char Answer { get; set; }
    TestQuestion ITestQuestionUserAnswer.Question {
        get => Question;
        set => Question = (MultipleChoiceProblem)value;
    }
}