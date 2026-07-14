namespace FrontEnd.Models;

public sealed record ProjectDocumentationData(
    ProjectApiDto? Project,
    ProjectDocumentApiDto? Document,
    IReadOnlyList<ProjectDocumentBlockApiDto> Blocks,
    IReadOnlyList<MediaApiDto> Media,
    IReadOnlyList<ProjectTechnologyApiDto> Technologies,
    IReadOnlyList<ProjectLearningApiDto> Learnings,
    bool TechnologyLoadFailed = false,
    bool LearningLoadFailed = false);
