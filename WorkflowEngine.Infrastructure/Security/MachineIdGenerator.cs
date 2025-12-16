using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.Infrastructure.Security;

public class MachineIdGenerator : IMachineIdGenerator
{
    public string GetMachineId()
    {
        var sb = new StringBuilder();

        // 1. Host Name
        sb.Append(Environment.MachineName);
        sb.Append('|');

        // 2. OS Version
        sb.Append(Environment.OSVersion);
        sb.Append('|');

        // 3. MAC Address (First operational interface)
        var macAddress = GetMacAddress();
        sb.Append(macAddress);

        return ComputeSha256Hash(sb.ToString());
    }

    private string GetMacAddress()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var firstInterface = interfaces
                .OrderBy(i => i.Name) // Consistent ordering
                .FirstOrDefault(i =>
                    i.OperationalStatus == OperationalStatus.Up &&
                    i.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    i.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

            if (firstInterface != null)
            {
                return firstInterface.GetPhysicalAddress().ToString();
            }
        }
        catch
        {
            // Fallback or ignore if permission issues
        }

        return "UNKNOWN_MAC";
    }

    private string ComputeSha256Hash(string rawData)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
