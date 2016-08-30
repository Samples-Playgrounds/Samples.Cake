
public int Shell(string source)
{
    string[] commands = source.Split
                                (
                                    new string[] { Environment.NewLine }, 
                                    StringSplitOptions.None
                                );

    foreach (string command in commands)
    {
        string cmd_tmp = command.TrimStart();
        if(IsRunningOnWindows())
        {
            if (cmd_tmp.StartsWith("::"))
            {
                continue;
            }
        }
        if(IsRunningOnUnix())
        {
            if (cmd_tmp.StartsWith("#"))
            {
                continue;
            }
        }

        string[] command_parts = command.Split
                                    (
                                        new string[] { " " }, 
                                        StringSplitOptions.None
                                    );

        List<string>  command_parts_cleaned = new List<string>();

        foreach(string cp in command_parts)
        {
            if ( cp.Equals(string.Empty) || cp.Equals( " " ) || cp.Equals(Environment.NewLine))
            {
                continue;
            }
            //Information("     command_part = {0}", cp);
            command_parts_cleaned.Add(cp);
        }

        string process_executable = null;
        string process_args = null;
        
        if (command_parts_cleaned.Count >= 1)
        {
            process_executable = command_parts_cleaned[0];
            command_parts_cleaned.RemoveAt(0);
            process_args = string.Join(" ", command_parts_cleaned);
        }
        if (process_executable != null && process_args != null)
        {
            Information("     process_executable = {0}", process_executable);
            Information("     process_args       = {0}", process_args);
                
            StartProcess (process_executable, process_args);
        }
    }
    
    return 0;
}
