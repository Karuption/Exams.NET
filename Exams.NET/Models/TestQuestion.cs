using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Models; 

[JsonDerivedType(typeof(MultipleChoiceProblem), typeDiscriminator:"MultipleChoice")]
[JsonDerivedType(typeof(FreeFormProblem), typeDiscriminator:"FreeForm")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
public class TestQuestion {
    public string CreatedBy { get; set; }
    public int TestQuestionId { get;set; }
    public string Prompt { get; set; }
    public int TotalPointValue { get; set; }
}

[JsonDerivedType(typeof(MultipleChoiceProblemDto))]
[JsonDerivedType(typeof(FreeFormProblemDto))]
public class TestQuestionDto {
    public string? CreatedBy { get; set; }
    public int? TestQuestionId { get;set; }
    public string Prompt { get; set; }
    public int TotalPointValue { get; set; }
}

public class FreeFormProblem:TestQuestion {
    public string? Answer { get; set; }
}

public class FreeFormProblemDto:TestQuestionDto {
    public string? Answer { get; set; }
}

public class MultipleChoiceProblem:TestQuestion {
    public IList<Choice>? Choices { get; set; } = new List<Choice>();
}

public class MultipleChoiceProblemDto:TestQuestionDto {
    public IList<ChoiceDto>? Choices { get; set; } = Array.Empty<ChoiceDto>();
}

public class ChoiceDto {
    public Guid? Id { get; set; }
    public int? TestQuestionID { get; set; }
    public TestQuestion? TestQuestion { get; set; }
    public required string Description { get; set; }
    public int ChoicePointValue { get; set; } = 0;
}

public class Choice {
    public Guid Id { get; set; }
    public int TestQuestionID { get; set; }
    [JsonIgnore]
    public TestQuestion TestQuestion { get; set; }
    public required string Description { get; set; }
    public decimal ChoicePointValue { get; set; } = 0;
}