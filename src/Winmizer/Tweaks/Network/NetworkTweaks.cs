using Winmizer.Core;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.Network;

public static class NetworkTweaks
{
    private const string Interfaces = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.Network;

        yield return new RegistryTweak("network.nagle", "Wyłącz algorytm Nagle'a (niższy ping)",
            "TcpAckFrequency=1 i TCPNoDelay=1 na każdym interfejsie - pakiety gier wysyłane od razu. Wpływ na ping jest niewielki; nie rusza Wi-Fi ani sterowników.",
            c, RiskLevel.Moderate,
            () => Reg.SubKeys(Interfaces).SelectMany(g => new[]
            {
                Dw($@"{Interfaces}\{g}", "TcpAckFrequency", 1),
                Dw($@"{Interfaces}\{g}", "TCPNoDelay", 1)
            }));

        yield return new RegistryTweak("network.dop2p", "Delivery Optimization: wyłącz udostępnianie P2P",
            "DODownloadMode=0 - aktualizacje pobierane tylko z serwerów Microsoft (bez wysyłania ich innym komputerom). Windows Update działa normalnie.",
            c, RiskLevel.Safe,
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0)) { Recommended = true };

        yield return new CloudflareDnsTweak();
    }
}

public sealed class CloudflareDnsTweak : TweakBase
{
    public CloudflareDnsTweak() : base("network.dns", "DNS Cloudflare (1.1.1.1 / 1.0.0.1)",
        "Ustawia szybki publiczny DNS na aktywnych kartach sieciowych. Poprzednie ustawienia są zapamiętane.",
        TweakCategory.Network, RiskLevel.Advanced)
    {
        Warning = "Zmienia dostawcę DNS - w sieciach firmowych/z filtrowaniem rodzicielskim może coś zablokować.";
    }

    public override bool IsApplied()
    {
        var ad = DnsHelper.Adapters();
        return ad.Count > 0 && ad.All(a => a.Dns == string.Join(",", DnsHelper.Cloudflare));
    }

    public override void Apply()
    {
        var ad = DnsHelper.Adapters();
        if (ad.Count == 0) throw new TweakSkippedException("Brak aktywnych kart sieciowych.");
        foreach (var a in ad)
        {
            Ctx.Backup.Record(Id, new BackupEntry { Kind = "dns", Key = a.Alias, Value = a.Dns });
            Ctx.Backup.Save();
            DnsHelper.Set(a.Alias, string.Join(",", DnsHelper.Cloudflare));
        }
    }
}
