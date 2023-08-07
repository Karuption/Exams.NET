using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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

public class UserTest {
    public int TestId { get; set; }
    public string TestTitle { get; set; } = "";
    public string TestDescription { get; set; } = "";
    public IList<UserTestQuestion>? Problems { get; set; }
}

[JsonDerivedType(typeof(UserMultipleChoiceQuestion), typeDiscriminator:"MultipleChoice")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
public class UserTestQuestion {
    public string CreatedBy { get; set; }
    public int TestQuestionId { get;set; }
    public string Prompt { get; set; }
    public decimal TotalPointValue { get; set; }
}

public class UserMultipleChoiceQuestion : UserTestQuestion {
    public List<UserChoice?> Choices { get; set; }
}

public class UserChoice {
    public Guid Id { get; set; }
    public int TestQuestionID { get; set; }
    public required string Description { get; set; }
    public decimal ChoicePointValue { get; set; } = 0;
}

public class UserTestQuestionAnswer {
    [Key]
    public Guid Id { get; set; }
    [ForeignKey(nameof(TestQuestion))]
    public int QuestionId { get; set; }
    public string Answer { get; set; }
    public string? UserId { get; set; }
    public int TestQuestionId { get; set; }
}