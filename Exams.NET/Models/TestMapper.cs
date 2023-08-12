using Riok.Mapperly.Abstractions;

namespace Exams.NET.Models; 

[Mapper(UseDeepCloning = true)]
public static partial class TestMapper {
    public static partial Test ToEntity(this TestCreationDto testCreation);

    public static partial UserTest ToUser(this Test test);
    
    public static partial FreeFormProblem ToEntity(this FreeFormProblemDto testQuestion);
    public static partial MultipleChoiceProblem ToEntity(this MultipleChoiceProblemDto testQuestion);


    public static UserTestQuestion ToUserQuestion(this TestQuestion testQuestion) {
        return testQuestion switch {
            FreeFormProblem ffp       => ToUserQuestion(ffp),
            MultipleChoiceProblem mcp => ToUserQuestion(mcp),
        };
    }
    public static partial UserMultipleChoiceQuestion ToUserQuestion(this MultipleChoiceProblem multipleChoiceProblem);
    public static partial UserTestQuestion ToUserQuestion(this FreeFormProblem freeFormProblem);
    public static partial UserChoice ToUser(this Choice choice);

    public static partial UserTestQuestionAnswer? ToEntity(this UserTestQuestionAnswerCreationDto questionAnswerCreationDto);
}