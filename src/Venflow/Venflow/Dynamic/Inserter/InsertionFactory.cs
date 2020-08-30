using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionFactory<TEntity> where TEntity : class, new()
    {
        private Func<NpgsqlConnection, List<TEntity>, Task<int>>? _defaultInserter;
        private Func<NpgsqlConnection, List<TEntity>, Task<int>>? _relationInserter;
        private Func<NpgsqlConnection, List<TEntity>, Task<int>>? _identityInserter;

        private readonly Entity<TEntity> _entity;

        internal InsertionFactory(Entity<TEntity> entity)
        {
            _entity = entity;
        }

        internal Func<NpgsqlConnection, List<TEntity>, Task<int>> GetOrCreateInserter(InsertOptions insertOptions)
        {
            if (_entity.Relations is { })
            {
                if (insertOptions == InsertOptions.PopulateRelations)
                {
                    if (_relationInserter is null)
                    {
                        var sourceCompiler = new InsertionSourceCompiler();

                        sourceCompiler.RootCompile(_entity);

                        return _relationInserter = new InsertionFactoryCompiler<TEntity>(_entity).CreateInserter(sourceCompiler.GenerateSortedEntities(), insertOptions);
                    }
                    else
                    {
                        return _relationInserter;
                    }
                }
                else if (insertOptions == InsertOptions.SetIdentityColumns)
                {
                    if (_identityInserter is null)
                    {
                        var sourceCompiler = new InsertionSourceCompiler();

                        sourceCompiler.RootCompile(_entity);

                        return _identityInserter = new InsertionFactoryCompiler<TEntity>(_entity).CreateInserter(sourceCompiler.GenerateSortedEntities(), insertOptions);
                    }
                    else
                    {
                        return _identityInserter;
                    }
                }
                else
                {
                    if (_defaultInserter is null)
                    {
                        return _defaultInserter = new InsertionFactoryCompiler<TEntity>(_entity).CreateInserter(new EntityRelationHolder[] { new EntityRelationHolder(_entity) }, insertOptions);
                    }
                    else
                    {
                        return _defaultInserter;
                    }
                }
            }
            else
            {
                // TODO: Create Single Inserter

                if (insertOptions == InsertOptions.None)
                {
                    if (_defaultInserter is null)
                    {
                        return _defaultInserter = new InsertionFactoryCompiler<TEntity>(_entity).CreateInserter(new EntityRelationHolder[] { new EntityRelationHolder(_entity) }, insertOptions);
                    }
                    else
                    {
                        return _defaultInserter;
                    }
                }
                else
                {
                    if (_identityInserter is null)
                    {
                        var sourceCompiler = new InsertionSourceCompiler();

                        sourceCompiler.RootCompile(_entity);

                        return _identityInserter = new InsertionFactoryCompiler<TEntity>(_entity).CreateInserter(sourceCompiler.GenerateSortedEntities(), insertOptions);
                    }
                    else
                    {
                        return _identityInserter;
                    }
                }
            }
        }
    }
}
