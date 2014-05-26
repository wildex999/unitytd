//32 bit Fixed Point(64-bit internally to avoid overflow on large multiply)
//We use this for deterministic movement calculations on network games.
//Copied mostly from http://stackoverflow.com/questions/605124/fixed-point-math-in-c/616015#616015

//NOTE: The lack of Float to FInt is on purpose, as that would not be deterministic!!!

using System;
public struct FInt
{
    public long RawValue;
    public const int SHIFT_AMOUNT = 12; //12 is 4096, so we have 3 full digits(Can't we also get that by 1024(8)?)

    public const long One = 1 << SHIFT_AMOUNT;
    public const int OneI = 1 << SHIFT_AMOUNT;
    public static FInt OneF = FInt.Create(1, true);

    #region Constructors
    public static FInt Create(long startingRawValue, bool useMultiple)
    {
        FInt fInt;
        fInt.RawValue = startingRawValue;
        if (useMultiple)
            fInt.RawValue = fInt.RawValue << SHIFT_AMOUNT;
        return fInt;
    }
    #endregion

    public int ToInt()
    {
        return (int)(this.RawValue >> SHIFT_AMOUNT);
    }

    public double ToDouble()
    {
        return (double)this.RawValue / (double)One;
    }

    public float ToFloat()
    {
        return (float)this.RawValue / (float)One;
    }

    public static FInt FromParts(int preDecimal, int postDecimal)
    {
        FInt fInt = FInt.Create(preDecimal, true);
        if (postDecimal != 0)
            fInt.RawValue += (FInt.Create(postDecimal*One, false) / 1000).RawValue;

        return fInt;
    }

    #region *
    public static FInt operator *(FInt one, FInt other)
    {
        FInt fInt;
        fInt.RawValue = (one.RawValue * other.RawValue) >> SHIFT_AMOUNT;
        return fInt;
    }

   /* public static FInt operator *(FInt one, int multi)
    {
        return one * (FInt)multi;
    }

    public static FInt operator *(int multi, FInt one)
    {
        return one * (FInt)multi;
    }*/
    #endregion

    #region /
    public static FInt operator /(FInt one, FInt other)
    {
        FInt fInt;
        fInt.RawValue = (one.RawValue << SHIFT_AMOUNT) / (other.RawValue);
        return fInt;
    }

   /* public static FInt operator /(FInt one, int divisor)
    {
        return one / (FInt)divisor;
    }

    public static FInt operator /(int divisor, FInt one)
    {
        return (FInt)divisor / one;
    }*/
    #endregion

    #region +
    public static FInt operator +(FInt one, FInt other)
    {
        FInt fInt;
        fInt.RawValue = one.RawValue + other.RawValue;
        return fInt;
    }

  /*  public static FInt operator +(FInt one, int other)
    {
        return one + (FInt)other;
    }

    public static FInt operator +(int other, FInt one)
    {
        return one + (FInt)other;
    }*/
    #endregion

    #region -
    public static FInt operator -(FInt one, FInt other)
    {
        FInt fInt;
        fInt.RawValue = one.RawValue - other.RawValue;
        return fInt;
    }

   /* public static FInt operator -(FInt one, int other)
    {
        return one - (FInt)other;
    }

    public static FInt operator -(int other, FInt one)
    {
        return (FInt)other - one;
    }*/
    #endregion

    #region --
    public static FInt operator --(FInt one)
    {
        return one - 1;
    }
    #endregion

    #region ++
    public static FInt operator ++(FInt one)
    {
        return one + 1;
    }
    #endregion

    #region ==
    public static bool operator ==(FInt one, FInt other)
    {
        return one.RawValue == other.RawValue;
    }

 /*   public static bool operator ==(FInt one, int other)
    {
        return one == (FInt)other;
    }

    public static bool operator ==(int other, FInt one)
    {
        return (FInt)other == one;
    }*/
    #endregion

    #region !=
    public static bool operator !=(FInt one, FInt other)
    {
        return one.RawValue != other.RawValue;
    }

  /*  public static bool operator !=(FInt one, int other)
    {
        return one != (FInt)other;
    }

    public static bool operator !=(int other, FInt one)
    {
        return (FInt)other != one;
    }*/
    #endregion

    #region >=
    public static bool operator >=(FInt one, FInt other)
    {
        return one.RawValue >= other.RawValue;
    }

   /* public static bool operator >=(FInt one, int other)
    {
        return one >= (FInt)other;
    }

    public static bool operator >=(int other, FInt one)
    {
        return (FInt)other >= one;
    }*/
    #endregion

    #region <=
    public static bool operator <=(FInt one, FInt other)
    {
        return one.RawValue <= other.RawValue;
    }

   /* public static bool operator <=(FInt one, int other)
    {
        return one <= (FInt)other;
    }

    public static bool operator <=(int other, FInt one)
    {
        return (FInt)other <= one;
    }*/
    #endregion

    #region >
    public static bool operator >(FInt one, FInt other)
    {
        return one.RawValue > other.RawValue;
    }

   /* public static bool operator >(FInt one, int other)
    {
        return one > (FInt)other;
    }

    public static bool operator >(int other, FInt one)
    {
        return (FInt)other > one;
    }*/
    #endregion

    #region <
    public static bool operator <(FInt one, FInt other)
    {
        return one.RawValue < other.RawValue;
    }

   /* public static bool operator <(FInt one, int other)
    {
        return one < (FInt)other;
    }

    public static bool operator <(int other, FInt one)
    {
        return (FInt)other < one;
    }*/
    #endregion

    #region cast
    public static explicit operator int(FInt src)
    {
        return (int) (src.RawValue >> SHIFT_AMOUNT);
    }

    public static explicit operator float(FInt src)
    {
        return src.ToFloat();
    }

    public static implicit operator FInt(int src) //Allow int to FInt implicit, as there is no loss
    {
        return FInt.Create(src, true);
    }

    public static explicit operator FInt(long src)
    {
        return FInt.Create(src, true);
    }

    public static explicit operator FInt(ulong src)
    {
        return FInt.Create((long)src, true);
    }
    #endregion

    #region Sqrt
    public static FInt Sqrt(FInt f, int NumberOfIterations)
    {
        if (f.RawValue < 0) //NaN in Math.Sqrt
            throw new ArithmeticException("Input Error");
        if (f.RawValue == 0)
            return (FInt)0;
        FInt k = f + FInt.OneF >> 1;
        for (int i = 0; i < NumberOfIterations; i++)
            k = (k + (f / k)) >> 1;

        if (k.RawValue < 0)
            throw new ArithmeticException("Overflow");
        else
            return k;
    }

    public static FInt Sqrt(FInt f)
    {
        byte numberOfIterations = 8;
        if (f.RawValue > 0x64000)
            numberOfIterations = 12;
        if (f.RawValue > 0x3e8000)
            numberOfIterations = 16;
        return Sqrt(f, numberOfIterations);
    }
    #endregion

    public static FInt operator <<(FInt one, int amount)
    {
        return FInt.Create(one.RawValue << amount, false);
    }

    public static FInt operator >>(FInt one, int amount)
    {
        return FInt.Create(one.RawValue >> amount, false);
    }

    public override bool Equals(object obj)
    {
        if(obj is FInt)
            return ((FInt)obj).RawValue == this.RawValue;
        return false;
    }

    public override int GetHashCode()
    {
        return RawValue.GetHashCode();
    }

    public override string ToString()
    {
        return RawValue + "(" + (RawValue >> SHIFT_AMOUNT) + "." + ((RawValue - ((RawValue >> SHIFT_AMOUNT) << SHIFT_AMOUNT))/4096) + ")";
    }
}