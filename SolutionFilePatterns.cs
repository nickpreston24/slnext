using CodeMechanic.RegularExpressions;

public sealed class SolutionFilePatterns : RegexEnumBase
{
    public static SolutionFilePatterns ProjectHeader =
        new SolutionFilePatterns(1
            , nameof(ProjectHeader)
            , @"(?<project_definition>Project\(""\{(?<project_guid>[A-Z\d-]+?)\}""\)\s*   # Header
=\s+""(?<project_name>.*?)"",\s*    # project or solution folder are named here
""(?<project_path>[\w+\._\s'\\]+)""? ,\s*  # path, if exists.
""\{(?<config_guid>[A-Z\d-]+?)\}""?        # if project has a configuration, match the guid for that config.
)"
            , "https://regex101.com/r/tbvTwT/2");

    protected SolutionFilePatterns(int id, string name, string pattern,
        string uri = "") : base(id, name, pattern, uri)
    {
    }
}

public sealed class ProjectDefinition
{
    public string project_definition { get; set; } = string.Empty;
    public string project_guid { get; set; } = string.Empty;
    public string project_name { get; set; } = string.Empty;
    public string project_path { get; set; } = string.Empty;
    public string config_guid { get; set; } = string.Empty;
}