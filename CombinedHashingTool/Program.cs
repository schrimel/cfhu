using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
	public static void Main(String[] args)
    {
        byte[] testFile = CreateSingleFileHash(args[0]);
        Console.WriteLine(BitConverter.ToString(testFile).Replace("-", "").ToLower());

        try { 
            byte[] testFolder = CreateFolderHash(args[1]);
            Console.WriteLine(BitConverter.ToString(testFolder).Replace("-", "").ToLower());
        } catch(UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
        }

        SetEnvironmentVariableUser("TEST_ENV_CSS", BitConverter.ToString(testFile).Replace("-", "").ToLower());
    }

    
    
    static void SetEnvironmentVariableUser(String iEnvname, String iValue)
    {
        if (iEnvname == null || iEnvname.Length == 0)
            return;
        if (iValue== null || iValue.Length == 0)
            return;
        Environment.SetEnvironmentVariable(iEnvname, iValue, EnvironmentVariableTarget.User);
    }

    static void SetEnvironmentVariableMachine(String iEnvname, String iValue)
    {
        if (iEnvname == null || iEnvname.Length == 0)
            return;
        if (iValue == null || iValue.Length == 0)
            return;
        Environment.SetEnvironmentVariable(iEnvname, iValue, EnvironmentVariableTarget.Machine);
    }

    static String GetEnv(String iEnv)
    {
        return Environment.GetEnvironmentVariable(iEnv);
    }

    static Byte[] CreateSingleFileHash(String iFilename)
    {
        using(var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(iFilename))
            {
                return md5.ComputeHash(stream);
            }
        }
    }

    static Byte[] CreateFolderHash(String iFolder)
    {
        if (iFolder == null || iFolder.Length == 0 || !Directory.Exists(iFolder))
        {
            byte[] r = { 0 };
            return r;
        }

        byte[] rValue;

        var files = Directory.GetFiles(iFolder, "*.*", SearchOption.AllDirectories).OrderBy(p => p).ToList();
        using(var md5 = MD5.Create())
        {
            for(var i = 0; i < files.Count; ++i)
            {
                string file = files[i];
                // hash path
                string relativePath = file.Substring(iFolder.Length + 1);
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                byte[] contentBytes = File.ReadAllBytes(file);
                if (i == files.Count - 1)
                    md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                else
                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }
            rValue = md5.Hash;
        }
        return rValue;
    }
}