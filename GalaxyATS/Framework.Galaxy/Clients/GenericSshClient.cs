using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;

namespace Framework.Galaxy
{
    /// <summary>
    /// Generic Client for SSH operations
    /// </summary>
    public class GenericSshClient
    {
        /// <summary>
        /// Connect to remote and execute commands
        /// </summary>
        /// <param name="host">Remote machine address to connect</param>
        /// <param name="userName">Authentication username to connect to host</param>
        /// <param name="password">Authentication Password to connect to host</param>
        /// <param name="commands">List of commands as strings to execute on host</param>
        /// <param name="sudo">Sudo user if to be connected as Sudo user, by default empty</param>
        /// <param name="port">Connection Port, defaults to 22</param>
        /// <returns>Output of the commands as a string</returns>
        public string Connect(string host, string userName, string password, List<string> commands, string sudo = "", int port = 22)
        {
            string output = "";
            //Connect to Host
            using (var client = new SshClient(host, port, userName, password))
            {
                client.Connect();
                ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 2048);

                //Connect as Sudo user
                if (sudo != "")
                {
                    RunAsSudo(password, shellStream, sudo);
                }

                //Execute commands
                foreach (string command in commands)
                {
                    WriteToStream(command, shellStream);
                    string stream = ReadStream(shellStream);
                    int index = stream.IndexOf(Environment.NewLine);
                    stream = stream.Substring(index + Environment.NewLine.Length);
                    Console.WriteLine("Command output: " + stream.Trim());
                    output += stream.Trim();
                }
                client.Disconnect();
            }
            return output;
        }

        /// <summary>
        /// Connect to remote and execute sftcClient to upload a file from physical path to Server location
        /// </summary>
        /// <param name="host">Remote machine address to connect</param>
        /// <param name="userName">Authentication username to connect to host</param>
        /// <param name="password">Authentication Password to connect to host</param>
        /// <param name="srcLocation">source location of the file</param>
        /// <param name="destLocation">destination location of the file</param>
        /// <param name="port">Connection Port, defaults to 22</param>
        public void UploadFIle(string host, string userName, string password, string srcLocation, string destLocation, int port = 22)
        {
            using (SftpClient client = new SftpClient(new PasswordConnectionInfo(host, port, userName, password)))
            {
                try
                {
                    client.Connect();
                    client.ChangeDirectory(destLocation);
                    string sourceFile = srcLocation;
                    using (Stream stream = File.OpenRead(sourceFile))
                    {
                        client.UploadFile(stream, @"" + Path.GetFileName(sourceFile), x => { Console.WriteLine(x); });
                    }
                    //Change the permission of the uploaded file
                    client.ChangePermissions(destLocation + Path.GetFileName(sourceFile), 777);
                    client.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occurring during upload a file :  " + e.ToString());
                }
            }
        }

        /// <summary>
        /// Connect to remote and execute sftpClient to download a file from Server location to physical path
        /// </summary>
        /// <param name="host">Remote machine address to connect</param>
        /// <param name="userName">Authentication username to connect to host</param>
        /// <param name="password">Authentication Password to connect to host</param>
        /// <param name="srcLocation">source location of the file</param>
        /// <param name="destLocation">destination location of the file</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="port">Connection Port, defaults to 22</param>
        public void DownloadFile(string host, string userName, string password, string srcLocation, string destLocation, string fileName, int port = 22)
        {
            using (SftpClient client = new SftpClient(new PasswordConnectionInfo(host, port, userName, password)))
            {
                try
                {
                    client.Connect();
                    client.ChangeDirectory(srcLocation);
                    IEnumerable<SftpFile> files = client.ListDirectory(srcLocation);
                    files = files.Where(file => file.Name.Contains(fileName));
                    foreach (SftpFile file in files)
                    {
                        string pathLocalFile = Path.Combine(destLocation, file.Name);
                        using (var stream = File.Create(pathLocalFile))
                        {
                            client.DownloadFile(file.FullName, stream);
                        }
                    }
                    client.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occurring during download a file :  " + e.ToString());
                }
            }
        }

        /// <summary>
        /// Writes command to stream
        /// </summary>
        /// <param name="cmd">Command to write</param>
        /// <param name="stream">stream to write commands on</param>
        private void WriteToStream(string cmd, ShellStream stream)
        {
            stream.WriteLine(cmd + "; echo end-of-stream");
            while (stream.Length == 0)
                Thread.Sleep(500);
        }

        /// <summary>
        /// Reads from Stream
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns></returns>
        private string ReadStream(ShellStream stream)
        {
            StringBuilder result = new StringBuilder();

            string line;
            while ((line = stream.ReadLine()) != "end-of-stream")
                result.AppendLine(line);

            return result.ToString();
        }

        /// <summary>
        /// Commands to execute to connect as Sudo user
        /// </summary>
        /// <param name="password">Password for the sudo user</param>
        /// <param name="stream">stream to run commands on</param>
        /// <param name="user">sudo user to connect as</param>
        private void RunAsSudo(string password, ShellStream stream, string user)
        {
            // Get logged in and get user prompt
            string prompt = stream.Expect(new Regex(@"[$>]"));

            // Send command and expect password or user prompt
            stream.WriteLine("sudo su - " + user);
            prompt = stream.Expect(new Regex(@"([$#>:])"));

            // Check to send password
            if (prompt.Contains(":"))
            {
                // Send password
                stream.WriteLine(password);
                prompt = stream.Expect(new Regex(@"[$#>]"));
            }
        }
    }
}
