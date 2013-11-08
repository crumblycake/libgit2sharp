namespace LibGit2Sharp
{
    /// <summary>
    /// Class that holds credentials for remote repository access.
    /// </summary>
    public abstract class Credentials
    {
    }

    public class BasicCredentials : Credentials
    {
        /// <summary>
        /// Username for username/password authentication (as in HTTP basic auth).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for username/password authentication (as in HTTP basic auth).
        /// </summary>
        public string Password { get; set; }
    }

    public class SshCredentials : Credentials
    {
        public string Username { get; set; }
        public string PublicKeyPath { get; set; }
        public string PrivateKeyPath { get; set; }
        public string Passphrase { get; set; }
    }
}
