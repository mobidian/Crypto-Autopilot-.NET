using Strategies.LuxAlgoImbalance.Enums;

namespace Strategies.LuxAlgoImbalance.Models;

public record struct LuxAlgoFVG
{
    public required FvgSide Side;
    public required decimal Top;
    public required decimal Bottom;

    public decimal Middle => (this.Top + this.Bottom) / 2.0m;
}
