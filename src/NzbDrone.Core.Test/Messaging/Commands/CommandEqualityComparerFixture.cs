﻿using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Test.Messaging.Commands
{
    [TestFixture]
    public class CommandEqualityComparerFixture
    {
        [Test]
        public void should_return_true_when_there_are_no_properties()
        {
            var command1 = new DownloadedMoviesScanCommand();
            var command2 = new DownloadedMoviesScanCommand();

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_single_property_matches()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int>{ 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_multiple_properties_match()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_single_property_doesnt_match()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 2 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_only_one_property_matches()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_no_properties_match()
        {
            var command1 = new SeasonSearchCommand { SeriesId = 1, SeasonNumber = 1 };
            var command2 = new SeasonSearchCommand { SeriesId = 2, SeasonNumber = 2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_only_one_has_properties()
        {
            var command1 = new SeasonSearchCommand();
            var command2 = new SeasonSearchCommand { SeriesId = 2, SeasonNumber = 2 };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_only_one_has_null_property()
        {
            var command1 = new EpisodeSearchCommand(null);
            var command2 = new EpisodeSearchCommand(new List<int>());

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_commands_are_diffrent_types()
        {
            CommandEqualityComparer.Instance.Equals(new RssSyncCommand(), new ApplicationUpdateCommand()).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_commands_list_are_different_lengths()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1, 2 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_commands_list_dont_match()
        {
            var command1 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 1 } };
            var command2 = new EpisodeSearchCommand { EpisodeIds = new List<int> { 2 } };

            CommandEqualityComparer.Instance.Equals(command1, command2).Should().BeFalse();
        }
    }
}
