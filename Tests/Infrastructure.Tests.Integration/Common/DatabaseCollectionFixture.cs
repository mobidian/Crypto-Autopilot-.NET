using Xunit;

namespace Infrastructure.Tests.Integration.AbstractBases;

[CollectionDefinition(nameof(DatabaseFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DatabaseFixture>
{
}
