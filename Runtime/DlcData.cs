#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
#if FACEPUNCH
using Steamworks.Data;
#endif
using System;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    public struct DlcData
    {
        public AppId_t AppId { get; private set; }
        public bool Available { get; private set; }
        public string Name { get; private set; }

        public DlcData(AppId_t id, bool available, string name)
        {
            AppId = id;
            Available = available;
            Name = name;
        }
    }
#elif FACEPUNCH
    public struct DlcData : IEquatable<Steamworks.Data.DlcInformation>, IEquatable<AppId>
    {
        public AppId AppId { get; private set; }
        public bool Available { get; private set; }
        public string Name { get; private set; }

        public DlcData(AppId id, bool available, string name)
        {
            AppId = id;
            Available = available;
            Name = name;
        }

        public bool Equals(DlcInformation other)
        {
            return AppId.Equals(other.AppId);
        }

        public override bool Equals(object obj)
        {
            return AppId.Equals(obj);
        }

        public override int GetHashCode()
        {
            return AppId.GetHashCode() + Name.GetHashCode() + Available.GetHashCode();
        }

        public bool Equals(AppId other)
        {
            return AppId.Equals(other);
        }

        public static bool operator ==(DlcData l, DlcInformation r) => l.AppId == r.AppId;
        public static bool operator ==(DlcData l, AppId r) => l.AppId == r;
        public static bool operator ==(DlcInformation l, DlcData r) => l.AppId == r.AppId;
        public static bool operator ==(AppId l, DlcData r) => l == r.AppId;
        public static bool operator !=(DlcData l, DlcInformation r) => l.AppId != r.AppId;
        public static bool operator !=(DlcData l, AppId r) => l.AppId != r;
        public static bool operator !=(DlcInformation l, DlcData r) => l.AppId != r.AppId;
        public static bool operator !=(AppId l, DlcData r) => l != r.AppId;

        public static implicit operator DlcData(DlcInformation c) => new DlcData(c.AppId, c.Available, c.Name);
    }
#endif
}
#endif