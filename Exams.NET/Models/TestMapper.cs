using Riok.Mapperly.Abstractions;

namespace Exams.NET.Models; 

[Mapper]
public partial class TestMapper {
    public partial Test DtoToEntity(TestCreationDto testCreation);

    public partial FreeFormProblem QuestionDtoToEntity(FreeFormProblemDto testQuestion);
    public partial MultipleChoiceProblem QuestionDtoToEntity(MultipleChoiceProblemDto testQuestion);
}