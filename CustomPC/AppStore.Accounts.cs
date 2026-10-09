using Microsoft.Data.Sqlite;
using System.Net.Mail;
using System.Security.Cryptography;

namespace CustomPC;

public sealed partial class AppStore
{
    public void Login(string email, string password)
    {
        Refresh();
        UserAccount? matchingAccount = null;

        foreach (UserAccount account in Users)
        {
            bool matchingEmail = account.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
            if (!account.Archived && matchingEmail)
            {
                matchingAccount = account;
                break;
            }
        }

        if (matchingAccount == null || !VerifyPassword(password, matchingAccount.PasswordHash))
        {
            throw new InvalidOperationException("Email or password is incorrect, or the account is archived.");
        }

        Session = matchingAccount;
    }

    public void Logout()
    {
        Session = null;
    }

    public void SaveUser(UserAccount user, string password, bool registration = false)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);

                if (!registration)
                {
                    RequireAdmin();
                }

                if (string.IsNullOrWhiteSpace(user.Name) || !MailAddress.TryCreate(user.Email, out _))
                {
                    throw new InvalidOperationException("Enter a name and valid email address.");
                }

                UserAccount? existingAccount = null;

                foreach (UserAccount account in Users)
                {
                    if (account.Id == user.Id)
                    {
                        existingAccount = account;
                    }
                    else if (account.Email.Equals(user.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("This email address is already registered.");
                    }
                }

                if (registration)
                {
                    if (existingAccount != null)
                    {
                        throw new InvalidOperationException("Registration must create a new account.");
                    }

                    // Public registration cannot give someone staff or admin access.
                    user.Role = "Customer";
                    user.Archived = false;
                }

                if (Array.IndexOf(Roles, user.Role) == -1)
                {
                    throw new InvalidOperationException("Choose a valid role.");
                }

                if (existingAccount == null || password.Length > 0)
                {
                    if (password.Length < 8)
                    {
                        throw new InvalidOperationException("Use a password of at least 8 characters.");
                    }

                    user.PasswordHash = HashPassword(password);
                }
                else
                {
                    user.PasswordHash = existingAccount.PasswordHash;
                }

                bool editingSignedInAccount = Session != null && user.Id == Session.Id;
                if (editingSignedInAccount && (user.Archived || user.Role != "Admin"))
                {
                    throw new InvalidOperationException("You cannot archive or demote your current admin account.");
                }

                user.Email = user.Email.Trim();

                if (existingAccount != null)
                {
                    Users.Remove(existingAccount);
                }

                Users.Add(user);
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    public static string HashPassword(string password)
    {
        // A salt makes identical passwords produce different hashes.
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210000, HashAlgorithmName.SHA256, 32);

        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string savedPasswordHash)
    {
        string[] savedValues = savedPasswordHash.Split(':');
        byte[] salt = Convert.FromBase64String(savedValues[0]);
        byte[] savedHash = Convert.FromBase64String(savedValues[1]);
        byte[] enteredHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210000, HashAlgorithmName.SHA256, 32);

        return CryptographicOperations.FixedTimeEquals(enteredHash, savedHash);
    }
}
