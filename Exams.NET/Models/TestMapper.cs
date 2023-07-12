using Riok.Mapperly.Abstractions;

namespace Exams.NET.Models; 

[Mapper]
public partial class TestMapper {
    public partial Test DtoToEntity(TestCreationDto testCreation);
}