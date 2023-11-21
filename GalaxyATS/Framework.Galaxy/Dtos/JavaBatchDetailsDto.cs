using System.Collections.Generic;

namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// Dto for the JavaBatch Details Information
    /// </summary>
    public class JavaBatchDetailsDto
    {
        public List<JavaBatchDetails> JavaBatchDetails { get; set; }
    }

    public class JavaBatchDetails
    {
        public string BatchName { get; set; }
        public string ExistingLocation { get; set; }
        public string UpdatedLocation { get; set; }
    }
}
