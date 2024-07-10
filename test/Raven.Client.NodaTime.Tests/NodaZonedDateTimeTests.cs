﻿using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaZonedDateTimeTests : MyRavenTestDriver
    {
        // NOTE: Tests are intentionally omited for very early dates.
        //       This is because most of the timezones did not exist then, so their values
        //       are meaningless.  ZonedDateTime is only for values that are actually
        //       valid at some point in the time zone's history.


        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Document_Now()
        {
            var instant = SystemClock.Instance.GetCurrentInstant();
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var zdt = new ZonedDateTime(instant, zone);
            Can_Use_NodaTime_ZonedDateTime_In_Document(zdt);
        }

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Document_NearIsoMax()
        {
            var instant = NodaUtil.Instant.MaxIsoValue - Duration.FromHours(24);
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var zdt = new ZonedDateTime(instant, zone);
            Can_Use_NodaTime_ZonedDateTime_In_Document(zdt);
        }

        private void Can_Use_NodaTime_ZonedDateTime_In_Document(ZonedDateTime zdt)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", ZonedDateTime = zdt });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(zdt, foo.ZonedDateTime);
                }

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand( new DocumentConventions(), "foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expectedDateTime = zdt.ToDateTimeOffset().ToString("o");
                    var expectedZone = zdt.Zone.Id;
                    json.TryGetMember("ZonedDateTime", out var obj);
                    var bInterval = obj as BlittableJsonReaderObject;
                    bInterval.TryGet("OffsetDateTime", out string value1);
                    bInterval.TryGet("Zone", out string value2);
                    Assert.Equal(expectedDateTime, value1);
                    Assert.Equal(expectedZone, value2);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index_Now()
        {
            var instant = SystemClock.Instance.GetCurrentInstant();
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index(new ZonedDateTime(instant, zone));
        }

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index_NearIsoMax()
        {
            var instant = NodaUtil.Instant.MaxIsoValue - Duration.FromHours(24);
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index(new ZonedDateTime(instant, zone));
        }

        private void Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index(ZonedDateTime zdt)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", ZonedDateTime = zdt });
                    session.Store(new Foo { Id = "foos/2", ZonedDateTime = zdt - Duration.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", ZonedDateTime = zdt - Duration.FromMinutes(2) });
                    session.SaveChanges();
                }

                //WaitForUserToContinueTheTest(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    // .ToInstant() is required for dynamic query.  See comments in the static index for an alternative.

                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.ZonedDateTime.ToInstant() == zdt.ToInstant());
                    System.Diagnostics.Debug.WriteLine(q1);
                    var results1 = q1.ToList();
                    //WaitForUserToContinueTheTest(documentStore);
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.ZonedDateTime.ToInstant() < zdt.ToInstant())
                                    .OrderBy(x => x.ZonedDateTime.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(ZonedDateTime.Comparer.Local.Compare(results2[0].ZonedDateTime, results2[1].ZonedDateTime) < 0);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.ZonedDateTime.ToInstant() <= zdt.ToInstant())
                                    .OrderBy(x => x.ZonedDateTime.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(ZonedDateTime.Comparer.Local.Compare(results3[0].ZonedDateTime, results3[1].ZonedDateTime) < 0);
                    Assert.True(ZonedDateTime.Comparer.Local.Compare(results3[1].ZonedDateTime, results3[2].ZonedDateTime) < 0);
                }
            }
        }

        [Fact(Skip = "RavenDB can not translate Comparer.Local.Compare")]
        public void Can_Use_NodaTime_ZonedDateTime_In_Static_Index_Now()
        {
            var instant = SystemClock.Instance.GetCurrentInstant();
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            Can_Use_NodaTime_ZonedDateTime_In_Static_Index(new ZonedDateTime(instant, zone));
        }

        [Fact(Skip="RavenDB can not translate Comparer.Local.Compare")]
        public void Can_Use_NodaTime_ZonedDateTime_In_Static_Index_NearIsoMax()
        {
            var instant = NodaUtil.Instant.MaxIsoValue - Duration.FromHours(24);
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            Can_Use_NodaTime_ZonedDateTime_In_Static_Index(new ZonedDateTime(instant, zone));
        }

        private void Can_Use_NodaTime_ZonedDateTime_In_Static_Index(ZonedDateTime zdt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", ZonedDateTime = zdt });
                    session.Store(new Foo { Id = "foos/2", ZonedDateTime = zdt - Duration.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", ZonedDateTime = zdt - Duration.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.ZonedDateTime == zdt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => ZonedDateTime.Comparer.Local.Compare(x.ZonedDateTime, zdt) < 0)
                                    .OrderBy(x => x.ZonedDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(ZonedDateTime.Comparer.Local.Compare(results2[0].ZonedDateTime, results2[1].ZonedDateTime) < 0);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => ZonedDateTime.Comparer.Local.Compare(x.ZonedDateTime, zdt) <= 0)
                                    .OrderBy(x => x.ZonedDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(ZonedDateTime.Comparer.Local.Compare(results3[0].ZonedDateTime, results3[1].ZonedDateTime) < 0);
                    Assert.True(ZonedDateTime.Comparer.Local.Compare(results3[1].ZonedDateTime, results3[2].ZonedDateTime) < 0);
                }
            }
        }

        [Fact(Skip = "Raven can't compile, TODO later")]
        public void Can_Use_NodaTime_ZonedDateTime_In_Static_MapReduceIndex()
        {
            var zdt = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault());

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex2());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", ZonedDateTime = zdt });
                    session.Store(new Foo { Id = "foos/2", ZonedDateTime = zdt - Duration.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", ZonedDateTime = zdt - Duration.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var result = session.Query<TestIndex2.Result, TestIndex2>().Customize(x => x.WaitForNonStaleResults())
                        .First();
                    Assert.Equal(zdt.ToOffsetDateTime(), result.Value);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public ZonedDateTime ZonedDateTime { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  // If you map the OffsetDatetime value here, you don't need to call .ToInstant() method in the query.
                                  ZonedDateTime = foo.ZonedDateTime.AsZonedDateTime().ToOffsetDateTime().Resolve()
                              };

                AdditionalSources = new Dictionary<string, string> {
                    { "Raven.Client.NodaTime", NodaTimeCompilationExtension.AdditionalSourcesRavenBundlesNodaTime },
                    { "Raven.Client.NodaTime2", NodaTimeCompilationExtension.AdditionalSourcesNodaTime }
                };
            }
        }

        public class TestIndex2 : AbstractIndexCreationTask<Foo, TestIndex2.Result>
        {
            public class Result
            {
                public OffsetDateTime Value { get; set; }
            }

            public TestIndex2()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  Value = foo.ZonedDateTime.AsZonedDateTime().ToOffsetDateTime().Resolve()
                              };

                Reduce = results => from result in results
                                    group result by 0
                                    into g
                                    select new
                                    {
                                        Value = g.Max(x => x.Value)
                                    };


                AdditionalSources = new Dictionary<string, string> {
                    { "Raven.Client.NodaTime", NodaTimeCompilationExtension.AdditionalSourcesRavenBundlesNodaTime },
                    { "Raven.Client.NodaTime2", NodaTimeCompilationExtension.AdditionalSourcesNodaTime }
                };
            }
        }
    }
}
