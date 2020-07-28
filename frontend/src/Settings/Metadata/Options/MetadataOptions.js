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

export const certificationCountryOptions = [
  { key: 'us', value: 'United States' },
  { key: 'gb', value: 'Great Britain' }
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
          <div>Unable to load indexer options</div>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormGroup>
              <FormLabel>Certification Country</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="certificationCountry"
                values={certificationCountryOptions}
                onChange={onInputChange}
                helpText="Select Country for Movie Certifications"
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
