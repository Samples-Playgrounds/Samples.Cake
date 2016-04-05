
public int Shell(string source)
{
    string[] commands = source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

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
        int idx = cmd_tmp.IndexOf(" ", 0);
        int len = cmd_tmp.Length;

        string process_executable = null;
        string process_args = null;
        
        if (idx > 0)
        {
            process_executable = cmd_tmp.Substring(0, idx);
            process_args = cmd_tmp.Substring(idx + 1, len - idx - 1);
            
            StartProcess (process_executable, process_args);
        }
    }
    
    return 0;
}
