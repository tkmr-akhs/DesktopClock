using System.Diagnostics;
using System.Text;

namespace DesktopClock.Core.Helpers;

/// <summary>
/// Provides utility methods for executing commands.
/// Sanitizing is not the responsibility of this class. Please ensure sanitization is completed before passing data to this class.
/// </summary>
public class CommandExecuter
{
    private readonly string _command;
    private string _fileName;
    private readonly ISanitizer _sanitizer;

    private class DefaultSanitizer : ISanitizer
    {
        internal static readonly ISanitizer Default = new DefaultSanitizer();

        public string SanitizeCommand(string cmd)
        {
            return cmd;
        }

        public IReadOnlyList<string> SanitizeArguments(IList<string> args)
        {
            return args.AsReadOnly();
        }
    }

    /// <summary>
    /// Initializes a new instance of the `CommandExecuter` class with a specified command and an optional sanitizer.
    /// If no sanitizer is provided, a default one is used.
    /// </summary>
    /// <param name="command">The command to be executed.</param>
    /// <param name="sanitizer">The sanitizer to be used for input sanitization. If null, the default sanitizer that do nothing is used.</param>
    public CommandExecuter(string command, ISanitizer sanitizer = default)
    {
        if (sanitizer == default)
        {
            _sanitizer = DefaultSanitizer.Default;
        }
        else
        {
            _sanitizer = sanitizer;
        }

        _command = _sanitizer.SanitizeCommand(command);
        
    }

    /// <summary>
    /// Executes a command. If the command stops in the middle of processing, this method will not complete.
    /// </summary>
    /// <param name="arguments">Command arguments. Double quotes within the arguments are replaced with two double quotes to escape them.</param>
    /// <param name="trim">Specifies whether to trim leading and trailing whitespace from the command output. The default value is true (trim).</param>
    /// <returns>The output of the executed command.</returns>
    public string Execute(IList<string> arguments, bool trim = true)
    {
        var args = _sanitizer.SanitizeArguments(arguments);

        _fileName ??= GetCommandPath(_command);

        return ExecuteCore(_command, _fileName, args, trim);
    }

    private static string ExecuteCore(string cmd, string fileName, IReadOnlyList<string> arguments, bool trim = true)
    {
        var escFileName = EscapeSpecialCharacter(fileName);
        var args = GetArgumentsLine(arguments);

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = cmd,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        System.Diagnostics.Debug.WriteLine($"startInfo.FileName: {startInfo.FileName}");
        System.Diagnostics.Debug.WriteLine($"startInfo.Arguments: {startInfo.Arguments}");

        string stdOut;
        string stdErr;
        int exitCode;

        using (Process process = Process.Start(startInfo))
        using (StreamReader stdOutReader = process.StandardOutput)
        using (StreamReader stdErrReader = process.StandardError)
        {
            process.WaitForExit();
            stdOut = stdOutReader.ReadToEnd();
            stdErr = stdErrReader.ReadToEnd();
            exitCode = process.ExitCode;
        }

        if (trim)
        {
            stdOut = stdOut.Trim();
            stdErr = stdErr.Trim();
        }

        stdOut = stdOut.Replace("\r\n", "\n");
        stdErr = stdErr.Replace("\r\n", "\n");

        if (exitCode != 0 || !String.IsNullOrEmpty(stdErr)) throw new CommandExecutionException(cmd, escFileName, args, stdOut, stdErr, exitCode);

        return stdOut;
    }

    private static string GetCommandPath(string command)
    {
        return ExecuteCore("cmd", "cmd.exe", new string[] { "/c", "where", command });
    }

    /// <summary>
    /// Executes a command asynchronously. If the command stops in the middle of processing, this method will not complete.
    /// </summary>
    /// <param name="arguments">Command arguments. Double quotes within the arguments are replaced with two double quotes to escape them.</param>
    /// <param name="trim">Specifies whether to trim leading and trailing whitespace from the command output. The default value is true (trim).</param>
    /// <returns>A task representing the asynchronous operation, containing the output of the executed command.</returns>
    public async Task<string> ExecuteAsync(string[] arguments, bool trim = true)
    {
        var args = _sanitizer.SanitizeArguments(arguments);

        _fileName ??= await GetCommandPathAsync(_command);

        return await ExecuteAsyncCore(_command, _fileName, args, trim);
    }

    private static async Task<string> ExecuteAsyncCore(string cmd, string fileName, IReadOnlyList<string> arguments, bool trim = true)
    {
        var escFileName = EscapeSpecialCharacter(fileName);
        var args = GetArgumentsLine(arguments);

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = cmd,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        System.Diagnostics.Debug.WriteLine($"startInfo.FileName: {startInfo.FileName}");
        System.Diagnostics.Debug.WriteLine($"startInfo.Arguments: {startInfo.Arguments}");

        string stdOut;
        string stdErr;
        int exitCode;

        using (Process process = Process.Start(startInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                stdOut = await reader.ReadToEndAsync();
            }

            using (StreamReader reader = process.StandardError)
            {
                stdErr = await reader.ReadToEndAsync();
            }

            await process.WaitForExitAsync();
            exitCode = process.ExitCode;
        }

        if (trim)
        {
            stdOut = stdOut.Trim();
            stdErr = stdErr.Trim();
        }

        stdOut = stdOut.Replace("\r\n", "\n");
        stdErr = stdErr.Replace("\r\n", "\n");

        if (exitCode != 0 || !String.IsNullOrEmpty(stdErr)) throw new CommandExecutionException(cmd, escFileName, args, stdOut, stdErr, exitCode);

        return stdOut;
    }

    private static string GetArgumentsLine(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0) return String.Empty;

        var result = new StringBuilder(EscapeSpecialCharacter(arguments[0]));

        for (int i = 1; i < arguments.Count; i++)
        {
            if (arguments[i] == null) continue;

            result.Append(" ")
                  .Append(EscapeSpecialCharacter(arguments[i]));
        }

        return result.ToString();
    }

    private static string EscapeSpecialCharacter(string s)
    {
        return $"\"{s.Replace("\"", "\"\"")}\"";
    }

    private static async Task<string> GetCommandPathAsync(string command)
    {
        return await ExecuteAsyncCore("cmd", "cmd.exe", new string[] { "/c", "where", command });
    }
}

/// <summary>
/// Exception thrown when a command execution results in an error output or a non-zero exit code.
/// </summary>
public class CommandExecutionException : Exception
{
    /// <summary>
    /// Gets the command of the command execution.
    /// </summary>
    public string Command { get; }
    
    /// <summary>
    /// Gets the command file name of the command execution.
    /// </summary>
    public string FileName { get; }
    
    /// <summary>
    /// Gets the arguments of the command execution.
    /// </summary>
    public string Arguments { get; }

    /// <summary>
    /// Gets the exit code of the command execution.
    /// </summary>
    public int ExitCode { get; }
    
    /// <summary>
    /// Gets the stardard output of the command execution.
    /// </summary>
    public string StandardOutput { get; }
    
    /// <summary>
    /// Gets the standard error output of the command execution.
    /// </summary>
    public string StandardError { get; }

    public CommandExecutionException(string cmd, string fileName, string args, string stdout, string stderr, int exitcode)
        : base(CreateMessage(stdout, stderr, exitcode))
    {
        Command = cmd;
        FileName = fileName;
        Arguments = args;
        StandardOutput = stdout;
        StandardError = stderr;
        ExitCode = exitcode;
    }

    private static string CreateMessage(string stdout, string stderr, int exitcode)
    {
        var message = new StringBuilder();
        message.AppendLine($"Command exited with code {exitcode}.");

        if (!string.IsNullOrEmpty(stderr))
        {
            message.AppendLine("Error:");
            message.AppendLine(stderr);
        }

        if (!string.IsNullOrEmpty(stdout))
        {
            message.AppendLine("Output:");
            message.AppendLine(stdout);
        }

        return message.ToString();
    }
}

/// <summary>
/// Defines the interface for sanitizers that are used to clean and validate command strings and their arguments. 
/// Implementations of this interface should focus on preventing security vulnerabilities such as command injection attacks.
/// </summary>
public interface ISanitizer
{
    /// <summary>
    /// Sanitizes a command string to ensure it is safe for execution.
    /// </summary>
    /// <param name="cmd">The command string to sanitize.</param>
    /// <returns>A sanitized version of the command string.</returns>
    string SanitizeCommand(string cmd);

    /// <summary>
    /// Sanitizes a list of command arguments to ensure they are safe for execution.
    /// </summary>
    /// <param name="args">The list of command arguments to sanitize.</param>
    /// <returns>A read-only list of sanitized command arguments.</returns>
    IReadOnlyList<string> SanitizeArguments(IList<string> args);
}
