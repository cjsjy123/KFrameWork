using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;

public struct IgnoreLowString :IEqualityComparer<IgnoreLowString>,IEquatable<IgnoreLowString>, IEquatable<string>, IEqualityComparer<string>
{

    private string origin ;

    public bool Equals(string other)
    {
        return this.origin.IgnoreUpOrlower(other);
    }

    public bool Equals(IgnoreLowString other)
    {
        if (other.origin == null)
            return false;

        return this.origin.IgnoreUpOrlower(other.origin);
    }

    public bool Equals(string x, string y)
    {
        return x.IgnoreUpOrlower(y);
    }

    public bool Equals(IgnoreLowString x, IgnoreLowString y)
    {
        if (x.origin == null && y.origin == null)
            return true;

        if (x.origin == null && y.origin != null)
            return false;

        if (x.origin != null && y.origin == null)
            return false;

        return x.origin.IgnoreUpOrlower(y.origin);
    }

    public int GetHashCode(string obj)
    {
        return FrameWorkTools.GetHashCode(obj);
    }

    public int GetHashCode(IgnoreLowString obj)
    {
        return obj.origin.GetHashCode();
    }

    public override int GetHashCode()
    {
        return origin.GetHashCode();
    }

    public override string ToString()
    {
        return origin;
    }

    public static implicit operator IgnoreLowString(string other)
    {
        IgnoreLowString s = new IgnoreLowString();
        s.origin = other;
        return s;
    }
}
