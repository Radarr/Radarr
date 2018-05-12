﻿using System.Collections.Generic;
using System.Data;
 using System.Linq;
 using FluentMigrator;
 using Marr.Data.QGen;
 using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
 using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(147)]
    public class add_custom_formats : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Execute.WithConnection(RenameUrlToBaseUrl);
            Create.TableForModel("CustomFormats")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("FormatTags").AsString();

            Execute.WithConnection(AddCustomFormatsToProfile);
        }

        private void AddCustomFormatsToProfile(IDbConnection conn, IDbTransaction tran)
        {

        }
    }
}
