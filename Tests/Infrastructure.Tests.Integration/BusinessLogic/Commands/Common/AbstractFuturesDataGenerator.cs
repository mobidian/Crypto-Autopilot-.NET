using System.Collections;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Common.Fakers;
using Infrastructure.Tests.Integration.DataAccess.Extensions;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.Common;

public abstract class AbstractFuturesDataGenerator : FuturesDataFakersClass, IEnumerable<object[]>
{
    protected readonly string limitOrderNotFilledRule = $"{OrderType.Limit.ToRuleSetName()}";

    protected readonly string marketOrderRule = OrderType.Market.ToRuleSetName();
    protected readonly string limitFilledRule = $"{OrderType.Limit.ToRuleSetName()}, {OrderStatus.Filled.ToRuleSetName()}";

    //// //// ////

    protected readonly List<string> positionSideRules = Enum.GetValues<PositionSide>().Except(new[] { PositionSide.None }).Select(x => x.ToRuleSetName()).ToList();
    protected readonly List<string> orderSideRules = Enum.GetValues<OrderSide>().Select(x => x.ToRuleSetName()).ToList();

    //// //// ////

    public abstract IEnumerator<object[]> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
