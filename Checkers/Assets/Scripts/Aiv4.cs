using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Aiv4 : MonoBehaviour
{

    public string runShell(string myBoard)
    {
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        //cmd.StandardInput.WriteLine("python test.py w ________ b_b_b_b_ ___b____ __b_____ _w______ w_b___w_ _w_____b w_____w_ hastaAqui");
        cmd.StandardInput.WriteLine("python test.py " + myBoard +" hastaAqui");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        cmd.WaitForExit();

        string signal = "hastaAqui";
        string info = cmd.StandardOutput.ReadToEnd();

        int index = info.IndexOf(signal);
        index += 9;

        string results = info.Substring(index);

        //UnityEngine.Debug.Log(results);
        return results;
    }


}
