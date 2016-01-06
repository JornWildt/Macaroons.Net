using System;
using System.Globalization;


namespace Macaroons
{
  /// <summary>
  /// Standard first party general caveat verifiers.
  /// </summary>
  public static class StandardCaveatVerifiers
  {
    /// <summary>
    /// Create standard "expires: -date-" caveat.
    /// </summary>
    /// <param name="expires"></param>
    /// <returns></returns>
    public static string CreateExpiresCaveat(DateTime expires)
    {
      return string.Format("expires: {0:yyyy-MM-dd'T'HH:mm:ss'Z'}", expires);
    }


    /// <summary>
    /// Verifies caveats of the form 'expires: %yyyy-%MM-%ddT%HH:%mm:%ssZ'.
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public static bool ExpiresVerifier(Packet cid, out string reason)
    {
      reason = null;

      string caveat = cid.ToString();
      if (!caveat.StartsWith("expires:"))
        return false;

      string ts = caveat.Substring(8).Trim();

      DateTime t;
      if (DateTime.TryParseExact(ts, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out t))
      {
        if (DateTime.Now < t)
          return true;

        reason = string.Format("Timestamp '{0}' has expired", ts);
        return false;
      }
      else
      {
        reason = string.Format("Invalid timestamp in '{0}'", caveat);
        return false;
      }
    }
  }
}
