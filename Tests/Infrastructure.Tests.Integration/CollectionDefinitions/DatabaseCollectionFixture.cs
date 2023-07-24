using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.CollectionDefinitions;

[CollectionDefinition(nameof(DatabaseFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DatabaseFixture>
{
}