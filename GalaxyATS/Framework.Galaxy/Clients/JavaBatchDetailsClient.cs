using Cedar.Configuration;
using Framework.Galaxy.Dtos;
using System.IO;
using System.Text.Json;

namespace Framework.Galaxy
{
    /// <summary>
    /// Class to get/set JavaBatch Existing or Updated Location for a JavaBatch
    /// </summary>
    public static class JavaBatchDetailsClient
    {
        private static JavaBatchDetailsDto javaBatchData;

        /// <summary>
        /// Get a record from JavaBatch Details List
        /// </summary>
        /// <param name="batchName">Java BatchName without .sh as it is deployed in Job Server and passed from Test Layer</param>
        /// <returns>JavaBatch details from Json List for a Batch Name from the entered Batches else null</returns>
        public static string GetJavaBatchDetails(string batchName)
        {
            if (javaBatchData == null)
            {
                GetJavaBatchListDetails();
            }

            var batchData = javaBatchData.JavaBatchDetails.Find(x => x.BatchName == batchName);

            if (batchData != null)
            {
                return batchData.UpdatedLocation ?? batchData.ExistingLocation;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Fetch all JavaBatch details data from JavaBatchDetails.json
        /// </summary>
        /// <returns>JavaBatch details list as setup under JavaBatchDetails.json</returns>
        private static void GetJavaBatchListDetails()
        {
            var listJson = File.ReadAllText(Path.Combine(TestConfiguration.ResourcePath, "JavaBatchDetails.json"));
            javaBatchData = JsonSerializer.Deserialize<JavaBatchDetailsDto>(listJson);
        }
    }
}
