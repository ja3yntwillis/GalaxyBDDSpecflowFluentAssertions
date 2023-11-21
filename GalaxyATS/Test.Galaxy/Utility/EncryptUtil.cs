using Cedar.Configuration;
//using DataGeneration.DbOperations;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Tests.Galaxy.Utility
{
    /// <summary>
    /// Utility class for Encryption related methods
    /// </summary>
    public static class EncryptUtil
    {
        /// <summary>
        /// Size of the regular byte in bits
        /// </summary>
        private const int InByteSize = 8;

        /// <summary>
        /// Size of converted byte in bits
        /// </summary>
        private const int OutByteSize = 5;

        /// <summary>
        /// Base32 Alphabet
        /// </summary>
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        /// <summary>
        /// Convert byte array to Base32 format string
        /// </summary>
        /// <param name="bytes">An array of bytes to convert to Base32 format</param>
        /// <param name="toBePadded">Bool parameter to indicate if base32 encoding to be padded, defaults to true</param>
        /// <returns>Returns a string representing byte array</returns>
        private static string ToBase32String(byte[] bytes, bool toBePadded = true)
        {
            // Check if byte array is null
            if (bytes == null)
            {
                return null;
            }
            // Check if empty
            else if (bytes.Length == 0)
            {
                return string.Empty;
            }

            // Prepare container for the final value
            StringBuilder builder = new StringBuilder(bytes.Length * InByteSize / OutByteSize);

            // Position in the input buffer
            int bytesPosition = 0;

            // Offset inside a single byte that <bytesPosition> points to (from left to right)
            // 0 - highest bit, 7 - lowest bit
            int bytesSubPosition = 0;

            // Byte to look up in the dictionary
            byte outputBase32Byte = 0;

            // The number of bits filled in the current output byte
            int outputBase32BytePosition = 0;

            // Iterate through input buffer until we reach past the end of it
            while (bytesPosition < bytes.Length)
            {
                // Calculate the number of bits we can extract out of current input byte to fill missing bits in the output byte
                int bitsAvailableInByte = Math.Min(InByteSize - bytesSubPosition, OutByteSize - outputBase32BytePosition);

                // Make space in the output byte
                outputBase32Byte <<= bitsAvailableInByte;

                // Extract the part of the input byte and move it to the output byte
                outputBase32Byte |= (byte)(bytes[bytesPosition] >> (InByteSize - (bytesSubPosition + bitsAvailableInByte)));

                // Update current sub-byte position
                bytesSubPosition += bitsAvailableInByte;

                // Check overflow
                if (bytesSubPosition >= InByteSize)
                {
                    // Move to the next byte
                    bytesPosition++;
                    bytesSubPosition = 0;
                }

                // Update current base32 byte completion
                outputBase32BytePosition += bitsAvailableInByte;

                // Check overflow or end of input array
                if (outputBase32BytePosition >= OutByteSize)
                {
                    // Drop the overflow bits
                    outputBase32Byte &= 0x1F;  // 0x1F = 00011111 in binary

                    // Add current Base32 byte and convert it to character
                    builder.Append(Base32Alphabet[outputBase32Byte]);

                    // Move to the next byte
                    outputBase32BytePosition = 0;
                }
            }

            // Check if we have a remainder
            if (outputBase32BytePosition > 0)
            {
                // Move to the right bits
                outputBase32Byte <<= (OutByteSize - outputBase32BytePosition);

                // Drop the overflow bits
                outputBase32Byte &= 0x1F;  // 0x1F = 00011111 in binary

                // Add current Base32 byte and convert it to character
                builder.Append(Base32Alphabet[outputBase32Byte]);
            }

            return toBePadded ? AddPaddingToBase32(builder.ToString()) : builder.ToString();
        }

        /// <summary>
        /// Adds padding to a base32 string
        /// </summary>
        /// <param name="inBase32">base32 string</param>
        /// <returns>Padded base32 string</returns>
        private static string AddPaddingToBase32(string inBase32)
        {
            var result = new StringBuilder(inBase32);
            int padding = 8 - (result.Length % 8);
            if (padding > 0) result.Append('=', padding == 8 ? 0 : padding);

            return result.ToString();
        }

        /// <summary>
        /// Method to Get security keys for HmacSHA256 encryption
        /// </summary>
        /// <returns>Returns security key based on the environment</returns>
        private static string GetHmacSecurityKey()
        {
            switch (TestConfiguration.BaseURL)
            {
                case "https://dev-mobile-ig.participantportal.com/dev":
                    return "dNyVtTOizCfDPmGnCMVBJube8VGu+bv13ib6kDNDQ6o=";
                case "https://dev-mobile-ig.participantportal.com/uat":
                    return "dNyVtTOizCfDPmGnCMVBJube8VGu+bv13ib6kDNDQ6o=";
                case "https://dev-mobile-ig.participantportal.com/dev/stage":
                    return "dNyVtTOizCfDPmGnCMVBJube8VGu+bv13ib6kDNDQ6o=";
                default:
                    return "dNyVtTOizCfDPmGnCMVBJube8VGu+bv13ib6kDNDQ6o=";
            }
        }

        /// <summary>
        /// Converts the string to Hash Code based on environment
        /// </summary>
        /// <param name="encodedKey">The Base32 encoded HMAC Encryption key</param>
        /// <returns>Converted to HashCode key as a string</returns>
        private static string ConvertToHashCode(string encodedKey)
        {
            int hashCodedKey = CalculateHashCode(encodedKey);
            return hashCodedKey.ToString();     
        }

        /// <summary>
        /// Calculates Java Hash Code for a string
        /// </summary>
        /// <param name="key">string to be hash coded</param>
        /// <returns>hash code of the input string</returns>
        private static int CalculateHashCode(string key)
        { 
            int hashCode = 0;
            foreach (var c in key)
            {
                hashCode = hashCode * 31 + c;
            }
            return hashCode;
        } 

        /// <summary>
        /// Method to convert a string to SHA256 encrypted string
        /// </summary>
        /// <param name="input">Input to be converted to SHA256</param>
        /// <returns>SHA256 encrypted string</returns>
        public static string ComputeHmacHash(string input)
        {
            var encoding = new ASCIIEncoding();
            var keyArray = encoding.GetBytes(GetHmacSecurityKey());
            var inputByte = encoding.GetBytes(input);

            var hash = new HMACSHA256(keyArray);
            var encryptedArray = hash.ComputeHash(inputByte);

            var base32EncodedString = ToBase32String(encryptedArray);
            var base32EncodedHashCode = ConvertToHashCode(base32EncodedString);
            return base32EncodedHashCode;
        }

        /// <summary>
        /// Method to encrypt based on Obfuscate Proc and Encode to base32
        /// </summary>
        /// <param name="input">Input string to be encrypted</param>
        /// <returns>Returns encrypted string through Obfuscate and encode</returns>
        /*public static string ObfuscateEncode(string input)
        {
            //Procedure to get Obfuscated Id
            var obfuscatedString = new Obfuscate().Execute(input);

            //Conversion to encode Obfuscated Id to encrypt
            byte[] obfuscatedBytes = Encoding.ASCII.GetBytes(obfuscatedString);
            var encryptedId = ToBase32String(obfuscatedBytes);

            return encryptedId;
        }*/
    }
}
