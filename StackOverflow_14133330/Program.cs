using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Driver;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;

namespace StackOverflow_14133330
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to continue ...");
            Console.Read();
        }

        static void Execute()
        {
            //Mapper
            var mapper = new ModelMapper();
            MapVisualFeed(mapper);
            MapVideoFeed(mapper);
            MapPlaylistAssignment(mapper);
            var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            //Configuration
            var configuration = new Configuration();
            configuration.DataBaseIntegration(x =>
            {
                x.Dialect<MsSql2008Dialect>();
                x.Driver<Sql2008ClientDriver>();
                x.AutoCommentSql = true;
                x.SchemaAction = SchemaAutoAction.Validate;
                x.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                x.ConnectionString = "Data Source=localhost;Initial Catalog=Test;Integrated Security=true";
                x.PrepareCommands = false;
                x.BatchSize = short.MaxValue;
            });
            configuration.AddMapping(mapping);

            //Export Schema
            new SchemaExport(configuration).Drop(false, true);
            new SchemaExport(configuration).Create(false, true);

            //Session Factory
            var sf = configuration.BuildSessionFactory();

            //Insert
            using (var session = sf.OpenSession())
            {
                var video = new VideoFeed();
                var playlist = new PlaylistAssignment() { VideoFeed = video };

                video.PlaylistAssignments = new Iesi.Collections.Generic.HashedSet<PlaylistAssignment>();
                video.PlaylistAssignments.Add(playlist);

                session.Save(video);
                session.Flush();
            }

            //Query
            using (var session = sf.OpenSession())
            {
                var videos = session.QueryOver<VideoFeed>().List();
                var playlists = videos.SelectMany(x => x.PlaylistAssignments);
            }
        }

        static void MapVisualFeed(ModelMapper mapper)
        {
            mapper.Class<VisualFeed>(x =>
            {
                x.Id(k => k.Id, m => m.Generator(Generators.Identity));
            });
        }
        static void MapVideoFeed(ModelMapper mapper)
        {
            mapper.JoinedSubclass<VideoFeed>(
            joinedSubClassMapper =>
            {
                joinedSubClassMapper.Table("cms_VideoFeed");
                joinedSubClassMapper.Key(keyMapper =>
                {
                    keyMapper.Column("Id");
                }
                );


                joinedSubClassMapper.Set(
                    playerGroup => playerGroup.PlaylistAssignments,
                    setPropertiesMapper =>
                    {
                        setPropertiesMapper.Key(
                            keyMapper =>
                            {
                                keyMapper.Column("VideoFeed_Id");
                                //keyMapper.PropertyRef(videoFeed => videoFeed.Id);
                            }
                        );
                        setPropertiesMapper.Cascade(Cascade.All | Cascade.DeleteOrphans);
                        setPropertiesMapper.Inverse(true);
                        //setPropertiesMapper.OrderBy(playlistAssignment => playlistAssignment.AssignmentRank);
                    },
                    collectionElementRelation =>
                    {
                        collectionElementRelation.OneToMany();
                    }
                );
            }
            );

        }
        static void MapPlaylistAssignment(ModelMapper mapper)
        {

            mapper.Class<PlaylistAssignment>(
                    classMapper =>
                    {
                        classMapper.Table("cms_PlaylistAssignment");
                        classMapper.Id(
                            playlistAssignment => playlistAssignment.Id,
                            idMapper =>
                            {
                                idMapper.Generator(Generators.Identity);
                            }
                        );

                        classMapper.ManyToOne(
                                pa => pa.VideoFeed,
                                manyToOneMapper =>
                                {
                                    manyToOneMapper.Column("VideoFeed_Id");
                                    manyToOneMapper.Lazy(LazyRelation.Proxy);
                                }
                            );
                    });
        }
    }
}
