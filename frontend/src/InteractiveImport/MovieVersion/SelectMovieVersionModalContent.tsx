import React, { useCallback, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, scrollDirections } from 'Helpers/Props';
import MovieVersion from 'Movie/Details/Versions/MovieVersion';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

export interface SelectMovieVersionModalContentProps {
  movieVersionId: number;
  movieVersions: MovieVersion[];
  modalTitle: string;
  onMovieVersionSelect(movieVersionId: number): void;
  onModalClose(): void;
}

// The primary version is sent as 0, like files that aren't attached to an additional version
export function getMovieVersionKey(version: MovieVersion) {
  return version.isPrimary ? 0 : version.id;
}

function SelectMovieVersionModalContent({
  movieVersions,
  modalTitle,
  onMovieVersionSelect,
  onModalClose,
  ...otherProps
}: SelectMovieVersionModalContentProps) {
  const [movieVersionId, setMovieVersionId] = useState(
    otherProps.movieVersionId
  );

  const qualityProfiles = useSelector(
    (state: AppState) => state.settings.qualityProfiles.items
  );

  const values = useMemo(() => {
    return movieVersions.map((version) => {
      const profile = qualityProfiles.find(
        (p) => p.id === version.qualityProfileId
      );

      return {
        key: getMovieVersionKey(version),
        value: profile?.name ?? translate('Unknown'),
        hint: version.path,
      };
    });
  }, [movieVersions, qualityProfiles]);

  const handleChange = useCallback(
    ({ value }: InputChanged<string | number>) => {
      setMovieVersionId(Number(value));
    },
    []
  );

  const handleSelectPress = useCallback(() => {
    onMovieVersionSelect(movieVersionId);
  }, [movieVersionId, onMovieVersionSelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SetMovieVersionModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.NONE}>
        <Form>
          <FormGroup>
            <FormLabel>{translate('Version')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="movieVersionId"
              value={movieVersionId}
              values={values}
              helpText={translate('MovieVersionHelpText')}
              onChange={handleChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.SUCCESS} onPress={handleSelectPress}>
          {translate('SetMovieVersion')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectMovieVersionModalContent;
