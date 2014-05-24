using System;
using UnityEngine;
public class GameVersion
{
    public static string major = "1"; //Note, can not be 0
    public static string minor = "1"; //Note, MUST be single digit, can not be 0
    public static string patch = "1"; //Note, MUST be single digit, can not be 0

    private static int build = 0; //Change with every build. Used to verify all clients run the same version

    public static int getVersion()
    {
        int minorint = Convert.ToInt32(minor);
        int patchint = Convert.ToInt32(patch);
        if(minorint > 9 || patchint > 9 || minorint <= 0 || patchint <= 0)
        {
            Debug.LogError("Minor or Patch version error! Minor: " + minor + " Patch: " + patch);
            return 0;
        }
        return Convert.ToInt32(major + minor + patch);
    }

    public static int getBuild()
    {
        return build;
    }
}