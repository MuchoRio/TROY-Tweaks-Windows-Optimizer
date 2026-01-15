namespace NRTX.Optimizer.Core.Abstractions;

public interface IProfile
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Icon { get; }
    IReadOnlyList<string> TargetTweakIds { get; }
}
