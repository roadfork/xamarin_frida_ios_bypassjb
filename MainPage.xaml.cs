using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;
using Xamarin.Forms;

namespace AwesomeApp
{
    class CryptoOps
    {
        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                plainText = "default";
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        public byte[] HashStringToBytes(string text)
        {
            if (text == null || text.Length <= 0)
                text = "default";
            SHA256 mySHA256 = SHA256.Create();
            byte[] array = Encoding.UTF8.GetBytes(text);
            byte[] hashValue = mySHA256.ComputeHash(array);
            return hashValue;
        }
    }

    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        
        public MainPage()
        {
            
            InitializeComponent();
         
        }

        void Generate(object sender, System.EventArgs e)
        {
            byte[] iv = Encoding.UTF8.GetBytes(IV.Text);
            string ct = Input.Text;
            CryptoOps ops = new CryptoOps();
            byte[] key = ops.HashStringToBytes(ct);
            byte[] outputBytes = ops.EncryptStringToBytes_Aes(ct, key, iv);
            string output = System.Convert.ToBase64String(outputBytes);
            Output.Text = $"{output}";

        }
    }
}
