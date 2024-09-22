import React from 'react';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

interface AvailabilitySelectInputProps {
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed?: boolean;
}

interface IMovieAvailabilityOption {
  key: string;
  value: string;
  format?: string;
  isDisabled?: boolean;
}

const movieAvailabilityOptions: IMovieAvailabilityOption[] = [
  {
    key: 'announced',
    get value() {
      return translate('Announced');
    },
  },
  {
    key: 'inCinemas',
    get value() {
      return translate('InCinemas');
    },
  },
  {
    key: 'released',
    get value() {
      return translate('Released');
    },
  },
];

function AvailabilitySelectInput(props: AvailabilitySelectInputProps) {
  const values = [...movieAvailabilityOptions];

  const {
    includeNoChange = false,
    includeNoChangeDisabled = true,
    includeMixed = false,
  } = props;

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      value: translate('NoChange'),
      isDisabled: includeNoChangeDisabled,
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: `(${translate('Mixed')})`,
      isDisabled: true,
    });
  }

  return <EnhancedSelectInput {...props} values={values} />;
}

export default AvailabilitySelectInput;
