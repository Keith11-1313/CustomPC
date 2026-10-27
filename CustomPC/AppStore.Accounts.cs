using Microsoft.Data.Sqlite;
using System.Net.Mail;
using System.Security.Cryptography;

namespace CustomPC;

public sealed partial class AppStore
{
    public void Login(string email, string password)
    {
        Session = null;
        // Email ignores outer spaces and letter case. Passwords must match exactly.
        // Never trim the password: spaces can be part of a user's chosen password.
        if (string.IsNullOrWhiteSpace(email) || password.Length == 0)
        {
            throw new InvalidOperationException("Enter your email address and password.");
        }

        Refresh();
        CheckLoginThrottle(email);
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

        if (matchingAccount == null || !Roles.Contains(matchingAccount.Role) || !VerifyPassword(password, matchingAccount.PasswordHash)
            || password.Trim().Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)
            || (!DemoMode && password == "CustomPC123!"))
        {
            RecordLoginResult(email, false);
            throw new InvalidOperationException("Email or password is incorrect, or the account is archived.");
        }

        RecordLoginResult(email, true);
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

                if (string.IsNullOrWhiteSpace(user.Name) || user.Name.Length > 100 || user.Email.Length > 254 || !MailAddress.TryCreate(user.Email, out _))
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
                    if (NeedsSetup) throw new InvalidOperationException("Complete store setup before registering customers.");
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
                    ValidateNewPassword(password, user.Email);

                    user.PasswordHash = HashPassword(password);
                }
                else
                {
                    if (VerifyPassword(user.Email.Trim(), existingAccount.PasswordHash))
                        throw new InvalidOperationException("Choose a new password that differs from the email address.");
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

    // Assisted recovery changes only the password and its login throttle, atomically.
    public void ResetUserPassword(string userId, string newPassword)
    {
        using var connection = database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            LoadData(connection);
            RequireAdmin();
            UserAccount account = Users.FirstOrDefault(user => user.Id == userId)
                ?? throw new InvalidOperationException("The selected account no longer exists.");
            if (account.Archived) throw new InvalidOperationException("Restore this account before resetting its password.");
            ValidateNewPassword(newPassword, account.Email);
            account.PasswordHash = HashPassword(newPassword);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM LoginAttempts WHERE Key = $key";
            command.Parameters.AddWithValue("$key", LoginKey(account.Email));
            command.ExecuteNonQuery();
            SaveData(connection, transaction);
        }
        catch
        {
            UndoFailedChange(connection, transaction);
            throw;
        }
    }

    // Apply only when creating/changing a password. Existing accounts can still sign in.
    public static void ValidateNewPassword(string password, string? email = null)
    {
        if (email != null && password.Trim().Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Your password must differ from your email address.");
        if (password.Length < 12 || password.Length > 128 || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower) || !password.Any(char.IsDigit)
            || !password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            throw new InvalidOperationException("Use 12â€“128 characters with uppercase and lowercase letters, a number and a symbol.");
        if (password == "CustomPC123!") throw new InvalidOperationException("Choose a personal password.");
    }

    public static string HashPassword(string password)
    {
        // A salt makes identical passwords produce different hashes.
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 600000, HashAlgorithmName.SHA256, 32);

        return "PBKDF2-SHA256:600000:" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string savedPasswordHash)
    {
        string[] savedValues = savedPasswordHash.Split(':');
        // Older accounts used two fields and 210,000 rounds. Keep them readable until their password changes.
        int iterations = 210000;
        int saltIndex = 0;
        if (savedValues.Length == 4 && savedValues[0] == "PBKDF2-SHA256")
        {
            if (!int.TryParse(savedValues[1], out iterations) || iterations < 210000 || iterations > 1000000) return false;
            saltIndex = 2;
        }
        else if (savedValues.Length != 2)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(savedValues[saltIndex]);
            byte[] savedHash = Convert.FromBase64String(savedValues[saltIndex + 1]);
            if (salt.Length != 16 || savedHash.Length != 32)
            {
                return false;
            }

            byte[] enteredHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(enteredHash, savedHash);
        }
        catch (FormatException)
        {
            // A damaged stored value must fail sign-in, not bypass it or crash decoding.
            return false;
        }
    }
}
