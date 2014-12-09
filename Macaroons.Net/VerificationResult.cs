using System.Collections.Generic;


namespace Macaroons.Net
{
  /// <summary>
  /// Represents the result of verifying a macaroon.
  /// </summary>
  public class VerificationResult
  {
    /// <summary>
    /// Verification success indicator (true means success).
    /// </summary>
    public bool Success { get; protected set; }

    
    /// <summary>
    /// Read only list of failure messages when verification fails.
    /// </summary>
    public IList<string> Messages { get { return MessagesList.AsReadOnly(); } }


    /// <summary>
    /// Internal read/write list of messages.
    /// </summary>
    protected List<string> MessagesList { get; set; }


    public VerificationResult()
    {
      Success = true;
      MessagesList = new List<string>();
    }


    /// <summary>
    /// Initialize verification result with a single failure message and Success = false.
    /// </summary>
    /// <param name="message"></param>
    public VerificationResult(string message)
      : this()
    {
      AddFailure(message);
    }


    /// <summary>
    /// Add failure message and set Success = false.
    /// </summary>
    /// <param name="message"></param>
    public void AddFailure(string message)
    {
      Success = false;
      MessagesList.Add(message);
    }


    /// <summary>
    /// Merge failure messages from other verification result and update Success.
    /// </summary>
    /// <param name="src"></param>
    public void MergeFailures(VerificationResult src)
    {
      Success = Success && src.Success;
      MessagesList.AddRange(src.MessagesList);
    }
  }
}
