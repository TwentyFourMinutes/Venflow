using System.Collections.Generic;
using Venflow.Modeling;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.DatabaseTests
{
    public class RelationMapping
    {
        [Fact]
        public void ModelBuilding()
        {
            var relationalDb = new RelationDatabase();

            VerifyModelBuildingOrder(relationalDb.Entities, false);
        }

        [Fact]
        public void ReverseModelBuilding()
        {
            var reverseRelationDb = new ReverseRelationDatabase();

            VerifyModelBuildingOrder(reverseRelationDb.Entities, true);
        }

        private void VerifyModelBuildingOrder(IReadOnlyDictionary<string, Entity> entities, bool isReverse)
        {
            Assert.True(entities.TryGetValue((isReverse ? "Reverse" : string.Empty) + "Person", out var entity));
            Assert.NotNull(entity.Relations);
            Assert.Equal(1, entity.Relations.Count);

            var relation = entity.Relations[0];

            Assert.Equal(Enums.ForeignKeyLocation.Right, relation.ForeignKeyLocation);
            Assert.Equal(entity, relation.LeftEntity);
            Assert.Equal((isReverse ? "Reverse" : string.Empty) + "Email", relation.RightEntity.EntityName);

            var emailEntity = relation.RightEntity;
            Assert.NotNull(emailEntity.Relations);
            Assert.Equal(2, emailEntity.Relations.Count);

            relation = emailEntity.Relations[0];
            var relationTwo = emailEntity.Relations[1];

            if (relation.RightEntity == entity)
            {
                Assert.Equal(Enums.ForeignKeyLocation.Left, relation.ForeignKeyLocation);
                Assert.Equal((isReverse ? "Reverse" : string.Empty) + "Person", relation.RightEntity.EntityName);

                Assert.Equal(Enums.ForeignKeyLocation.Right, relationTwo.ForeignKeyLocation);
                Assert.Equal((isReverse ? "Reverse" : string.Empty) + "EmailContent", relationTwo.RightEntity.EntityName);

                relation = relationTwo;
            }
            else
            {
                Assert.Equal(Enums.ForeignKeyLocation.Left, relationTwo.ForeignKeyLocation);
                Assert.Equal((isReverse ? "Reverse" : string.Empty) + "Person", relationTwo.RightEntity.EntityName);

                Assert.Equal(Enums.ForeignKeyLocation.Right, relation.ForeignKeyLocation);
                Assert.Equal((isReverse ? "Reverse" : string.Empty) + "EmailContent", relation.RightEntity.EntityName);
            }

            var emailContentEntity = relation.RightEntity;
            Assert.NotNull(emailContentEntity.Relations);
            Assert.Equal(1, emailContentEntity.Relations.Count);

            relation = emailContentEntity.Relations[0];

            Assert.Equal(Enums.ForeignKeyLocation.Left, relation.ForeignKeyLocation);
            Assert.Equal(emailContentEntity, relation.LeftEntity);
            Assert.Equal(emailEntity, relation.RightEntity);
        }
    }
}
