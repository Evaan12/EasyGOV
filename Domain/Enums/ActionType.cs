using System;

namespace Domain.Enums
{
    [Flags]
    public enum ActionType : int
    {
        None = 0,
        Read = 1 << 0,
        Create = 1 << 1,
        Update = 1 << 2,
        Delete = 1 << 3,
        Export = 1 << 4,
        Approve = 1 << 5,
        Assign = 1 << 6,
        Admin = 1 << 7,
        Revoke = 1 << 8,
        Activate = 1 << 9
    }
}