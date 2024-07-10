﻿using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaLocalDateTimeTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Document_Now()
        {
            Can_Use_NodaTime_LocalDateTime_In_Document(NodaUtil.LocalDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Document_IsoMin()
        {
            Can_Use_NodaTime_LocalDateTime_In_Document(NodaUtil.LocalDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Document_IsoMax()
        {
            Can_Use_NodaTime_LocalDateTime_In_Document(NodaUtil.LocalDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Document(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(ldt, foo.LocalDateTime);
                }

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand(new DocumentConventions(), "foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expected = ldt.ToString(NodaUtil.LocalDateTime.FullIsoPattern.PatternText, null);
                    json.TryGet("LocalDateTime", out string value);
                    Assert.Equal(expected, value);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index_Now()
        {
            Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index1(NodaUtil.LocalDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index_IsoMin()
        {
            Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index1(NodaUtil.LocalDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index_IsoMax()
        {
            Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index2(NodaUtil.LocalDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index1(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt + Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt + Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime > ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime > results2[1].LocalDateTime);
                    
                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime >= ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime > results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime > results3[2].LocalDateTime);
                }
            }
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index2(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt - Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt - Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime < ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime < results2[1].LocalDateTime);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime <= ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime < results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime < results3[2].LocalDateTime);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Static_Index_Now()
        {
            Can_Use_NodaTime_LocalDateTime_In_Static_Index1(NodaUtil.LocalDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Static_Index_IsoMin()
        {
            Can_Use_NodaTime_LocalDateTime_In_Static_Index1(NodaUtil.LocalDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Static_Index_IsoMax()
        {
            Can_Use_NodaTime_LocalDateTime_In_Static_Index2(NodaUtil.LocalDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Static_Index1(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt + Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt + Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime > ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime > results2[1].LocalDateTime);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime >= ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime > results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime > results3[2].LocalDateTime);
                }
            }
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Static_Index2(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt - Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt - Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime < ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime < results2[1].LocalDateTime);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime <= ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime < results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime < results3[2].LocalDateTime);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public LocalDateTime LocalDateTime { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.LocalDateTime
                              };
            }
        }
    }
}
