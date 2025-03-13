using CodeMechanic.Async;
using CodeMechanic.Diagnostics;
using CodeMechanic.FileSystem;
using CodeMechanic.RegularExpressions;
using CodeMechanic.Shargs;
using Sharpify.Core;
using Sharprompt;

public class SlnGenerator : QueuedService
{
    private readonly ArgsMap arguments;

    public SlnGenerator(ArgsMap arguments)
    {
        this.arguments = arguments;
        // steps.Add(GenerateSlnx);
        steps.Add(GenerateSlnxUsingSharpify);
        if (this.arguments.HasCommand("clean"))
            steps.Add(Cleanup);
    }

    private async Task Cleanup()
    {
        string cwd = Directory.GetCurrentDirectory();
        var conversion_files = new Grepper()
            {
                FileSearchMask = "*.conversion.*", Recursive = true,
                RootPath = cwd
            }
            .GetFileNames();

        conversion_files.Take(5).Dump(nameof(conversion_files));

        bool run_cleanup = Prompt.Confirm("Clean up all .conversion files?");

        if (run_cleanup)
        {
            foreach (var file_path in conversion_files)
            {
                File.Delete(file_path);
            }
        }
    }

    private async Task GenerateSlnxUsingSharpify()
    {
        bool debug = arguments.HasFlag("--debug");
        string cwd = Directory.GetCurrentDirectory();
        var sharpify = new SharpifyService(arguments);

        // var refactors = sharpify.GetRefactors(cwd);
        // if (debug)
        //     refactors.Dump(nameof(refactors), ignoreNulls: false);
        // var files_for_conversion =
        //     await sharpify.GetFilesForConversion(refactors);

        // if (debug)
        //     files_for_conversion.Dump(nameof(files_for_conversion));

        await sharpify.Convert();
    }

    private async Task GenerateSlnx()
    {
        bool debug = arguments.HasFlag("--debug");
        string cwd = Directory.GetCurrentDirectory();

        var sln_files =
            new Grepper()
                {
                    Recursive = true,
                    RootPath = cwd, FileSearchMask = "*.sln",
                    // FileSearchLinePattern = $".*"
                }
                // .GetFileNames()
                .GetMatchingFiles(SolutionFilePatterns.ProjectHeader
                    .CompiledRegex)
                .ToArray();

        if (debug)
        {
            new Grepper()
            {
                Recursive = true,
                RootPath = cwd, FileSearchMask = "*.sln",
            }.GetFileNames().Dump("sln files (no pattern)");
        }

        if (sln_files.Length == 0)
        {
            Console.WriteLine("no .sln files found. Exiting.");
            return;
        }

        await ConvertSlnToSlnx(sln_files);
    }

    private async Task ConvertSlnToSlnx(Grepper.GrepResult[] sln_files)
    {
        var Q = new SerialQueue();
        var tasks = sln_files
            .Select(res =>
                Q.Enqueue(async () =>
                {
                    Console.WriteLine(
                        $"Looking for projects in .sln file '{res.FileName}'");
                    var projects = await ExtractProjectDefinitions(res);
                    if (projects.Count == 0)
                        Console.WriteLine("no extractions found");

                    foreach (var def in projects)
                    {
                    }
                })
            );

        await Task.WhenAll(tasks);
        Console.WriteLine("Done converting.");
    }

    private async Task<List<ProjectDefinition>> ExtractProjectDefinitions(
        Grepper.GrepResult res)
    {
        var projects = res.Line.Extract<ProjectDefinition>(SolutionFilePatterns
            .ProjectHeader
            .CompiledRegex);

        return projects;
    }

    public class SolutionFileDefinition
    {
        public SolutionFileDefinition(
            List<ProjectDefinition> project_definitions)
        {
            this.projects = project_definitions;
        }

        public List<ProjectDefinition> projects { get; set; } = new();
    }
}