import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import FieldSet from 'Components/FieldSet';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';

class MoreInfo extends Component {

  //
  // Render

  render() {
    return (
      <FieldSet legend="More Info">
        <DescriptionList>
          <DescriptionListItemTitle>Home page</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://readarr.com/">readarr.com</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Wiki</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/readarr/Readarr/wiki">wiki.readarr.com</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Reddit</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://www.reddit.com/r/Readarr/">Readarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Discord</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://discord.gg/Rt6gQdB">#readarr on Discord</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Donations</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://opencollective.com/readarr">Donate to Readarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Sonarr Donations</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://sonarr.tv/donate">Donate to Sonarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Source</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/readarr/Readarr/">github.com/Readarr/Readarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>Feature Requests</DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/readarr/Readarr/issues">github.com/Readarr/Readarr/issues</Link>
          </DescriptionListItemDescription>

        </DescriptionList>
      </FieldSet>
    );
  }
}

MoreInfo.propTypes = {

};

export default MoreInfo;
