using System;
using System.Security.Cryptography;

public class OTPGenerator
{
    private HMACSHA1 hmac;
    private byte[] buffer = new byte[8]; // Buffer size should match your HMAC size
    private int modDivisor;

    public OTPGenerator(int modDivisor)
    {
        this.modDivisor = modDivisor;
        hmac = new HMACSHA1();
    }

    public int GenerateOneTimePassword(string password, ulong counter)
    {
        byte[] key = GenerateKey(password);
        hmac.Key = key;

        buffer[0] = (byte)((counter & 0xff00000000000000L) >> 56);
        buffer[1] = (byte)((counter & 0x00ff000000000000L) >> 48);
        buffer[2] = (byte)((counter & 0x0000ff0000000000L) >> 40);
        buffer[3] = (byte)((counter & 0x000000ff00000000L) >> 32);
        buffer[4] = (byte)((counter & 0x00000000ff000000L) >> 24);
        buffer[5] = (byte)((counter & 0x0000000000ff0000L) >> 16);
        buffer[6] = (byte)((counter & 0x000000000000ff00L) >> 8);
        buffer[7] = (byte)(counter & 0x00000000000000ffL);

        byte[] macResult = hmac.ComputeHash(buffer);

        int offset = macResult[macResult.Length - 1] & 0x0F;

        int truncatedResult = ((macResult[offset] & 0x7F) << 24 | (macResult[offset + 1] & 0xFF) << 16
                | (macResult[offset + 2] & 0xFF) << 8 | (macResult[offset + 3] & 0xFF));

        switch (modDivisor)
        {
            case 6:
                {
                    modDivisor = 1_000_000;
                    break;
                }

            case 7:
                {
                    modDivisor = 10_000_000;
                    break;
                }

            case 8:
                {
                    modDivisor = 100_000_000;
                    break;
                }
        }
        return truncatedResult % modDivisor;
    }

    private byte[] GenerateKey(string password)
    {
        using (var sha1 = SHA1.Create())
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            return sha1.ComputeHash(passwordBytes);
        }
    }
}
