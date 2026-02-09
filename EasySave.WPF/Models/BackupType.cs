namespace EasySave.WPF.Models
{
    /// <summary>
    /// Defines the available types of backup operations.
    /// 
    /// This enumeration is used to select the appropriate
    /// backup strategy during job execution.
    /// </summary>
    public enum BackupType
    {
        Full,
        Differential
    }
}
