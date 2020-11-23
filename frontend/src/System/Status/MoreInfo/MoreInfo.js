import React, { Component } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import FieldSet from 'Components/FieldSet';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';

class MoreInfo extends Component {

  //
  // Render

  render() {
    return (
      <FieldSet legend={translate('MoreInfo')}>
        <DescriptionList>
          <DescriptionListItemTitle>
            {translate('HomePage')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://radarr.video/">radarr.video</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Discord')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://discord.gg/r5wJPt9">discord.gg/r5wJPt9</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Wiki')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://wiki.servarr.com/Radarr">wiki.servarr.com/Radarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Donations')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://opencollective.com/radarr">opencollective.com/radarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('Source')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/Radarr/Radarr/">github.com/Radarr/Radarr</Link>
          </DescriptionListItemDescription>

          <DescriptionListItemTitle>
            {translate('FeatureRequests')}
          </DescriptionListItemTitle>
          <DescriptionListItemDescription>
            <Link to="https://github.com/Radarr/Radarr/issues">github.com/Radarr/Radarr/issues</Link>
          </DescriptionListItemDescription>

        </DescriptionList>
      </FieldSet>
    );
  }
}

MoreInfo.propTypes = {

};

export default MoreInfo;
