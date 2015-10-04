﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerEnabledDbContext.Common.Configuration;
using TrackerEnabledDbContext.Common.Testing;
using TrackerEnabledDbContext.Common.Testing.Extensions;
using TrackerEnabledDbContext.Common.Testing.Models;

namespace TrackerEnabledDbContext.IntegrationTests
{
    [TestClass]
    public class FluentConfigurationTests : PersistanceTests<TestTrackerContext>
    {
        [TestMethod]
        public void Can_recognise_global_tracking_indicator_when_disabled()
        {
            GlobalTrackingConfig.Enabled = false;
            EntityTrackingConfiguration
                .Configure<POCO>()
                .EnableTracking();

            POCO model = ObjectFactory<POCO>.Create();
            db.POCOs.Add(model);
            db.SaveChanges();

            model.AssertNoLogs(db, model.Id);
        }

        [TestMethod]
        public void Can_recognise_global_tracking_indicator_when_enabled()
        {
            EntityTrackingConfiguration.Configure<POCO>().EnableTracking();

            POCO model = new POCO
            {
                Color = "Red",
                Height = 67.4,
                StartTime = new DateTime(2015, 5, 5)
            };

            db.POCOs.Add(model);
            db.SaveChanges();

            model.AssertAuditForAddition(db, model.Id, null,
                new KeyValuePair<string, string>("Color", model.Color),
                new KeyValuePair<string, string>("Id", model.Id.ToString()),
                new KeyValuePair<string, string>("Height", model.Height.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string,string>("StartTime", model.StartTime.ToString()));
        }

        [TestMethod]
        public async Task Can_Override_annotation_based_configuration_for_entity_skipTracking()
        {
            var model = new NormalModel();
            EntityTrackingConfiguration
                .Configure<NormalModel>()
                .DisableTracking();

            string userName = RandomText;

            db.NormalModels.Add(model);
            await db.SaveChangesAsync(userName);

            model.AssertNoLogs(db,model.Id);
        }

        [TestMethod]
        public void Can_Override_annotation_based_configuration_for_property()
        {
            var model = new TrackedModelWithMultipleProperties
            {
                Category = RandomChar,
                Description = RandomText, //skipped
                IsSpecial = true,
                StartDate = new DateTime(2015, 5, 5), //skipped
                Name = RandomText,
                Value = RandomNumber //skipped
            };

            EntityTrackingConfiguration
                .Configure<TrackedModelWithMultipleProperties>()
                //enable tracking for value
                .ConfigureProperties()
                .TrackProperty(x => x.Value)
                //disable for description
                .SkipProperty(x => x.Description);


            //string userName = RandomText;

            //db.NormalModels.Add(model);
            //await db.SaveChangesAsync(userName);

            //model.AssertNoLogs(db, model.Id);
        }
    }
}
