using TestRunner.UI.DTOs;

namespace TestRunner.UI.Services.Interfaces
{
    public interface IAZClient
    {
        AZReleaseDto GetRelease(string releaseId);
        AZBuildDto GetBuild(string buildId);
        AZCommitDto GetCommit(string buildId);
    }
}
