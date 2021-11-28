using System.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Sections;
using Reflow.Analyzer.Sections.LambdaSorter;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal class QueryParserEmitter
    {
        private ushort _parserIndex;

        private readonly string _className;
        private readonly Database _database;
        private readonly List<Query> _queries;
        private readonly List<MethodDeclarationSyntax> _parsers;
        private readonly Dictionary<Query, MethodLocation> _parserCache;
        private readonly List<SwitchSectionSyntax> _cases;
        private readonly List<StatementSyntax> _afterCases;
        private readonly List<StatementSyntax> _caseStatements;
        private readonly List<StatementSyntax> _localSyntaxis;
        private readonly List<StatementSyntax> _iterationLocalSyntaxis;
        private readonly List<StatementSyntax> _miscStatements;

        private QueryParserEmitter(Database database, List<Query> queries)
        {
            _database = database;
            _queries = queries;
            _className = $"__{database.Symbol.GetFullName().Replace('.', '_')}";
            _parsers = new(queries.Count);
            _parserCache = new(queries.Count, QueryEqualityComparer.Default);
            _cases = new();
            _afterCases = new();
            _caseStatements = new();
            _localSyntaxis = new();
            _iterationLocalSyntaxis = new();
            _miscStatements = new();
        }

        private SourceText Build()
        {
            for (var queryIndex = 0; queryIndex < _queries.Count; queryIndex++)
            {
                var query = _queries[queryIndex];

                if (_parserCache.TryGetValue(query, out var methodLocation))
                {
                    ((QueryLinkData)query.FluentCall.LambdaLink.Data!).Location = methodLocation;
                    continue;
                }

                if (query.Type.HasFlag(QueryType.Single))
                {
                    if (query.Type.HasFlag(QueryType.WithRelations))
                    {
                        methodLocation = BuildSingleWithRelationParser(query);
                    }
                    else
                    {
                        methodLocation = BuildSingleNoRelationParser(query);
                    }
                }
                else if (query.Type.HasFlag(QueryType.Many))
                {
                    if (query.Type.HasFlag(QueryType.WithRelations)) { }
                    else
                    {
                        methodLocation = BuildManyNoRelationParser(query);
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }

                ((QueryLinkData)query.FluentCall.LambdaLink.Data!).Location = methodLocation;

                _parserCache.Add(query, methodLocation);
                _cases.Clear();
                _afterCases.Clear();
                _localSyntaxis.Clear();
                _iterationLocalSyntaxis.Clear();
            }

            return File("Reflow.QueryParsers")
                .WithMembers(
                    Class(_className, CSharpModifiers.Internal | CSharpModifiers.Static)
                        .WithMembers(_parsers)
                )
                .GetText();
        }

        private MethodLocation BuildSingleNoRelationParser(Query query)
        {
            var entity = _database.Entities[query.Entity];

            for (var columnIndex = 0; columnIndex < entity.Columns.Count; columnIndex++)
            {
                var column = entity.Columns[columnIndex];

                if (columnIndex == 0)
                {
                    _caseStatements.Add(
                        Statement(AssignLocal(Variable("entity"), Instance(Type(entity.Symbol))))
                    );
                }

                _caseStatements.Add(
                    AssignMember(
                        Variable("entity"),
                        column.PropertyName,
                        Invoke(
                            Variable("reader"),
                            GenericName("GetFieldValue", Type(column.Type)),
                            Variable("columnIndex")
                        )
                    )
                );
                _caseStatements.Add(Break());

                _cases.Add(Case(Constant(columnIndex), _caseStatements));
                _caseStatements.Clear();
            }

            var methodDefinition = new MethodLocation(
                "Reflow.QueryParsers." + _className,
                "Parser_" + _parserIndex++
            );

            _parsers.Add(
                Method(
                        methodDefinition.MethodName,
                        Type(query.Entity),
                        CSharpModifiers.Internal | CSharpModifiers.Static
                    )
                    .WithParameters(
                        Parameter("reader", Type(typeof(DbDataReader))),
                        Parameter("columns", Array(Type(typeof(ushort))))
                    )
                    .WithStatements(
                        Local("entity", Type(query.Entity)).WithInitializer(Default()),
                        Local("columnCount", Var())
                            .WithInitializer(AccessMember(Variable("columns"), "Length")),
                        For(
                            Local("columnIndex", Type(typeof(int))).WithInitializer(Constant(0)),
                            LessThen(Variable("columnIndex"), Variable("columnCount")),
                            Increment(Variable("columnIndex")),
                            Switch(
                                AccessElement(Variable("columns"), Variable("columnIndex")),
                                _cases
                            )
                        ),
                        Return(Variable("entity"))
                    )
            );

            return methodDefinition;
        }

        private MethodLocation BuildSingleWithRelationParser(Query query)
        {
            var virtualEntities = GetVirtualEntities(
                query.JoinedEntities,
                _database.Entities[query.Entity]
            );

            var entityChangesType = BitUtilities.GetTypeBySize(virtualEntities.Length);

            var columnOffset = 0;

            for (
                var virtualEntityIndex = 0;
                virtualEntityIndex < virtualEntities.Length;
                virtualEntityIndex++
            )
            {
                var virtualEntity = virtualEntities[virtualEntityIndex];

                var lastEntityLocalName = "lastEntity_" + virtualEntity.Id;
                _localSyntaxis.Add(
                    Local(lastEntityLocalName, Type(virtualEntity.Entity.Symbol))
                        .WithInitializer(Default())
                );

                string? entityDictionaryLocalName = null;

                if (virtualEntityIndex > 0)
                {
                    entityDictionaryLocalName = "entitiesDictionary_" + virtualEntity.Id;

                    _localSyntaxis.Add(
                        Local(entityDictionaryLocalName, Var())
                            .WithInitializer(
                                Instance(
                                    GenericType(
                                        typeof(Dictionary<,>),
                                        Type(virtualEntity.Entity.Columns[0].Type),
                                        Type(virtualEntity.Entity.Symbol)
                                    )
                                )
                            )
                    );
                }

                var entityIdLocalName = "entityId_" + virtualEntity.Id;
                _iterationLocalSyntaxis.Add(
                    Local(entityIdLocalName, Type(virtualEntity.Entity.Columns[0].Type))
                        .WithInitializer(Default())
                );

                for (
                    var columnIndex = 0;
                    columnIndex < virtualEntity.Entity.Columns.Count;
                    columnIndex++
                )
                {
                    var column = virtualEntity.Entity.Columns[columnIndex];

                    if (columnIndex == 0)
                    {
                        // Instantiate a new entity instance
                        _caseStatements.Add(
                            Statement(
                                AssignLocal(
                                    Variable(lastEntityLocalName),
                                    Instance(Type(virtualEntity.Entity.Symbol))
                                )
                            )
                        );

                        // Instantiate used collection
                        for (
                            var navigationIndex = 0;
                            navigationIndex < virtualEntity.InitializeNavigations.Count;
                            navigationIndex++
                        )
                        {
                            var navigation = virtualEntity.InitializeNavigations[navigationIndex];

                            if (navigation.IsLeftNavigationPropertyInitialized)
                                continue;

                            _caseStatements.Add(
                                AssignMember(
                                    Variable(lastEntityLocalName),
                                    navigation.LeftNavigationProperty!,
                                    Instance(
                                        GenericType(
                                            typeof(List<>),
                                            Type(navigation.RightEntitySymbol)
                                        )
                                    )
                                )
                            );
                        }

                        // Initialize relation maps
                        for (
                            var relationIndex = 0;
                            relationIndex < virtualEntity.InitializeRelationMaps.Count;
                            relationIndex++
                        )
                        {
                            var (relation, foreignEntity) = virtualEntity.InitializeRelationMaps[
                                relationIndex
                            ];

                            var lastEntityRelationsLocalName =
                                $"lastEntityRelations_{foreignEntity.Id}_{relation.Id}";
                            var entityRelationsLocalName =
                                $"entityRelations_{foreignEntity.Id}_{relation.Id}";

                            _localSyntaxis.Add(
                                Local(
                                        lastEntityRelationsLocalName,
                                        GenericType(
                                            typeof(HashSet<>),
                                            Type(relation.RightEntitySymbol)
                                        )
                                    )
                                    .WithInitializer(Null())
                            );

                            _caseStatements.Add(
                                Statement(
                                    AssignLocal(
                                        Variable(lastEntityRelationsLocalName),
                                        Instance(
                                            GenericType(
                                                typeof(HashSet<>),
                                                Type(relation.RightEntitySymbol)
                                            )
                                        )
                                    )
                                )
                            );

                            _localSyntaxis.Add(
                                Local(entityRelationsLocalName, Var())
                                    .WithInitializer(
                                        Instance(
                                            GenericType(
                                                typeof(Dictionary<,>),
                                                Type(virtualEntity.Entity.Columns[0].Type),
                                                GenericType(
                                                    typeof(HashSet<>),
                                                    Type(relation.RightEntitySymbol)
                                                )
                                            )
                                        )
                                    )
                            );

                            _caseStatements.Add(
                                Statement(
                                    Invoke(
                                        Variable(entityRelationsLocalName!),
                                        "Add",
                                        Variable(entityIdLocalName),
                                        Variable(lastEntityRelationsLocalName)
                                    )
                                )
                            );
                        }

                        // Set the starting point of the columns loop to the next entity
                        if (virtualEntityIndex == 0)
                        {
                            _caseStatements.Add(
                                Statement(
                                    AssignLocal(
                                        Variable("startColumnIndex"),
                                        Constant(columnOffset + virtualEntity.Entity.Columns.Count)
                                    )
                                )
                            );
                        }

                        // Assign the primary column from the read local
                        _caseStatements.Add(
                            AssignMember(
                                Variable(lastEntityLocalName),
                                column.PropertyName,
                                Variable(entityIdLocalName)
                            )
                        );

                        // Add self to the entity dictionary
                        if (entityDictionaryLocalName is not null)
                        {
                            _caseStatements.Add(
                                Statement(
                                    Invoke(
                                        Variable(entityDictionaryLocalName),
                                        "Add",
                                        AccessMember(
                                            Variable(lastEntityLocalName),
                                            column.PropertyName
                                        ),
                                        Variable(lastEntityLocalName)
                                    )
                                )
                            );
                        }

                        List<StatementSyntax> checkForExistingEntitiy;

                        if (virtualEntity.RequiresDBNullCheck && virtualEntityIndex > 0)
                        {
                            // Check if the id is already known
                            checkForExistingEntitiy = new List<StatementSyntax>
                            {
                                If(
                                    Not(
                                        Invoke(
                                            Variable(entityDictionaryLocalName!),
                                            "TryGetValue",
                                            Variable(entityIdLocalName),
                                            Out(Variable(lastEntityLocalName))
                                        )
                                    ),
                                    _caseStatements
                                )
                            };
                        }
                        else
                        {
                            checkForExistingEntitiy = _caseStatements;
                        }

                        // Instantiate last entity relations
                        if (virtualEntity.ForeignAssignedRelations.Count > 0)
                        {
                            for (
                                var navigationIndex = 0;
                                navigationIndex < virtualEntity.ForeignAssignedRelations.Count;
                                navigationIndex++
                            )
                            {
                                var navigation =
                                    virtualEntity.ForeignAssignedRelations[navigationIndex].Item1;

                                var lastEntityRelationsLocalName =
                                    $"lastEntityRelations_{virtualEntity.Id}_{navigation.Id}";

                                _miscStatements.Add(
                                    Statement(
                                        AssignLocal(Variable(lastEntityRelationsLocalName), Null())
                                    )
                                );
                            }

                            if (virtualEntity.RequiresDBNullCheck && virtualEntityIndex > 0)
                            {
                                checkForExistingEntitiy[0] = If(
                                        (IfStatementSyntax)checkForExistingEntitiy[0]
                                    )
                                    .Else(_miscStatements);
                            }
                            else
                            {
                                checkForExistingEntitiy.AddRange(_miscStatements);
                            }

                            _miscStatements.Clear();
                        }

                        if (virtualEntity.RequiresChangedLocal)
                        {
                            // Mark the entity as changed
                            checkForExistingEntitiy.Add(
                                Statement(SetBit(Variable("entityChanges"), virtualEntity.Id))
                            );
                        }

                        var potentialIfBody = new StatementSyntax[]
                        {
                            // Get the primary column and store it in a local
                            Statement(
                                AssignLocal(
                                    Variable(entityIdLocalName),
                                    Invoke(
                                        Variable("reader"),
                                        GenericName("GetFieldValue", Type(column.Type)),
                                        Variable("columnIndex")
                                    )
                                )
                            ),
                            // Check if the id is the same as the last or if there is no last id
                            If(
                                Or(
                                    Equal(Variable(lastEntityLocalName), Default()),
                                    NotEqual(
                                        AccessMember(
                                            Variable(lastEntityLocalName),
                                            virtualEntity.Entity.Columns[0].PropertyName
                                        ),
                                        Variable(entityIdLocalName)
                                    )
                                ),
                                checkForExistingEntitiy
                            )
                        };

                        IEnumerable<StatementSyntax> potentialIf;

                        if (virtualEntityIndex == 0 || !virtualEntity.RequiresDBNullCheck)
                        {
                            potentialIf = potentialIfBody;
                        }
                        else
                        {
                            potentialIf = new StatementSyntax[]
                            {
                                // Check if column is null
                                If(
                                    Not(
                                        Invoke(
                                            Variable("reader"),
                                            "IsDBNull",
                                            Variable("columnIndex")
                                        )
                                    ),
                                    potentialIfBody
                                )
                            };
                        }

                        _cases.Add(
                            Case(Constant(columnOffset + columnIndex), Concat(potentialIf, Break()))
                        );

                        _miscStatements.Clear();
                    }
                    else
                    {
                        // Assign a none primary column
                        _caseStatements.Add(
                            AssignMember(
                                Variable(lastEntityLocalName),
                                column.PropertyName,
                                Invoke(
                                    Variable("reader"),
                                    GenericName("GetFieldValue", Type(column.Type)),
                                    Variable("columnIndex")
                                )
                            )
                        );

                        _caseStatements.Add(Break());

                        _cases.Add(Case(Constant(columnOffset + columnIndex), _caseStatements));
                    }

                    _caseStatements.Clear();
                }

                columnOffset += virtualEntity.Entity.Columns.Count;
            }

            for (
                var virtualEntityIndex = 0;
                virtualEntityIndex < virtualEntities.Length;
                virtualEntityIndex++
            )
            {
                var virtualEntity = virtualEntities[virtualEntityIndex];

                if (!virtualEntity.HasRelations)
                    continue;

                for (
                    var relationIndex = 0;
                    relationIndex < virtualEntity.ForeignAssignedRelations.Count;
                    relationIndex++
                )
                {
                    var (relation, foreignEntity) = virtualEntity.ForeignAssignedRelations[
                        relationIndex
                    ];

                    var entityRelationsLocalName =
                        $"entityRelations_{virtualEntity.Id}_{relation.Id}";
                    var lastEntityRelationsLocalName =
                        $"lastEntityRelations_{virtualEntity.Id}_{relation.Id}";

                    var lastRightEntityLocalName = "lastEntity_" + foreignEntity.Id;
                    var lastRightEntityIdLocalName = "entityId_" + foreignEntity.Id;

                    if (virtualEntity.RequiresDBNullCheck)
                    {
                        _afterCases.Add(
                            If(
                                IsBitSet(
                                    Variable("entityChanges"),
                                    Type(entityChangesType),
                                    virtualEntity.Id
                                ),
                                If(
                                    Equal(Variable(lastEntityRelationsLocalName), Null()),
                                    Statement(
                                        AssignLocal(
                                            Variable(lastEntityRelationsLocalName),
                                            AccessElement(
                                                Variable(entityRelationsLocalName),
                                                Variable(lastRightEntityIdLocalName)
                                            )
                                        )
                                    )
                                ),
                                If(
                                    Invoke(
                                        Variable(lastEntityRelationsLocalName),
                                        "Add",
                                        Variable("lastEntity_" + virtualEntity.Id)
                                    ),
                                    Statement(
                                        Invoke(
                                            AccessMember(
                                                Variable(lastRightEntityLocalName),
                                                relation.RightNavigationProperty!
                                            ),
                                            "Add",
                                            Variable("lastEntity_" + virtualEntity.Id)
                                        )
                                    )
                                )
                            )
                        );
                    }
                    else
                    {
                        _afterCases.Add(
                            If(
                                Or(
                                    IsBitSet(
                                        Variable("entityChanges"),
                                        Type(entityChangesType),
                                        virtualEntity.Id
                                    ),
                                    IsBitSet(
                                        Variable("entityChanges"),
                                        Type(entityChangesType),
                                        foreignEntity.Id
                                    )
                                ),
                                If(
                                    Equal(Variable(lastEntityRelationsLocalName), Null()),
                                    Statement(
                                        AssignLocal(
                                            Variable(lastEntityRelationsLocalName),
                                            AccessElement(
                                                Variable(entityRelationsLocalName),
                                                Variable(lastRightEntityLocalName)
                                            )
                                        )
                                    )
                                ),
                                If(
                                    Invoke(
                                        Variable(lastEntityRelationsLocalName),
                                        "Add",
                                        Variable("lastEntity_" + virtualEntity.Id)
                                    ),
                                    Statement(
                                        Invoke(
                                            AccessMember(
                                                Variable(lastRightEntityLocalName),
                                                relation.RightNavigationProperty!
                                            ),
                                            "Add",
                                            Variable("lastEntity_" + virtualEntity.Id)
                                        )
                                    )
                                )
                            )
                        );
                    }
                }

                if (virtualEntity.SelfAssignedRelations.Count > 0)
                {
                    for (
                        var relationIndex = 0;
                        relationIndex < virtualEntity.SelfAssignedRelations.Count;
                        relationIndex++
                    )
                    {
                        var (relation, foreignEntity) = virtualEntity.SelfAssignedRelations[
                            relationIndex
                        ];

                        _miscStatements.Add(
                            AssignMember(
                                Variable("lastEntity_" + virtualEntity.Id),
                                relation.LeftNavigationProperty!,
                                Variable("lastEntity_" + foreignEntity.Id)
                            )
                        );
                    }

                    _afterCases.Add(
                        If(
                            IsBitSet(
                                Variable("entityChanges"),
                                Type(entityChangesType),
                                virtualEntity.Id
                            ),
                            _miscStatements
                        )
                    );

                    _miscStatements.Clear();
                }
            }

            var methodDefinition = new MethodLocation(
                "Reflow.QueryParsers." + _className,
                "Parser_" + _parserIndex++
            );

            _iterationLocalSyntaxis.Add(
                Local("entityChanges", Type(entityChangesType)).WithInitializer(Constant(0))
            );

            _parsers.Add(
                Method(
                        methodDefinition.MethodName,
                        GenericType(typeof(Task<>), Type(query.Entity)),
                        CSharpModifiers.Internal | CSharpModifiers.Static | CSharpModifiers.Async
                    )
                    .WithParameters(
                        Parameter("reader", Type(typeof(DbDataReader))),
                        Parameter("columns", Array(Type(typeof(ushort))))
                    )
                    .WithStatements(
                        Concat(
                            _localSyntaxis,
                            Local("startColumnIndex", Var()).WithInitializer(Constant(0)),
                            Local("columnCount", Var())
                                .WithInitializer(AccessMember(Variable("columns"), "Length")),
                            While(
                                Await(Invoke(Variable("reader"), "ReadAsync")),
                                _iterationLocalSyntaxis.Concat(
                                    Concat(
                                        For(
                                            Local("columnIndex", Var())
                                                .WithInitializer(Variable("startColumnIndex")),
                                            LessThen(
                                                Variable("columnIndex"),
                                                Variable("columnCount")
                                            ),
                                            Increment(Variable("columnIndex")),
                                            Switch(
                                                AccessElement(
                                                    Variable("columns"),
                                                    Variable("columnIndex")
                                                ),
                                                _cases
                                            )
                                        ),
                                        _afterCases
                                    )
                                )
                            ),
                            Return(Variable("lastEntity_0"))
                        )
                    )
            );

            return methodDefinition;
        }

        private MethodLocation BuildManyNoRelationParser(Query query)
        {
            var entity = _database.Entities[query.Entity];

            for (var columnIndex = 0; columnIndex < entity.Columns.Count; columnIndex++)
            {
                var column = entity.Columns[columnIndex];

                if (columnIndex == 0)
                {
                    _caseStatements.Add(
                        Statement(
                            AssignLocal(Variable("lastEntity"), Instance(Type(entity.Symbol)))
                        )
                    );

                    _caseStatements.Add(
                        Statement(Invoke(Variable("entities"), "Add", Variable("lastEntity")))
                    );
                }

                _caseStatements.Add(
                    AssignMember(
                        Variable("lastEntity"),
                        column.PropertyName,
                        Invoke(
                            Variable("reader"),
                            GenericName("GetFieldValue", Type(column.Type)),
                            Variable("columnIndex")
                        )
                    )
                );
                _caseStatements.Add(Break());

                _cases.Add(Case(Constant(columnIndex), _caseStatements));
                _caseStatements.Clear();
            }

            var methodDefinition = new MethodLocation(
                "Reflow.QueryParsers." + _className,
                "Parser_" + _parserIndex++
            );

            _parsers.Add(
                Method(
                        methodDefinition.MethodName,
                        GenericType(
                            typeof(Task<>),
                            GenericType(typeof(IList<>), Type(query.Entity))
                        ),
                        CSharpModifiers.Internal | CSharpModifiers.Static | CSharpModifiers.Async
                    )
                    .WithParameters(
                        Parameter("reader", Type(typeof(DbDataReader))),
                        Parameter("columns", Array(Type(typeof(ushort))))
                    )
                    .WithStatements(
                        Local("entities", GenericType(typeof(List<>), Type(query.Entity)))
                            .WithInitializer(
                                Instance(GenericType(typeof(List<>), Type(query.Entity)))
                            ),
                        Local("lastEntity", Type(query.Entity)).WithInitializer(Null()),
                        Local("columnCount", Var())
                            .WithInitializer(AccessMember(Variable("columns"), "Length")),
                        While(
                            Await(Invoke(Variable("reader"), "ReadAsync")),
                            For(
                                Local("columnIndex", Type(typeof(int)))
                                    .WithInitializer(Constant(0)),
                                LessThen(Variable("columnIndex"), Variable("columnCount")),
                                Increment(Variable("columnIndex")),
                                Switch(
                                    AccessElement(Variable("columns"), Variable("columnIndex")),
                                    _cases
                                )
                            )
                        ),
                        Return(Variable("entities"))
                    )
            );

            return methodDefinition;
        }

        private VirtualEntity[] GetVirtualEntities(
            RelationBuilderValues relationBuilderValues,
            Entity rootEntity
        )
        {
            return new VirtualEntitySorter(_database, relationBuilderValues).GenerateSortedEntities(
                rootEntity
            );
        }

        internal static SourceText Emit(Database database, List<Query> queries)
        {
            return new QueryParserEmitter(database, queries).Build();
        }

        private class QueryEqualityComparer : IEqualityComparer<Query>
        {
            internal static QueryEqualityComparer Default = new();

            private QueryEqualityComparer() { }

            bool IEqualityComparer<Query>.Equals(Query x, Query y)
            {
                var xPath = x.JoinedEntities.FlattenedPath;
                var yPath = y.JoinedEntities.FlattenedPath;

                if (
                    x.TrackChanges != y.TrackChanges
                    || x.Type != y.Type
                    || xPath.Count != yPath.Count
                    || !x.Entity.Equals(y.Entity, SymbolEqualityComparer.Default)
                )
                {
                    return false;
                }

                for (var entityIndex = 0; entityIndex < xPath.Count; entityIndex++)
                {
                    if (xPath[entityIndex].Equals(yPath[entityIndex]))
                    {
                        return false;
                    }
                }

                return true;
            }

            int IEqualityComparer<Query>.GetHashCode(Query obj)
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.TrackChanges);
                hashCode.Add(obj.Entity);
                hashCode.Add(obj.Type);

                var path = obj.JoinedEntities.FlattenedPath;

                for (var entityIndex = 0; entityIndex < path.Count; entityIndex++)
                {
                    hashCode.Add(path[entityIndex]);
                }

                return hashCode.ToHashCode();
            }
        }

        private class VirtualEntity
        {
            internal Entity Entity { get; }
            internal List<(EntityRelation, VirtualEntity)> SelfAssignedRelations { get; }
            internal List<(EntityRelation, VirtualEntity)> ForeignAssignedRelations { get; }
            internal List<(EntityRelation, VirtualEntity)> InitializeRelationMaps { get; }
            internal List<EntityRelation> InitializeNavigations { get; }

            internal bool HasRelations =>
                SelfAssignedRelations.Count > 0 || ForeignAssignedRelations.Count > 0;
            internal bool RequiresChangedLocal { get; set; }
            internal bool RequiresDBNullCheck { get; set; }
            internal int Id { get; set; }

            internal VirtualEntity(Entity entity)
            {
                SelfAssignedRelations = new();
                ForeignAssignedRelations = new();
                InitializeRelationMaps = new();
                InitializeNavigations = new();

                Entity = entity;
            }
        }

        private class VirtualEntitySorter
        {
            private readonly LinkedList<VirtualEntity> _entities;
            private readonly Database _database;
            private readonly RelationBuilderValues _relationBuilderValues;

            internal VirtualEntitySorter(
                Database database,
                RelationBuilderValues relationBuilderValues
            )
            {
                _database = database;
                _relationBuilderValues = relationBuilderValues;
                _entities = new();
            }

            internal VirtualEntity[] GenerateSortedEntities(Entity rootEntity)
            {
                var virtualEntity = new VirtualEntity(rootEntity);

                _entities.AddFirst(virtualEntity);

                for (var i = 0; i < _relationBuilderValues.TrailingPath.Count; i++)
                {
                    BaseCompile(_relationBuilderValues.TrailingPath[i], virtualEntity);
                }

                var entities = new VirtualEntity[_entities.Count];

                var index = 0;

                for (var entry = _entities.First; entry is not null; entry = entry.Next)
                {
                    entry.Value.Id = index;
                    entities[index++] = entry.Value;
                }

                return entities;
            }

            private void BaseCompile(RelationPath path, VirtualEntity rightVirtualEntity)
            {
                var relation = GetRelationFromPath(path).Mirror;

                var leftVirtualEntity = new VirtualEntity(
                    _database.Entities[relation.LeftEntitySymbol]
                );

                _entities.AddLast(leftVirtualEntity);

                if (relation.RelationType == RelationType.ManyToOne)
                {
                    if (!relation.IsRightNavigationPropertyInitialized)
                    {
                        rightVirtualEntity.InitializeNavigations.Add(relation.Mirror);
                    }

                    leftVirtualEntity.ForeignAssignedRelations.Add((relation, rightVirtualEntity));
                    rightVirtualEntity.InitializeRelationMaps.Add(
                        (relation.Mirror, leftVirtualEntity)
                    );

                    rightVirtualEntity.RequiresChangedLocal = true;
                    leftVirtualEntity.RequiresChangedLocal = true;

                    leftVirtualEntity.RequiresDBNullCheck = true;
                }
                else
                {
                    rightVirtualEntity.SelfAssignedRelations.Add(
                        (relation.Mirror, leftVirtualEntity)
                    );
                    rightVirtualEntity.RequiresChangedLocal = true;

                    if (
                        relation.RelationType == RelationType.OneToOne
                        && relation.IsRightNavigationPropertyNullable
                    )
                    {
                        leftVirtualEntity.RequiresDBNullCheck = true;
                    }
                    else if (relation.RelationType == RelationType.OneToMany)
                    {
                        leftVirtualEntity.RequiresDBNullCheck = true;
                    }
                }

                if (relation.LeftNavigationProperty is not null)
                {
                    if (relation.RelationType == RelationType.OneToMany)
                    {
                        if (!relation.IsLeftNavigationPropertyInitialized)
                        {
                            leftVirtualEntity.InitializeNavigations.Add(relation);
                        }

                        rightVirtualEntity.ForeignAssignedRelations.Add(
                            (relation.Mirror, leftVirtualEntity)
                        );
                        leftVirtualEntity.InitializeRelationMaps.Add(
                            (relation, rightVirtualEntity)
                        );

                        rightVirtualEntity.RequiresChangedLocal = true;
                        leftVirtualEntity.RequiresChangedLocal = true;

                        rightVirtualEntity.RequiresDBNullCheck = true;
                    }
                    else
                    {
                        leftVirtualEntity.SelfAssignedRelations.Add((relation, rightVirtualEntity));
                        leftVirtualEntity.RequiresChangedLocal = true;

                        if (
                            relation.RelationType == RelationType.OneToOne
                            && relation.IsLeftNavigationPropertyNullable
                        )
                        {
                            rightVirtualEntity.RequiresDBNullCheck = true;
                        }
                        else if (relation.RelationType == RelationType.OneToMany)
                        {
                            rightVirtualEntity.RequiresDBNullCheck = true;
                        }
                    }
                }

                for (var i = 0; i < path.TrailingPath.Count; i++)
                {
                    BaseCompile(path.TrailingPath[i], leftVirtualEntity);
                }
            }

            private EntityRelation GetRelationFromPath(RelationPath path)
            {
                if (!_database.Entities.TryGetValue(path.LeftEntitySymbol, out var entity))
                {
                    throw new InvalidOperationException();
                }

                for (var relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
                {
                    var relation = entity.Relations[relationIndex];

                    if (
                        relation.LeftNavigationProperty is not null
                        && relation.LeftNavigationProperty.Equals(
                            path.NavigationSymbol,
                            SymbolEqualityComparer.Default
                        )
                        && relation.LeftEntitySymbol.Equals(
                            path.LeftEntitySymbol,
                            SymbolEqualityComparer.Default
                        )
                        && relation.RightEntitySymbol.Equals(
                            path.RightEntitySymbol,
                            SymbolEqualityComparer.Default
                        )
                    )
                    {
                        return relation;
                    }
                }

                throw new InvalidOperationException();
            }
        }
    }
}
