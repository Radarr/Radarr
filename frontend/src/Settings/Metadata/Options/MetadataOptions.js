import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

// Note: Do Not Translate Certification Countries

export const certificationCountryOptions = [
  { key: 'au', value: 'Australia' },
  { key: 'br', value: 'Brazil' },
  { key: 'ca', value: 'Canada' },
  { key: 'fr', value: 'France' },
  { key: 'de', value: 'Germany' },
  { key: 'gb', value: 'Great Britain' },
  { key: 'it', value: 'Italy' },
  { key: 'es', value: 'Spain' },
  { key: 'us', value: 'United States' },
  { key: 'nz', value: 'New Zealand' }
];

function MetadataOptions(props) {
  const {
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  return (
    <FieldSet legend={translate('Options')}>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <div>
            {translate('UnableToLoadIndexerOptions')}
          </div>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormGroup>
              <FormLabel>{translate('CertificationCountry')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="certificationCountry"
                values={certificationCountryOptions}
                onChange={onInputChange}
                helpText={translate('CertificationCountryHelpText')}
                {...settings.certificationCountry}
              />
            </FormGroup>
          </Form>
      }
    </FieldSet>
  );
}

MetadataOptions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default MetadataOptions;
