using System.Diagnostics;
using System.Text.Json.Serialization;

using CryptoExchange.Net.CommonObjects;

namespace Domain.Models;

[DebuggerDisplay("{Name}")]
public class CurrencyPair : ICloneable
{
    public string Name { get; }

    [JsonConstructor]
    public CurrencyPair(string Name)
    {
        this.Name = Name ?? throw new ArgumentNullException(nameof(Name));
    }
    public CurrencyPair(string Base, string Quote)
    {
        _ = Base ?? throw new ArgumentNullException(nameof(Base));
        _ = Quote ?? throw new ArgumentNullException(nameof(Quote));
        
        this.Name = Base + Quote;
    }

    ////

    public object Clone() => this.MemberwiseClone();

    #region overrides
    public override string ToString() => this.Name;
    public override bool Equals(object? obj) => obj is CurrencyPair pair && this.Name == pair.Name;
    public override int GetHashCode() => this.Name.GetHashCode();
    #endregion

    #region operator overloading
    public static implicit operator CurrencyPair(string Name) => new CurrencyPair(Name);

    public static bool operator ==(CurrencyPair pair1, CurrencyPair pair2) => pair1.Name == pair2.Name;
    public static bool operator !=(CurrencyPair pair1, CurrencyPair pair2) => pair1.Name != pair2.Name;

    public static bool operator ==(CurrencyPair pair, Symbol symbol) => pair.Name == symbol.Name;
    public static bool operator !=(CurrencyPair pair, Symbol symbol) => pair.Name != symbol.Name;
    public static bool operator ==(Symbol symbol, CurrencyPair pair) => pair.Name == symbol.Name;
    public static bool operator !=(Symbol symbol, CurrencyPair pair) => pair.Name != symbol.Name;

    public static bool operator ==(CurrencyPair pair, string symbol) => pair.Name == symbol;
    public static bool operator !=(CurrencyPair pair, string symbol) => pair.Name != symbol;
    public static bool operator ==(string symbol, CurrencyPair pair) => pair.Name == symbol;
    public static bool operator !=(string symbol, CurrencyPair pair) => pair.Name != symbol;
    #endregion
}
