using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Aiv4 : MonoBehaviour
{
    void runShell()
    {
        string strCmdText;
        strCmdText = "python test.py";
        System.Diagnostics.Process.Start("CMD.exe", strCmdText);
    }

    void runShell2()
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "python test.py";
        process.StartInfo = startInfo;
        process.Start();
    }

    void runShell3()
    {
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine("python test.py w ________ b_b_b_b_ ___b____ __b_____ _w______ w_b___w_ _w_____b w_____w_ hastaAqui");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        cmd.WaitForExit();

        string info = cmd.StandardOutput.ReadToEnd();
        UnityEngine.Debug.Log(info);
    }

    // Start is called before the first frame update
    void Start()
    {
        runShell3();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
