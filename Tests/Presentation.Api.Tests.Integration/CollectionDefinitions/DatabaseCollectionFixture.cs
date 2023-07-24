using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Presentation.Api.Tests.Integration.CollectionDefinitions;

[CollectionDefinition(nameof(DatabaseFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DatabaseFixture>
{
}