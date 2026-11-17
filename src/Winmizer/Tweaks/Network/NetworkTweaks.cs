using Winmizer.Core;
using Winmizer.Core.Localization;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.Network;

public static class NetworkTweaks
{
    private const string Interfaces = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.Network;

        yield return new RegistryTweak("network.nagle", "Disable Nagle's algorithm (lower ping)",
            "TcpAckFrequency=1 and TCPNoDelay=1 on every interface - game packets are sent immediately. The effect on ping is small; doesn't touch Wi-Fi or drivers.",
            c, RiskLevel.Moderate,
            () => Reg.SubKeys(Interfaces).SelectMany(g => new[]
            {
                Dw($@"{Interfaces}\{g}", "TcpAckFrequency", 1),
                Dw($@"{Interfaces}\{g}", "TCPNoDelay", 1)
            }));

        yield return new RegistryTweak("network.dop2p", "Delivery Optimization: disable P2P sharing",
            "DODownloadMode=0 - updates are downloaded only from Microsoft servers (and not uploaded to other PCs). Windows Update works normally.",
            c, RiskLevel.Safe,
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0)) { Recommended = true };

        yield return new CloudflareDnsTweak();
    }
}

public sealed class CloudflareDnsTweak : TweakBase
{
    public CloudflareDnsTweak() : base("network.dns", "Cloudflare DNS (1.1.1.1 / 1.0.0.1)",
        "Sets a fast public DNS on active network adapters. Previous settings are remembered.",
        TweakCategory.Network, RiskLevel.Advanced)
    {
        Warning = "Changes your DNS provider - on corporate or parental-filtered networks it may block something.";
    }

    public override bool IsApplied()
    {
        var ad = DnsHelper.Adapters();
        return ad.Count > 0 && ad.All(a => a.Dns == string.Join(",", DnsHelper.Cloudflare));
    }

    public override void Apply()
    {
        var ad = DnsHelper.Adapters();
        if (ad.Count == 0) throw new TweakSkippedException(L.T("No active network adapters."));
        foreach (var a in ad)
        {
            Ctx.Backup.Record(Id, new BackupEntry { Kind = "dns", Key = a.Alias, Value = a.Dns });
            Ctx.Backup.Save();
            DnsHelper.Set(a.Alias, string.Join(",", DnsHelper.Cloudflare));
        }
    }
}
