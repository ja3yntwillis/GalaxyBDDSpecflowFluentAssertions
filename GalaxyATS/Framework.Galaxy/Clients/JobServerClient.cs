using System;
using System.Collections.Generic;
using System.IO;
using Cedar.Configuration;

namespace Framework.Galaxy
{
    /// <summary>
    /// Job Server Client to connect and execute batch process on a Job server
    /// </summary>
    public class JobServerClient : GenericSshClient
    {
        /// <summary>
        /// Method to Execute commands on a batch process
        /// </summary>
        /// <param name="batchProcess">Batch Process name to be executed</param>
        /// <param name="parameters">Parameters to pass on with batch execution, defaults to AutomationTest</param>
        /// <param name="path">Path to Batch process on Job server, defaults to newjavabatch folder</param>
        /// <returns>returns the batch execution time in UTC</returns>
        public DateTime ExecuteBatch(string batchProcess, string parameters = "AutomationTest", string path = "/u01/DevOps/newjavabatch")
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var sudoUser = JobServerConfig.SudoUser;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;

            //Get JavaBatch List Details
            var javaBatchPathFromConfig = JavaBatchDetailsClient.GetJavaBatchDetails(batchProcess);
            //Set Path Came as an Input
            //ToDo-Later From Test Layer the path parameter value and from ExecuteBatch the path parameter input value will be removed
            //Override Path if set under BatchDetails Json
            var pathName = javaBatchPathFromConfig ?? path;

            //Create Commands for JavaBatches
            List<string> commands = new List<string>();
            commands.Add($"cd {pathName}");
            commands.Add($"./{batchProcess}.sh {parameters}");

            //Get the Utc time right before batch execution
            var execTime = DateTime.UtcNow;

            //Connect to JobServer and execute commands
            Connect(jobServer, userName, password, commands, sudoUser, port);

            return execTime;
        }

        /// <summary>
        /// Method to Execute commands on a batch process and return console log
        /// </summary>
        /// <param name="batchProcess">Batch Process name to be executed</param>
        /// <param name="parameters">Parameters to pass on with batch execution, defaults to AutomationTest</param>
        /// <param name="path">Path to Batch process on Job server, defaults to newjavabatch folder</param>
        /// <returns>returns the batch execution log</returns>
        public string ExecuteBatchWithLog(string batchProcess, string parameters = "AutomationTest", string path = "/u01/DevOps/newjavabatch")
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var sudoUser = JobServerConfig.SudoUser;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;

            //Get JavaBatch List Details
            var javaBatchPathFromConfig = JavaBatchDetailsClient.GetJavaBatchDetails(batchProcess);
            //Set Path Came as an Input
            //ToDo-Later From Test Layer the path parameter value and from ExecuteBatchWithLog the path parameter input value will be removed
            //Override Path if set under BatchDetails Json
            var pathName = javaBatchPathFromConfig ?? path;

            //Create Commands for JavaBatches
            List<string> commands = new List<string>();
            commands.Add($"cd {pathName}");
            commands.Add($"./{batchProcess}.sh {parameters}");

            //Connect to JobServer and execute commands, return log message
            var output = Connect(jobServer, userName, password, commands, sudoUser, port);
            return output;
        }

        public void ExecuteCopyFile(string fileName, string srcLocation, string destLocation = "/u01/ftp_files/common/Clients/")
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var sudoUser = JobServerConfig.SudoUser;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;

            //Create Commands for JavaBatches
            List<string> commands = new List<string>();
            commands.Add($"cp {srcLocation}{fileName} {destLocation}{fileName}");

            //Connect to JobServer and execute commands
            Connect(jobServer, userName, password, commands, sudoUser, port);
        }

        /// <summary>
        /// Method to Execute SFTP client to upload a file to server location
        /// </summary>
        /// <param name="fileName">Filename want to upload to the server</param>
        /// <param name="srcLocation">Source location of the file</param>
        /// <param name="destLocation">Path to file upload on the Job server, defaults to /tmp/ folder</param>
        public void UploadFileToServer(string fileName, string srcLocation, string destLocation = "/tmp/")
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;
            string srcFileLocation = Path.Combine(srcLocation, fileName);

            //Connect to SFTP client and upload file
            UploadFIle(jobServer, userName, password, srcFileLocation, destLocation, port);
        }

        /// <summary>
        /// Method to Execute SFTP client to download a file from server location to physical path
        /// </summary>
        /// <param name="fileName">Filename want to download from the server</param>
        /// <param name="destLocation">Path to file download on the physical location</param>
        /// <param name="srcLocation">Source location of the file, defaults to /tmp/ folder</param>
        public void DownloadFileFromServer(string fileName, string destLocation, string srcLocation = "/tmp/")
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;

            //Connect to SFTP client and download file
            DownloadFile(jobServer, userName, password, srcLocation, destLocation, fileName, port);
        }

        /// <summary>
        /// Method to Read a specific file on FTP Server
        /// </summary>
        /// <param name="fileName">Filename to be read</param>
        /// <param name="pathToFile">Path to file, defaults to FTP filepath from JobServerConfig, can be set from Test Level</param>
        /// <returns>Returns the file content as a string</returns>
        public string ReadFile(string fileName, string pathToFile = null)
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var sudoUser = JobServerConfig.SudoUser;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;
            var filePath = pathToFile ?? JobServerConfig.FtpOutFilePath;

            //Command to display content of a specific file
            List<string> commands = new List<string>();
            commands.Add($"cat {filePath}/{fileName}");

            //Connect to JobServer and execute command
            var output = Connect(jobServer, userName, password, commands, sudoUser, port);
            return output;
        }

        /// <summary>
        /// Execute .sql File with sql plus from jobserver
        /// </summary>
        /// <param name="sqlFileName">Filename to be Executed</param>
        /// <param name="filePath">File Location - Default null (Internally will derive the ..../scripts/ProdOps/sql/ location)</param>
        /// <returns>SQL plus execution output</returns>   
        public string ExecuteSqlFile(string sqlFileName, string filePath = null)
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var sudoUser = JobServerConfig.SudoUser;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;
            var dbUsername = TestConfiguration.DbUserName;
            var dbPassword = TestConfiguration.DBPassword;
            var dbEnvironment = TestConfiguration.DatabaseName;
            const string startQuote = "'\"";
            const string endQuote = "\"'";
            var dbPasswordWithQuotes = startQuote + dbPassword + endQuote;

            var environment = dbEnvironment.Contains("ENTS_STAGE") ? "ENTSTG" : "ENT" + dbEnvironment.Split("_")[1];
            //Create Commands for JavaBatches
            List<string> commands = new List<string>();

            if (filePath == null)
            {
                commands.Add($"$ORACLE_HOME/bin/sqlplus {dbUsername}/{dbPasswordWithQuotes}@{dbEnvironment.Replace("SUPPORT", "BATCH")} @/u01/ProdOps/{environment.ToLower()}/scripts/ProdOps/sql/{sqlFileName}");
            }
            else
            {
                commands.Add($"$ORACLE_HOME/bin/sqlplus {dbUsername}/{dbPasswordWithQuotes}@{dbEnvironment.Replace("SUPPORT", "BATCH")} @{filePath}{sqlFileName}");
            }

            //Connect to JobServer and execute commands
            var output = Connect(jobServer, userName, password, commands, sudoUser, port);
            return output;
        }

        /// <summary>
        /// Move a file from one directory to another
        /// </summary>
        /// <param name="fileName"> File name to move </param>
        /// <param name="srcLocation"> Source location to move from </param>
        /// <param name="destLocation"> Desitination location - Default "/u01/ftp_files/common/Clients/"</param>
        public void ExecuteMoveFile(string fileName, string srcLocation, string destLocation = "/u01/ftp_files/common/Clients/")
        {
            //Get Job server Config details
            var jobServer = JobServerConfig.JobServerHost;
            var port = JobServerConfig.JobServerPort;
            var sudoUser = JobServerConfig.SudoUser;
            var userName = JobServerConfig.Username;
            var password = JobServerConfig.Password;

            //Create Commands for JavaBatches
            List<string> commands = new List<string>();
            commands.Add($"mv -f {srcLocation}{fileName} {destLocation}");

            //Connect to JobServer and execute commands
            Connect(jobServer, userName, password, commands, sudoUser, port);
        }
    }
}